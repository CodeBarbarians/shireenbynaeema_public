namespace Application
{
    using SharedServices;

    /// <summary>
    /// Interface for profile details services, providing methods to retrieve profile details. This service is designed to allow users to view their profile information,
    /// including personal details, contact information, and preferences.
    /// </summary>
    public interface IProfileDetailsService
    {
        /// <summary>
        /// Retrieves the profile details of the currently authenticated user. This method is intended to provide users with access to their profile information,
        /// allowing them to view and manage their personal details, contact information, and preferences. The response will include relevant data such as the
        /// user's name, email address, profile picture, and any other pertinent information associated with their profile.
        /// </summary>
        /// <returns>IResponse.</returns>
        public Task<IResponse> GetProfileDetials();
    }
}