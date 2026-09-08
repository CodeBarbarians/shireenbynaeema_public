namespace Infrastructure
{
    using Application;
    using Domain;
    using Microsoft.EntityFrameworkCore;
    using SharedServices;

    /// <summary>
    /// Provides functionality for generating, validating, and managing user invite tokens within the application.
    /// </summary>
    /// <remarks>This service ensures that only one valid invite token exists per user at a time and handles
    /// token lifecycle operations, including marking tokens as used and validating their association with users. It is
    /// intended to be used in scenarios where secure user onboarding or invitation flows are required.</remarks>
    /// <param name="dbContext">The database context used to access and modify user and invite token data.</param>
    /// <param name="config">The application configuration settings that determine token expiry and related parameters.</param>
    /// <param name="response">The response object used to encapsulate operation results and messages.</param>
    /// <param name="tokenService">The token service responsible for generating account action tokens upon successful invite validation.</param>
    public class UserInviteTokenService(DatabaseContext dbContext, IAppSettingsConfig config, IResponse response, ITokenService tokenService) : IUserInviteTokenService
    {
        /// <summary>
        /// Generates and saves a new user invite token for a specified user.
        /// </summary>
        /// <param name="userId">
        /// The unique identifier of the user for whom the invite token is being created.
        /// </param>
        /// <returns>
        /// A <see cref="UserInviteToken"/> containing:
        /// <list type="bullet">
        /// <item>The newly generated secure token.</item>
        /// <item>The user ID associated with the token.</item>
        /// <item>The token's expiry date based on configuration settings.</item>
        /// <item>A flag indicating whether the token has been used (initially <c>false</c>).</item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// Any existing unused tokens for the user are first marked as used to ensure only one valid token exists at a time.
        /// </remarks>
        public async Task<UserInviteToken> SaveUserInviteTokenAsync(Guid userId)
        {
            var resp = await dbContext.UserInviteTokens
                                   .Where(x => x.UserId == userId && x.IsUsed == false)
                                   .ExecuteUpdateAsync(u => u
                                       .SetProperty(u => u.IsUsed, u => true));

            var tokenEntry = new UserInviteToken
            {
                UserId = userId,
                Token = EncryptionExtension.GenerateSecureToken(),
                ExpiryDate = DateTimeExtension.UtcNow.AddMinutes(Convert.ToInt32(config.UserInvite.TokenExpiry)).ToTimeStamp(),
                IsUsed = false,
            };

            dbContext.UserInviteTokens.Add(tokenEntry);
            await dbContext.SaveChangesAsync();
            return tokenEntry;
        }

        /// <summary>
        /// Validates a user invite token by ensuring it is associated with the specified email, has not been used, and has not expired.
        /// </summary>
        /// <param name="token">
        /// The invite token to validate.
        /// </param>
        /// <param name="email">
        /// The email address of the user for whom the token should be valid.
        /// </param>
        /// <returns>
        /// An <see cref="IResponse"/> containing:
        /// <list type="bullet">
        /// <item>Success status indicating whether the token is valid.</item>
        /// <item>A message confirming validation success or describing the failure reason (e.g., not found or expired).</item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method first retrieves the user by email. If the user exists, it checks for a corresponding unused and unexpired token.
        /// If valid, the token is considered verified. Otherwise, a failure response is returned.
        /// </remarks>
        public async Task<IResponse> ValidateUserInviteTokenAsync(string token, string email)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null)
            {
                response.IsSuccess = Constants.ResponseFailure;
                response.Message = Constants.NotFound.FormatWith(typeof(User).GetEntityDisplayName());
                return response;
            }

            var tokenEntry = await dbContext.UserInviteTokens
                .FirstOrDefaultAsync(x => x.Token == token && x.UserId == user.Id && !x.IsUsed && x.ExpiryDate > DateTimeExtension.UtcNowUnixTimestamp);
            if (tokenEntry != null)
            {
                // var res = await MarkTokenAsUsed(tokenEntry);
                response.IsSuccess = true;
                response.Message = Constants.VerifiedSuccess.FormatWith("Token");
                response.Data = new { Token = tokenService.GenerateTokenForAccountAction(user) };
                return response;
            }
            else
            {
                response.IsSuccess = Constants.ResponseFailure;
                response.Message = Constants.InvalidUserInviteLink;
                return response;
            }
        }

        /// <summary>
        /// Marks a user invite token as used in the database to prevent it from being reused.
        /// </summary>
        /// <param name="tokenEntry">
        /// The <see cref="UserInviteToken"/> entity to mark as used.
        /// </param>
        /// <returns>
        /// A <see cref="bool"/> indicating whether the operation was successful:
        /// <list type="bullet">
        /// <item>True: The token was successfully marked as used.</item>
        /// <item>False: An error occurred while updating the token.</item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method updates the <c>IsUsed</c> property of the token to true and saves the changes to the database.
        /// Exceptions are caught, and false is returned in case of failure.
        /// </remarks>
        public async Task<bool> MarkTokenAsUsed(UserInviteToken tokenEntry)
        {
            try
            {
                tokenEntry.IsUsed = true;
                await dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}