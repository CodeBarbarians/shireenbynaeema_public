namespace Application
{
using SharedServices;

/// <summary>
/// Service interface for handling user authentication, including login, password reset, OTP verification, and token management.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Encrypts the specified user identifier and applies a digital signature to the result.
    /// </summary>
    /// <param name="userId">The user identifier to be encrypted and signed. Cannot be null or empty.</param>
    /// <returns>A string containing the encrypted and signed representation of the user identifier.</returns>
    string EncryptAndSign(string userId);

    /// <summary>
    /// Decrypts the provided token to retrieve the original user ID and verifies the token's integrity. This method is used to validate tokens received from clients,
    /// ensuring that they have not been tampered with and that the contained user ID is authentic. If the token is valid,
    /// it returns the decrypted user ID; otherwise, it returns null or an appropriae error response.
    /// </summary>
    /// <param name="protectedValue">protectedValue.</param>
    /// <returns>string.</returns>
    string? DecryptAndVerify(string protectedValue);

    /// <summary>
    /// Handles user login by validating the provided credentials (email and password) and recaptcha token. If the credentials are valid, it generates an authentication token for the user and returns a response indicating success.
    /// If the credentials are invalid, it returns an appropriate error response.
    /// </summary>
    /// <param name="request">request.</param>
    /// <returns>string.</returns>
    Task<IResponse> LoginUser(LoginRequest request);

    /// <summary>
    /// Sends a password reset email to the specified email address. This method generates a password reset token,
    /// encrypts it, and sends an email containing a link with the token to the user.
    /// The link allows the user to reset their password securely.
    /// The method returns a response indicating whether the email was sent successfully or if there were any issues during the process.
    /// </summary>
    /// <param name="email">email.</param>
    /// <returns>IResponse.</returns>
    Task<IResponse> SendForgotPasswordEmailAsync(string email);

    /// <summary>
    /// Verifies the password reset token received from the user. This method checks if the token is valid and has not expired,
    /// and if it corresponds to the correct user.
    /// </summary>
    /// <param name="request">request.</param>
    /// <returns>IResponse.</returns>
    Task<IResponse> VerifyPasswordResetToken(TokenVerificationRequest request);

    /// <summary>
    /// Updates the user's password after verifying the password reset token. This method takes the new password and confirmation password from the request.
    /// </summary>
    /// <param name="request">PasswordResetRequest.</param>
    /// <returns>IResponse.</returns>
    Task<IResponse> UpdateUserPassword(PasswordResetRequest request);

    /// <summary>
    /// Sends a One-Time Password (OTP) to the specified email address for authentication purposes. This method generates a unique OTP, associates it with the user's email,
    /// and sends it via email. The OTP can then be used for verifying the user's identity during login or other sensitive operations.
    /// The method returns a response indicating whether the OTP was sent successfully or if there were any issues during the process.
    /// </summary>
    /// <param name="email">Email.</param>
    /// <returns>IResponse.</returns>
    Task<IResponse> SendOTPAsync(string email);

    /// <summary>
    /// Verifies the One-Time Password (OTP) provided by the user. This method checks if the OTP is valid,
    /// has not expired, and matches the one associated with the user's email.
    /// </summary>
    /// <param name="email">Email.</param>
    /// <param name="code">Code.</param>
    /// <param name="rememberMe">rememberMe.</param>
    /// <returns>IResponse.</returns>
    Task<IResponse> VerifyUserOTPAsync(string email, string code, bool rememberMe);
}
}