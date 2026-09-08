namespace Application
{
    using SharedServices;

    /// <summary>
    /// Interface for reCAPTCHA services, providing methods to verify reCAPTCHA tokens. This service is designed to help protect the application from spam
    /// and abuse by verifying that user interactions are performed by humans rather than bots. The method included in this interface allows for the verification
    /// of a reCAPTCHA token against a specified action and a minimum score threshold, ensuring that only legitimate interactions are allowed to proceed based
    /// on the reCAPTCHA validation results.
    /// </summary>
    public interface IRecaptchaService
    {
        /// <summary>
        /// Asynchronously verifies a reCAPTCHA token against a specified action and a minimum score threshold. This method sends the token to the reCAPTCHA
        /// service for validation and checks if the token is valid and meets the specified criteria.
        /// </summary>
        /// <param name="token">token.</param>
        /// <param name="action">action.</param>
        /// <param name="minimumScore">minimumScore.</param>
        /// <returns>IResponse.</returns>
        Task<IResponse> VerifyTokenAsync(string token, string action, float minimumScore = 0.5f);
    }
}