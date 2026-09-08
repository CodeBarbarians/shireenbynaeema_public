namespace Application
{
    using Domain;
    using SharedServices;

    /// <summary>
    /// Interface for user invite token services, providing methods to manage user invite tokens. This service is designed to handle the generation, validation,
    /// and storage of user invite tokens, ensuring that users can securely invite others to join the application.
    /// </summary>
    public interface IUserInviteTokenService
    {
        /// <summary>
        /// Generates a new user invite token for the specified user and saves it for later validation. This method is typically used when a user wants
        /// to invite another person to join the application, allowing them to create an account using the generated token. The token will have an
        /// expiry date and can only be used once.
        /// </summary>
        /// <param name="userId">userid.</param>
        /// <returns>UserInviteToken.</returns>
        Task<UserInviteToken> SaveUserInviteTokenAsync(Guid userId);

        /// <summary>
        /// Validates the provided user invite token against the stored tokens. This method checks if the token exists, is associated with the correct email,
        /// and has not expired. If the token is valid, it allows the user to proceed with the account creation process.
        /// </summary>
        /// <param name="token">token.</param>
        /// <param name="email">email.</param>
        /// <returns>IResponse.</returns>
        Task<IResponse> ValidateUserInviteTokenAsync(string token, string email);

        /// <summary>
        /// Marks the specified user invite token as used, preventing it from being used again. This method is typically called after a user successfully uses the token
        /// to create an account.
        /// </summary>
        /// <param name="tokenEntry">tokenEntry.</param>
        /// <returns>true or false.</returns>
        Task<bool> MarkTokenAsUsed(UserInviteToken tokenEntry);
    }
}