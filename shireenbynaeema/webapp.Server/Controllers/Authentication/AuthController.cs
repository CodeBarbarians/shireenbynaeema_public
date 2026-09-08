namespace Server
{
    using Application;
    using Microsoft.AspNetCore.Mvc;
    using SharedServices;

    /// <summary>
    /// Controller for handling authentication and authorization operations including user registration, login, password management, and token verification.
    /// Provides endpoints for user authentication flows such as login with reCAPTCHA verification, password reset, OTP verification, and invitation/work order token handling.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserExtensionService userExtensionService;
        private readonly IAuthenticationService authenticationService;
        private readonly IUserPermissionService userPermissionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class with the specified services and database context.
        /// </summary>
        /// <param name="userExtensionService">The service that provides additional user-related operations and extensions.</param>
        /// <param name="authenticationService">The service responsible for handling user authentication processes.</param>
        /// <param name="userPermissionService">The service that manages user permissions and authorization checks.</param>
        public AuthController(IUserExtensionService userExtensionService, IAuthenticationService authenticationService, IUserPermissionService userPermissionService)
        {
            this.userExtensionService = userExtensionService;
            this.authenticationService = authenticationService;
            this.userPermissionService = userPermissionService;
        }

        /// <summary>
        /// Registers a new user in the system. This endpoint is only available in DEBUG configuration.
        /// </summary>
        /// <param name="request">The user registration request containing registration details such as email, password, and other required information.</param>
        /// <returns>An <see cref="IActionResult"/> containing the result of the registration operation, including success status and user details if successful.</returns>
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest request)
        {
            return this.Ok(await this.userExtensionService.RegisterUser(request));
        }

        /// <summary>
        /// Authenticates a user and returns a login response with authentication token.
        /// In DEBUG configuration, reCAPTCHA verification is bypassed for testing purposes.
        /// In RELEASE configuration, the reCAPTCHA token is verified before proceeding with user authentication.
        /// </summary>
        /// <param name="request">The login request containing user credentials (email, password, and reCAPTCHA token).</param>
        /// <param name="recaptchaService">The reCAPTCHA service used to verify the reCAPTCHA token in non-DEBUG environments.</param>
        /// <returns>An <see cref="IActionResult"/> containing the login response with authentication token and user details on successful authentication, or error details if authentication fails or reCAPTCHA verification fails.</returns>
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, [FromServices] IRecaptchaService recaptchaService)
        {
            return this.Ok(await this.authenticationService.LoginUser(request));
        }

        /// <summary>
        /// Sends a password reset email to the specified user email address.
        /// Initiates the password recovery process by sending a reset link or token to the user's registered email.
        /// </summary>
        /// <param name="request">The password reset email request containing the user's email address.</param>
        /// <returns>An <see cref="IActionResult"/> containing the result of the password reset email operation, including success status and any relevant error messages.</returns>
        [HttpPost]
        [Route("SendForgetPasswordEmail")]
        public async Task<IActionResult> SendForgetPasswordEmail([FromBody] PasswordResetEmailRequest request)
        {
            return this.Ok(await this.authenticationService.SendForgotPasswordEmailAsync(request.Email));
        }

        /// <summary>
        /// Verifies the password reset token to ensure it is valid and has not expired.
        /// Validates that the token provided in the reset request corresponds to a legitimate password reset process.
        /// </summary>
        /// <param name="request">The token verification request containing the reset token and any additional verification details.</param>
        /// <returns>An <see cref="IActionResult"/> containing the result of the token verification operation, including success status and any relevant error messages if the token is invalid or expired.</returns>
        [HttpPost]
        [Route("VerifyPasswordResetToken")]
        public async Task<IActionResult> VerifyPasswordResetToken([FromBody] TokenVerificationRequest request)
        {
            return this.Ok(await this.authenticationService.VerifyPasswordResetToken(request));
        }

        /// <summary>
        /// Verifies the user invitation token to ensure it is valid and has not expired.
        /// Validates that the token provided in the invite request corresponds to a legitimate user invitation process.
        /// </summary>
        /// <param name="request">The token verification request containing the invitation token and any additional verification details.</param>
        /// <returns>An <see cref="IActionResult"/> containing the result of the token verification operation, including success status and any relevant error messages if the token is invalid or expired.</returns>
        [HttpPost]
        [Route("VerifyUserInviteToken")]
        public async Task<IActionResult> VerifyUserInviteToken([FromBody] TokenVerificationRequest request)
        {
            return this.Ok(await this.userExtensionService.VerifyUserInviteToken(request));
        }

        /// <summary>
        /// Sends a One-Time Password (OTP) to the specified user email address.
        /// Initiates the OTP authentication process by generating and sending an OTP code to the user's registered email for verification purposes.
        /// </summary>
        /// <param name="request">The OTP request model containing the user's email address to which the OTP will be sent.</param>
        /// <returns>An <see cref="IActionResult"/> containing the result of the OTP sending operation, including success status and any relevant error messages or OTP delivery details.</returns>
        [HttpPost]
        [Route("SendOTP")]
        public async Task<IActionResult> SendOTP([FromBody] OtpRequestModel request)
        {
            var resp = await this.authenticationService.SendOTPAsync(request.Email);
            return this.Ok(resp);
        }

        /// <summary>
        /// Verifies a one-time password (OTP) for the given user and completes the sign-in flow when verification succeeds.
        /// Behavior:
        /// - Calls <see cref="IAuthenticationService.VerifyUserOTPAsync(string, string, bool)"/> to validate the OTP.
        /// - If verification fails, the service result is returned directly (containing error details).
        /// - If verification succeeds and the returned data is a <see cref="LoginResponse"/>:
        ///   - If <paramref name="request"/>.RememberMe is true and the <see cref="LoginResponse.UserId"/> is present,
        ///     a secure cookie named "remember_user" is created containing an encrypted/signed representation of the user id.
        ///     The cookie is Secure, has SameSite=None and expires in 30 days.
        ///   - The <see cref="LoginResponse.UserId"/> is cleared (set to null) before sending the response to avoid exposing persistent identifiers.
        /// - If verification succeeds but no <see cref="LoginResponse"/> is returned, an empty OK result is returned.
        /// </summary>
        /// <param name="request">An <see cref="OtpVerifyModel"/> containing the user's email, OTP code, and RememberMe flag.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the verification result:
        /// - On failure: the failure result from the authentication service.
        /// - On success: the sanitized <see cref="LoginResponse"/> (UserId cleared) and any tokens/user metadata; a remember cookie may be set when requested.
        /// </returns>
        [HttpPost]
        [Route("VerifyOTP")]
        public async Task<IActionResult> VerifyOTP([FromBody] OtpVerifyModel request)
        {
            var result = await this.authenticationService.VerifyUserOTPAsync(request.Email, request.Code, request.RememberMe);
            if (!result.IsSuccess)
            {
                return this.Ok(result);
            }
            else
            {
                if (result.Data is LoginResponse loginData)
                {
                    if (loginData.UserId.HasValue && loginData.UserId.Value != Guid.Empty)
                    {
                        if (request.RememberMe)
                        {
                            var cookieOptions = new CookieOptions
                            {
                                // HttpOnly = true,
                                Secure = true,
                                SameSite = SameSiteMode.None,
                                Expires = DateTimeExtension.UtcNow.AddDays(30),
                            };

                            var cookieValue = this.authenticationService.EncryptAndSign(loginData.UserId.ToString()!);
                            this.Response.Cookies.Append("remember_user", cookieValue, cookieOptions);
                        }
                    }

                    loginData.UserId = null;
                    result.Data = loginData;
                    return this.Ok(result);
                }
                else
                {
                    return this.Ok();
                }
            }
        }

        /// <summary>
        /// Updates the user's password using the provided password reset request.
        /// Changes the password for the specified user account to the new password provided in the request.
        /// </summary>
        /// <param name="request">The password reset request containing the user's email, new password, and password confirmation for validation.</param>
        /// <returns>An <see cref="IActionResult"/> containing the result of the password update operation, including success status and any relevant error messages.</returns>
        [HttpPost]
        [Route("UpdatePassword")]
        public async Task<IActionResult> UpdatePassword([FromBody] PasswordResetRequest request)
        {
            var resp = await this.authenticationService.UpdateUserPassword(request);
            return this.Ok(resp);
        }

        /// <summary>
        /// Completes the user registration process using the provided registration details.
        /// </summary>
        /// <param name="request">The registration information for the user. Cannot be null.</param>
        /// <returns>An IActionResult indicating the result of the registration completion operation.</returns>
        [HttpPost("CompleteUserRegistration")]
        public async Task<IActionResult> CompleteUserRegistration([FromBody] UserRegisterRequest request)
        {
            return this.Ok(await this.userExtensionService.CompleteUserRegistration(request));
        }

        /// <summary>
        /// Retrieves all permissions assigned to the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose permissions are to be retrieved.</param>
        /// <returns>An <see cref="IActionResult"/> containing the list of permissions for the specified user. Returns an empty
        /// list if the user has no permissions.</returns>
        [HttpGet("GetAllPermissions/{userId}")]
        public async Task<IActionResult> GetAllPermissions(Guid userId)
            => this.Ok(await this.userPermissionService.GetAllPermissions(userId));
    }
}