namespace Infrastructure
{
    using System.Security.Cryptography;

    using Application;
    using Domain;
    using Microsoft.EntityFrameworkCore;
    using SharedServices;

    /// <summary>
    /// Provides functionality for generating, saving, and verifying one-time passwords (OTPs) for users, including rate
    /// limiting and OTP lifecycle management.
    /// </summary>
    /// <remarks>This service enforces rate limiting to prevent excessive OTP requests and ensures that only
    /// one valid, unused OTP exists per user at a time. All OTP operations are performed asynchronously. The service is
    /// intended for scenarios requiring secure, time-limited authentication codes, such as user login or verification
    /// workflows.</remarks>
    /// <param name="dbContext">The database context used to access and persist OTP codes and related user data.</param>
    /// <param name="response">The response object used to encapsulate operation results, including success status, messages, and returned
    /// data.</param>
    public class OtpService(DatabaseContext dbContext, IResponse response) : IOtpService
    {
        /// <summary>
        /// Generates a new one-time password (OTP) for a specified user, invalidates any existing unused OTPs, and saves the new OTP to the database.
        /// </summary>
        /// <param name="userId">
        /// The <see cref="Guid"/> of the user for whom the OTP is generated.
        /// </param>
        /// <returns>
        /// An <see cref="IResponse"/> containing:
        /// <list type="bullet">
        /// <item>The newly generated OTP object, including code, expiry, and creation timestamp.</item>
        /// <item>A success message if the OTP was generated and saved successfully.</item>
        /// <item>An error message if an exception occurred during the process.</item>
        /// </list>
        /// </returns>
        public async Task<IResponse> GenerateAndSaveOtpAsync(Guid userId)
        {
            try
            {
                var now = DateTimeExtension.UtcNowUnixTimestamp;
                var windowStart = DateTimeExtension.UtcNow.AddMinutes(-5).ToTimeStamp();

                // Rate limit: max 3 OTP requests in 5 minutes
                var recentOtpCount = await dbContext.OtpCodes
                    .Where(x => x.UserId == userId && x.CreatedAt >= windowStart)
                    .CountAsync();

                if (recentOtpCount >= 3)
                {
                    response.Message = "Too many OTP requests. Please try again later.";
                    response.IsSuccess = Constants.ResponseFailure;
                    return response;
                }

                // invalidate previous OTPs
                await dbContext.OtpCodes
                    .Where(x => x.UserId == userId && x.IsUsed == false)
                    .ExecuteUpdateAsync(u => u
                        .SetProperty(u => u.IsUsed, true));

                var code = await this.GenerateOtpAsync(4);

                var otp = new OtpCode
                {
                    UserId = userId,
                    Code = code,
                    Expiry = DateTimeExtension.UtcNow.AddMinutes(10).ToTimeStamp(),
                    IsUsed = false,
                    CreatedAt = now,
                };

                dbContext.OtpCodes.Add(otp);
                await dbContext.SaveChangesAsync();

                response.Data = otp;
                response.Message = Constants.OtpGeneratedSuccess;
                response.IsSuccess = Constants.ResponseSuccess;

                return response;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.IsSuccess = Constants.ResponseFailure;
                return response;
            }
        }

        /// <summary>
        /// Generates a random numeric one-time password (OTP) of a specified length.
        /// </summary>
        /// <param name="size">
        /// The number of digits for the OTP. Defaults to 6 if not specified.
        /// </param>
        /// <returns>
        /// A <see cref="Task{String}"/> representing the asynchronously generated OTP as a string.
        /// </returns>
        public async Task<string> GenerateOtpAsync(int size = 6)
        {
            int min = (int)Math.Pow(10, size - 1);
            int max = (int)Math.Pow(10, size) - 1;

            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            int value = BitConverter.ToInt32(bytes, 0) & int.MaxValue;

            int code = (value % (max - min + 1)) + min;
            return await Task.FromResult(code.ToString());
        }

        /// <summary>
        /// Verifies a one-time password (OTP) for the specified user asynchronously.
        /// </summary>
        /// <remarks>The method checks for an unused, unexpired OTP code associated with the user. If the
        /// code is valid, it is marked as used and the verification is successful. If the code is invalid, expired, or
        /// already used, the response indicates failure. The method does not throw exceptions; errors are returned in
        /// the response object.</remarks>
        /// <param name="userId">The unique identifier of the user for whom the OTP verification is performed.</param>
        /// <param name="code">The OTP code to verify. Must match an active, unused code for the user.</param>
        /// <returns>A response object containing the result of the verification. The response includes the OTP data if
        /// verification succeeds; otherwise, it contains an error message.</returns>
        public async Task<IResponse> VerifyOtpAsync(Guid userId, string code)
        {
            try
            {
                var otp = await dbContext.OtpCodes
                        .Where(o => o.UserId == userId && o.Code == code && !o.IsUsed && o.Expiry > DateTimeExtension.UtcNowUnixTimestamp)
                        .OrderByDescending(o => o.CreatedAt)
                        .FirstOrDefaultAsync();

                if (otp == null)
                {
                    response.Message = Constants.NotFound.FormatWith("OTP");
                    response.IsSuccess = Constants.ResponseFailure;
                    return response;
                }

                otp.IsUsed = true;
                await dbContext.SaveChangesAsync();
                response.Data = otp;
                response.Message = Constants.VerifiedSuccess.FormatWith("OTP");
                response.IsSuccess = Constants.ResponseSuccess;
                return response;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.IsSuccess = Constants.ResponseFailure;
                return response;

                // throw;
            }
        }
    }
}