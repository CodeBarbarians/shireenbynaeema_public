namespace Infrastructure
{
    using Application;
    using Domain;
    using Microsoft.EntityFrameworkCore;
    using SharedServices;

    /// <summary>
    /// Provides functionality for generating, validating, and managing password reset tokens for users. Implements
    /// secure token creation, validation against user credentials and token state, and ensures tokens cannot be reused.
    /// </summary>
    /// <remarks>This service is intended to be used in scenarios where password reset functionality is
    /// required, such as user account recovery workflows. Tokens generated are securely stored and validated to prevent
    /// unauthorized access. The service ensures that tokens are single-use and expire after a configurable period.
    /// Thread safety depends on the underlying database context implementation.</remarks>
    /// <param name="dbContext">The database context used to access and modify password reset tokens and user information.</param>
    /// <param name="config">The application configuration settings that determine token expiry and related parameters.</param>
    /// <param name="response">The response object used to encapsulate operation results and messages for token validation actions.</param>
    /// <param name="tokenService">The token service responsible for generating account action tokens upon successful password reset validation.</param>
    public class PasswordResetTokenService(DatabaseContext dbContext, IAppSettingsConfig config, IResponse response, ITokenService tokenService) : IPasswordResetTokenService
    {
        /// <summary>
        /// Generates a secure password reset token for a specified user and saves it to the database.
        /// </summary>
        /// <param name="userId">
        /// The unique identifier of the user for whom the password reset token is being generated.
        /// </param>
        /// <returns>
        /// A <see cref="PasswordResetToken"/> containing:
        /// <list type="bullet">
        /// <item>The generated secure token.</item>
        /// <item>The user ID associated with the token.</item>
        /// <item>The expiration timestamp of the token.</item>
        /// <item>A flag indicating whether the token has been used.</item>
        /// </list>
        /// </returns>
        public async Task<PasswordResetToken> SavePasswordResetTokenAsync(Guid userId)
        {
            var resp = await dbContext.PasswordResetTokens
                                   .Where(x => x.UserId == userId && x.IsUsed == false)
                                   .ExecuteUpdateAsync(u => u
                                       .SetProperty(u => u.IsUsed, u => true));

            var tokenEntry = new PasswordResetToken
            {
                UserId = userId,
                Token = EncryptionExtension.GenerateSecureToken(),
                ExpiryDate = DateTimeExtension.UtcNow.AddMinutes(Convert.ToInt32(config.PasswordReset.TokenExpiry)).ToTimeStamp(),
                IsUsed = false,
            };

            dbContext.PasswordResetTokens.Add(tokenEntry);
            await dbContext.SaveChangesAsync();
            return tokenEntry;
        }

        /// <summary>
        /// Validates a password reset token for the specified email address asynchronously.
        /// </summary>
        /// <remarks>The method checks that the token is unused and has not expired. If the token is
        /// valid, it is marked as used and a new token is generated for further account actions. If the user is not
        /// found or the token is invalid, the response indicates failure.</remarks>
        /// <param name="token">The password reset token to validate. Must be a valid, unused token associated with the user.</param>
        /// <param name="email">The email address of the user whose password reset token is being validated. Cannot be null or empty.</param>
        /// <returns>A response indicating whether the token is valid. If valid, the response contains a new account action
        /// token; otherwise, it includes an error message.</returns>
        public async Task<IResponse> ValidatePasswordResetTokenAsync(string token, string email)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null)
            {
                response.IsSuccess = Constants.ResponseFailure;
                response.Message = Constants.NotFound.FormatWith(typeof(User).GetEntityDisplayName());
                return response;
            }

            var tokenEntry = await dbContext.PasswordResetTokens
                .FirstOrDefaultAsync(x => x.Token == token && x.UserId == user.Id && !x.IsUsed && x.ExpiryDate > DateTimeExtension.UtcNowUnixTimestamp);

            if (tokenEntry != null)
            {
                var res = await this.MarkTokenAsUsed(tokenEntry);
                response.IsSuccess = res;
                response.Message = Constants.VerifiedSuccess.FormatWith("Token");
                response.Data = new { Token = await tokenService.GenerateTokenForAccountAction(user) };
                return response;
            }
            else
            {
                response.IsSuccess = Constants.ResponseFailure;
                response.Message = Constants.InvalidResetLink;
                return response;
            }
        }

        /// <summary>
        /// Marks a given <see cref="PasswordResetToken"/> as used in the database to prevent reuse.
        /// </summary>
        /// <param name="tokenEntry">
        /// The <see cref="PasswordResetToken"/> entity to mark as used.
        /// </param>
        /// <returns>
        /// A <see cref="bool"/> indicating whether the operation succeeded:
        /// <list type="bullet">
        /// <item>true: the token was successfully marked as used.</item>
        /// <item>false: an error occurred while updating the token.</item>
        /// </list>
        /// </returns>
        public async Task<bool> MarkTokenAsUsed(PasswordResetToken tokenEntry)
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