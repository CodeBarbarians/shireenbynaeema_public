namespace Application
{
    using Domain;
    using SharedServices;

    /// <summary>
    /// Interface for notification setting services, providing methods to manage user notification settings. This service is designed to allow users to
    /// customize their notification preferences, enabling them to choose which types of notifications they want to receive and how they want to receive them.
    /// The methods include adding new notification settings for a user, updating existing notification settings in bulk, and retrieving the
    /// current notification settings for the user. This interface provides a centralized way to manage user notification preferences across the application,
    /// ensuring that users have control over their notifications and can tailor their experience according to their needs.
    /// </summary>
    public interface INotificationSettingService
    {
        /// <summary>
        /// Adds new notification settings for a user based on their UserId. This method is used to initialize notification settings for a user
        /// when they first interact with the application or when they want to set up their notification preferences. It takes the user's unique
        /// identifier as a parameter and returns a response indicating the success or failure of the operation.
        /// </summary>
        /// <param name="userId">The unique identifier of the user for whom notification settings are being created.</param>
        /// <returns>A task that represents the asynchronous operation, returning an <see cref="IResponse"/> containing the result of the operation.</returns>
        Task<IResponse> AddNotificationSetting(Guid userId);

        /// <summary>
        /// Updates existing notification settings for a user in bulk. This method allows users to modify multiple notification settings at once,
        /// providing a more efficient way to manage their preferences.
        /// </summary>
        /// <param name="request">A <see cref="NotificationSettingRequest"/> object containing the notification settings to be updated.</param>
        /// <returns>A task that represents the asynchronous operation, returning an <see cref="IResponse"/> containing the result of the bulk update operation.</returns>
        Task<IResponse> UpdateNotificationSettingRange(NotificationSettingRequest request);

        /// <summary>
        /// Retrieves the current notification settings for a user. This method allows users to view their existing notification preferences, enabling them
        /// to make informed decisions about their notification management.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, returning an <see cref="IResponse"/> containing the user's current notification settings.</returns>
        Task<IResponse> RetriveNotificationSetting();

        // Task<IResponse> AddAllUsersNotificationSetting();
    }
}