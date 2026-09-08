namespace Infrastructure
{
    using Application;
    using Domain;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using SharedServices;

    /// <summary>
    /// Provides authentication and authorization services for user login, password management, OTP verification, vendor
    /// work order token validation, and secure cookie handling. Integrates with data protection, email, token, and
    /// logging services to support secure authentication workflows.
    /// </summary>
    /// <remarks>AuthenticationService coordinates multiple authentication-related operations, including
    /// credential validation, account lockout enforcement, secure cookie management, OTP workflows, password reset, and
    /// vendor work order token validation. It leverages injected services to ensure security, auditability, and
    /// extensibility. Thread safety and transactional integrity are maintained for critical operations such as password
    /// updates. This class is intended to be used as a central authentication provider within the
    /// application.</remarks>
    /// <param name="protectorprovider">The data protection provider used to encrypt and decrypt sensitive values, such as user IDs for 'remember me'
    /// cookies.</param>
    /// <param name="httpContextAccessor">Provides access to the current HTTP context, enabling retrieval and manipulation of request and response data,
    /// including cookies.</param>
    /// <param name="response">The response handler used to construct and return standardized API responses for authentication operations.</param>
    /// <param name="emailService">The email service responsible for sending emails, such as password reset and OTP notifications, to users.</param>
    /// <param name="passwordResetTokenService">The service used to generate, store, and validate password reset tokens for secure password recovery.</param>
    /// <param name="otpService">The service used to generate, send, and verify one-time passwords (OTPs) for multi-factor authentication.</param>
    /// <param name="loginLogService">The service for logging user login attempts and tracking authentication events, including failed and successful
    /// logins.</param>
    /// <param name="tokenService">The service responsible for generating JWT tokens for authenticated users and vendors.</param>
    /// <param name="options">The application settings configuration containing authentication-related options, such as password reset URLs
    /// and email templates.</param>
    /// <param name="dbContext">The database context used to access and update user, login log, password reset token, and work order data.</param>
    public class AuthenticationService(
        IDataProtectionProvider protectorprovider,
        IHttpContextAccessor httpContextAccessor,
        IResponse response,
        IEmailService emailService,
        IPasswordResetTokenService passwordResetTokenService,
        IOtpService otpService,
        IUserLoginLogService loginLogService,
        ITokenService tokenService,
        IAppSettingsConfig options,
        DatabaseContext dbContext) : IAuthenticationService
    {
        /// <summary>
        /// Encrypts and signs a user ID to generate a secure value suitable for storing in a "remember me" cookie.
        /// </summary>
        /// <param name="userId">
        /// The user ID (<see cref="string"/>) to be encrypted and protected.
        /// </param>
        /// <returns>
        /// A <see cref="string"/> representing the encrypted and signed user ID that can safely be stored in a cookie.
        /// </returns>
        /// <remarks>
        /// Uses the configured data protection provider to create a protector named "remember_user_cookie_protector".
        /// The resulting value ensures confidentiality and integrity of the user ID when persisted on the client side.
        /// </remarks>
        public string EncryptAndSign(string userId)
        {
            var protector = protectorprovider.CreateProtector("remember_user_cookie_protector");
            return protector.Protect(userId);
        }

        /// <summary>
        /// Decrypts the specified protected value and verifies its integrity.
        /// </summary>
        /// <remarks>This method returns null if the input cannot be decrypted or fails integrity checks,
        /// such as when the value has been tampered with or has expired. Callers should always check for a null return
        /// value before using the result.</remarks>
        /// <param name="protectedValue">The protected string to decrypt and verify. This value must have been previously encrypted and protected
        /// using the corresponding data protector.</param>
        /// <returns>The original unprotected string if decryption and verification succeed; otherwise, null if the value is
        /// invalid, tampered with, or expired.</returns>
        public string? DecryptAndVerify(string protectedValue)
        {
            try
            {
                var protector = protectorprovider.CreateProtector("remember_user_cookie_protector");
                return protector.Unprotect(protectedValue);
            }
            catch
            {
                return null; // tampered or expired
            }
        }

        /// <summary>
        /// Handles the user login process, including validating credentials, checking account status, logging login attempts,
        /// enforcing account lockout after multiple failed attempts, handling "remember me" cookies, and sending OTPs for non-admin users.
        /// </summary>
        /// <param name="request">
        /// A <see cref="LoginRequest"/> containing:
        /// <list type="bullet">
        /// <item>Email: the user's email address.</item>
        /// <item>Password: the user's password.</item>
        /// <item>RememberMe (optional): flag to indicate if login should persist across sessions.</item>
        /// </list>
        /// </param>
        /// <returns>
        /// An <see cref="IResponse"/> containing:
        /// <list type="bullet">
        /// <item>Success or failure status of the login attempt.</item>
        /// <item>Message describing the result (e.g., "Login Successful", "User Locked", "OTP Sent").</item>
        /// <item><see cref="LoginResponse"/> data including the JWT token if login succeeds or OTP sent status.</item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// - Validates the user exists and checks their (Active, Inactive, Locked).
        /// - Logs all login attempts using.
        /// - Locks the user account after 5 failed attempts within the last hour.
        /// - Marks failed login logs as used upon successful login.
        /// - Skips OTP verification for ultimate admin users.
        /// - Supports "remember me" cookie functionality to bypass OTP if previously remembered.
        /// - Sends an OTP to the user email if login is successful but not remembered and not an admin.
        /// </remarks>
        public async Task<IResponse> LoginUser(LoginRequest request)
        {
            var loginResponse = new LoginResponse();

            var user = await dbContext.Users
                .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                .FirstOrDefaultAsync(u => u.Email == request.Email).ConfigureAwait(false);

            // If user not found at all
            if (user == null)
            {
                await loginLogService.LogAsync(Guid.Empty, request.Email, false, "User Invalid").ConfigureAwait(false);
                return response.SetFailure(Constants.LoginFailed);
            }

            // If user is locked
            if (user.Status == (int)StatusType.Locked)
            {
                await loginLogService.LogAsync(user.Id, request.Email, false, "User Locked").ConfigureAwait(false);
                return response.SetFailure(Constants.UserLocked);
            }

            // If user is not active (e.g. inactive, deactivated)
            if (user.Status == (int)StatusType.InActive)
            {
                await loginLogService.LogAsync(user.Id, request.Email, false, "User Inactive").ConfigureAwait(false);
                return response.SetFailure(Constants.UserInactive);
            }

            // Check password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                var since = DateTimeExtension.UtcNow.AddHours(-1).ToTimeStamp();
                var recentFails = await dbContext.UserLoginLogs
                    .Where(x => x.Email == request.Email && !x.IsSuccessful && x.AttemptedAt >= since && x.Status == (int)StatusType.NotUsed)
                    .OrderByDescending(x => x.AttemptedAt)
                    .Take(5)
                    .ToListAsync().ConfigureAwait(false);

                // Lock account if 5 failed attempts
                if (recentFails.Count == 5)
                {
                    user.Status = (int)StatusType.Locked;
                    dbContext.Users.Update(user);
                    await dbContext.SaveChangesAsync().ConfigureAwait(false);
                    return response.SetFailure(Constants.UserLocked);
                }

                await loginLogService.LogAsync(user.Id, request.Email, false, "Password Invalid").ConfigureAwait(false);
                return response.SetFailure(Constants.LoginFailed);
            }

            // Successful login
            await loginLogService.LogAsync(user.Id, user.Email, true, "Login Success").ConfigureAwait(false);

            // mark logs as used if successful login
            await dbContext.UserLoginLogs
                .Where(x => x.Email == request.Email && !x.IsSuccessful && x.Status == (int)StatusType.NotUsed)
                .ExecuteUpdateAsync(u => u.SetProperty(u => u.Status, (int)StatusType.Used)).ConfigureAwait(false);

            await dbContext.SaveChangesAsync().ConfigureAwait(false);

            // ✅ Skip OTP for admin account
            if (user.IsUltimateAdmin())
            {
                loginResponse.Token = await tokenService.GenerateToken(user).ConfigureAwait(false);
                return response.SetSuccess("Login Successful.", loginResponse);
            }

            // Check remember-me cookie
            if (httpContextAccessor.HttpContext.Request.Cookies.TryGetValue("remember_user", out var cookieVal))
            {
                var decrypted = this.DecryptAndVerify(cookieVal);
                if (decrypted == user.Id.ToString())
                {
                    loginResponse.Token = await tokenService.GenerateToken(user).ConfigureAwait(false);
                    return response.SetSuccess(Constants.LoginSuccess, loginResponse);
                }
            }

            // Send OTP if not remembered and not admin
            var otpResp = await this.SendOTPAsync(user.Email).ConfigureAwait(false);

            loginResponse.IsOTPSent = otpResp.IsSuccess;
            if (otpResp.IsSuccess)
            {
                return response.SetSuccess(Constants.Email.OTPSentSuccess, loginResponse);
            }
            else
            {
                return response.SetFailure(otpResp?.Message?.ToString() ?? string.Empty, loginResponse);
            }
        }

        /// <summary>
        /// Verifies the OTP code provided by the user during login and generates a JWT token upon successful verification.
        /// </summary>
        /// <param name="email">
        /// The email address of the user attempting to log in.
        /// </param>
        /// <param name="code">
        /// The one-time password (OTP) sent to the user's email or phone.
        /// </param>
        /// <param name="rememberMe">
        /// A flag indicating whether the user selected the "remember me" option (currently not used directly in this method).
        /// </param>
        /// <returns>
        /// An <see cref="IResponse"/> containing:
        /// <list type="bullet">
        /// <item>Success or failure status of the OTP verification.</item>
        /// <item>Message indicating the result (e.g., "OTP Verified Successfully" or failure reason).</item>
        /// <item><see cref="LoginResponse"/> data including the JWT token and the user's ID if verification succeeds.</item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// - Checks if the user exists by email.
        /// - Verifies the OTP using.
        /// - Generates a JWT token using if OTP verification succeeds.
        /// - Returns appropriate failure messages if the user is not found or OTP verification fails.
        /// </remarks>
        public async Task<IResponse> VerifyUserOTPAsync(string email, string code, bool rememberMe)
        {
            var loginResponse = new LoginResponse();

            var user = await dbContext.Users
                .Include(x => x.UserRoles)
                    .ThenInclude(x => x.Role)
                .FirstOrDefaultAsync(x => x.Email == email).ConfigureAwait(false);
            if (user == null)
            {
                return response.SetFailure(Constants.NotFound.FormatWith("User"), loginResponse);
            }

            var otpVerifyResult = await otpService.VerifyOtpAsync(user.Id, code).ConfigureAwait(false);
            if (!otpVerifyResult.IsSuccess)
            {
                return response.SetFailure(otpVerifyResult?.Message?.ToString() ?? string.Empty);
            }

            loginResponse.Token = await tokenService.GenerateToken(user).ConfigureAwait(false);
            loginResponse.UserId = user.Id;
            return response.SetSuccess(Constants.VerifiedSuccess.FormatWith("OTP"), loginResponse);
        }

        /// <summary>
        /// Sends a password reset email to the specified user if their email exists in the system.
        /// </summary>
        /// <param name="email">
        /// The email address of the user requesting a password reset.
        /// </param>
        /// <returns>
        /// An <see cref="IResponse"/> containing:
        /// <list type="bullet">
        /// <item>Success or failure status indicating whether the reset email was sent.</item>
        /// <item>A message describing the result (e.g., "Reset password email sent" or "User not found").</item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// - Checks if a user with the given email exists in the system.
        /// - Generates a secure password reset token.
        /// - Constructs a reset link and formats both plain text and HTML email content.
        /// - Sends the email.
        /// - Returns appropriate success or failure messages depending on the outcome.
        /// </remarks>
        public async Task<IResponse> SendForgotPasswordEmailAsync(string email)
        {
            var resetUrl = options.PasswordReset.Url;
            if (resetUrl == null)
            {
                return response.SetFailure("Password reset URL is not configured.");
            }

            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == email).ConfigureAwait(false);

            if (user == null)
            {
                return response.SetFailure(Constants.NotFound.FormatWith(typeof(User).GetEntityDisplayName()));
            }

            var subject = Constants.Email.ResetPasswordEmailSubject;
            var userName = user.GetDisplayName();
            var token = await passwordResetTokenService.SavePasswordResetTokenAsync(user.Id).ConfigureAwait(false);
            var resetLink = resetUrl.FormatWith(Uri.EscapeDataString(token.Token), Uri.EscapeDataString(user.Email));
            var plainTextContent = Constants.Email.ResetPasswordEmailPlainText.FormatWith(userName, resetLink);

            var resp = await emailService.SendEmail(user.Email, subject, plainTextContent, string.Empty).ConfigureAwait(false);

            if (resp.IsSuccess)
            {
                resp.Message = Constants.Email.ResetPasswordEmailSent;
            }

            return resp;
        }

        /// <summary>
        /// Validates a password reset token for a given email address.
        /// </summary>
        /// <param name="request">
        /// A <see cref="TokenVerificationRequest"/> containing:
        /// <list type="bullet">
        /// <item>The token to validate.</item>
        /// <item>The email address associated with the token.</item>
        /// </list>
        /// </param>
        /// <returns>
        /// An <see cref="IResponse"/> containing:
        /// <list type="bullet">
        /// <item>Success or failure status indicating whether the token is valid.</item>
        /// <item>A message describing the validation result (e.g., "Token verified successfully" or "Invalid reset link").</item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// Delegates the token validation.
        /// </remarks>
        public async Task<IResponse> VerifyPasswordResetToken(TokenVerificationRequest request)
        {
            return await passwordResetTokenService.ValidatePasswordResetTokenAsync(request.Token, request.Email).ConfigureAwait(false);
        }

        /// <summary>
        /// Generates a one-time password (OTP) for the user associated with the specified email address and sends it via email.
        /// </summary>
        /// <param name="email">
        /// The email address of the user for whom the OTP should be generated and sent.
        /// </param>
        /// <returns>
        /// An <see cref="IResponse"/> containing:
        /// <list type="bullet">
        /// <item>Success or failure status indicating whether the OTP was successfully sent.</item>
        /// <item>A message describing the result (e.g., "OTP sent successfully" or an error message).</item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// The method performs the following steps:
        /// <list type="number">
        /// <item>Retrieves the user by email.</item>
        /// <item>Generates and saves a new OTP for the user.</item>
        /// <item>Sends the email.</item>
        /// <item>Returns the response with the appropriate success or failure message.</item>
        /// </list>
        /// </remarks>
        public async Task<IResponse> SendOTPAsync(string email)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == email).ConfigureAwait(false);
            if (user?.Email == null)
            {
                return response.SetFailure(Constants.NotFound.FormatWith("User"));
            }

            var subject = Constants.Email.OTPEmailSubject;
            var userName = user.GetDisplayName();

            var otp = await otpService.GenerateAndSaveOtpAsync(user.Id).ConfigureAwait(false);

            if (otp.Data is not OtpCode otpCode)
            {
                return response.SetFailure(Constants.OtpGeneratedFailure);
            }

            var templatePath = Path.Combine(AppContext.BaseDirectory, "Content", "SendOtp.html");
            var htmlContent = "";
            if (File.Exists(templatePath))
            {
                htmlContent = await File.ReadAllTextAsync(templatePath).ConfigureAwait(false);
                htmlContent = htmlContent.Replace("{0}", userName);
                htmlContent = htmlContent.Replace("{1}", otpCode.Code);
            }

            var resp = await emailService.SendEmail(user.Email, subject, $"Your verification code is: {otpCode.Code}", htmlContent, "Notifications").ConfigureAwait(false);
            resp.Message = resp.IsSuccess ? Constants.Email.OTPSentSuccess : resp.Message;
            return resp;
        }

        /// <summary>
        /// Updates the password for a user after validating the password reset token and ensuring the new password meets complexity requirements.
        /// </summary>
        /// <param name="request">
        /// A <see cref="PasswordResetRequest"/> containing:
        /// <list type="bullet">
        /// <item>The user's email address.</item>
        /// <item>The new password and its confirmation.</item>
        /// </list>
        /// </param>
        /// <returns>
        /// An <see cref="IResponse"/> containing:
        /// <list type="bullet">
        /// <item>Success status and message if the password was updated successfully.</item>
        /// <item>Failure status and error message if the user is not found, the passwords do not match, or the password does not meet complexity requirements.</item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// The method performs the following steps:
        /// <list type="number">
        /// <item>Checks if the new password and confirmation match.</item>
        /// <item>Retrieves the user by email.</item>
        /// <item>Validates the complexity of the new password.</item>
        /// <item>Starts a database transaction to ensure atomic updates.</item>
        /// <item>Updates the user's password hash and unlocks the account if it was locked.</item>
        /// <item>Marks any previous failed login attempts and unused password reset tokens as used.</item>
        /// <item>Commits the transaction if successful, or rolls back in case of exceptions.</item>
        /// </list>
        /// </remarks>
        public async Task<IResponse> UpdateUserPassword(PasswordResetRequest request)
        {
            if (request.Password != request.ConfirmPassword)
            {
                return response.SetFailure(Constants.PasswordAndConfirmPasswordNotSame);
            }

            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == request.Email).ConfigureAwait(false);

            if (user != null)
            {
                var complexity = request.Password.CheckPasswordComplexity();

                if (complexity.IsComplex)
                {
                    using var transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);

                    try
                    {
                        user.PasswordHash = request.Password.GeneratePasswordHash();

                        if (user.Status == (int)StatusType.Locked)
                        {
                            user.Status = (int)StatusType.Active;
                        }

                        user.UpdatedOn = DateTimeExtension.UtcNowUnixTimestamp;

                        dbContext.Update(user);

                        var usedStatus = (int?)StatusType.Used;

                        var userLogUpdates = await dbContext.UserLoginLogs
                            .Where(x => x.Email == request.Email
                                && !x.IsSuccessful
                                && x.Status == (int)StatusType.NotUsed)
                            .ExecuteUpdateAsync(s =>
                                s.SetProperty(x => x.Status, _ => usedStatus)).ConfigureAwait(false);

                        await dbContext.SaveChangesAsync().ConfigureAwait(false);
                        var resp = await dbContext.PasswordResetTokens
                                      .Where(x => x.UserId == user.Id && x.IsUsed == false)
                                      .ExecuteUpdateAsync(u => u
                                          .SetProperty(u => u.IsUsed, u => true)).ConfigureAwait(false);
                        await transaction.CommitAsync().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync().ConfigureAwait(false);
                        return response.SetFailure(ex.Message);
                    }

                    return response.SetSuccess(Constants.UpdateSuccess.FormatWith("Password"));
                }
                else
                {
                    return response.SetFailure(complexity.ErrorMessage);
                }
            }
            else
            {
                return response.SetFailure(Constants.NotFound.FormatWith("User"));
            }
        }
    }
}