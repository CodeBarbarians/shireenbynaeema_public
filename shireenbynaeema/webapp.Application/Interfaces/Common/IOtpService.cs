namespace Application
{
    using SharedServices;

    /// <summary>
    /// Interface for OTP (One-Time Password) services, providing methods to generate, save, and verify OTPs for user authentication or verification purposes.
    /// </summary>
    public interface IOtpService
    {
        /// <summary>
        /// Generates a new OTP for the specified user and saves it for later verification. This method is typically used in scenarios where a user needs
        /// to verify their identity or perform a sensitive action that requires additional security. The generated OTP is usually sent to the user via email,
        /// SMS, or another communication channel.
        /// </summary>
        /// <param name="userId">The unique identifier of the user for whom the OTP is being generated.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IResponse"/> indicating whether the OTP was successfully generated and saved.</returns>
        Task<IResponse> GenerateAndSaveOtpAsync(Guid userId);

        /// <summary>
        /// Verifies the provided OTP code for the specified user. This method checks if the OTP is valid, has not expired, and matches the one that
        /// was generated and saved for the user. If the OTP is valid, it returns a response indicating success; otherwise, it returns an appropriate
        /// error response. This method is crucial for ensuring that only authorized users can perform certain actions or access specific resources after
        /// successfully verifying their identity with the OTP.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose OTP is being verified.</param>
        /// <param name="code">The OTP code provided by the user for verification.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IResponse"/> indicating whether the OTP verification was successful or providing error details if validation failed.</returns>
        Task<IResponse> VerifyOtpAsync(Guid userId, string code);
    }
}