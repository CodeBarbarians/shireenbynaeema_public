namespace Application
{
    using Domain;
    using SharedServices;

    /// <summary>
    /// Interface for password reset token services, providing methods to manage password reset tokens. This service is designed to handle the generation, validation,
    /// and storage of password reset tokens, ensuring that users can securely reset their passwords when needed.
    /// </summary>
    public interface IPasswordResetTokenService
    {
        /// <summary>
        /// Generates a new password reset token for the specified user and saves it for later validation. This method is typically used when a user requests
        /// to reset their password, and it ensures that a unique token is created and associated with the user's account, allowing for secure password reset processes.
        /// </summary>
        /// <param name="userId">The unique identifier of the user requesting the password reset.</param>
        /// <returns>A <see cref="PasswordResetToken"/> object containing the generated token and its associated metadata.</returns>
        Task<PasswordResetToken> SavePasswordResetTokenAsync(Guid userId);

        /// <summary>
        /// Validates the provided password reset token against the user's email. This method checks if the token is valid, has not expired, and
        /// is associated with the correct user.
        /// </summary>
        /// <param name="token">The password reset token string to validate.</param>
        /// <param name="email">The email address associated with the user account for validation.</param>
        /// <returns>An <see cref="IResponse"/> object indicating the validation result with success/failure status and any relevant error messages.</returns>
        Task<IResponse> ValidatePasswordResetTokenAsync(string token, string email);

        /// <summary>
        /// Marks the specified password reset token as used. This method is typically called after a successful password reset to ensure that the token cannot be reused,
        /// providing an additional layer of security.
        /// </summary>
        /// <param name="tokenEntry">The <see cref="PasswordResetToken"/> object to mark as used.</param>
        /// <returns><c>true</c> if the token was successfully marked as used; otherwise, <c>false</c>.</returns>
        Task<bool> MarkTokenAsUsed(PasswordResetToken tokenEntry);
    }
}