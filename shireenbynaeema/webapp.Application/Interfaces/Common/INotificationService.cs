namespace Application
{
    using Domain;
    using SharedServices;

    /// <summary>
    /// Interface for notification services, providing methods to send notifications, retrieve notifications, and manage notification statuses.
    /// This service is designed to handle various types of notifications that may be sent to users or systems based on specific events or actions
    /// within the application. The methods allow for sending individual notifications, bulk notifications, retrieving a list of notifications,
    /// marking notifications as read, and sending email notifications related to notes and status updates. This interface provides a centralized way to
    /// manage and deliver notifications across the application, ensuring consistency and efficiency in communication with users and systems.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Sends a single notification based on the provided <see cref="Notification"/> object and <see cref="NotificationCategory"/>.
        /// This method is used to deliver notifications to users or systems for specific events or triggers.
        /// </summary>
        /// <param name="notification">The <see cref="Notification"/> object containing notification details and content.</param>
        /// <param name="category">The <see cref="NotificationCategory"/> that categorizes the type of notification being sent.</param>
        /// <returns>An <see cref="IResponse"/> indicating the success or failure of the notification delivery.</returns>
        Task<IResponse> NotifyAsync(Notification notification, NotificationCategory category);

        /// <summary>
        /// Sends multiple notifications in bulk based on the provided list of <see cref="Notification"/> objects and a specified <see cref="NotificationCategory"/>.
        /// This method is designed to efficiently deliver a large number of notifications to users or systems, reducing the overhead associated
        /// with sending individual notifications and ensuring timely communication for events that may trigger multiple notifications at once.
        /// </summary>
        /// <param name="notifications">A collection of <see cref="Notification"/> objects to be sent in bulk.</param>
        /// <param name="category">The <see cref="NotificationCategory"/> that applies to all notifications in the batch.</param>
        /// <returns>An <see cref="IResponse"/> indicating the success or failure of the bulk notification delivery operation.</returns>
        Task<IResponse> BulkNotifyAsync(List<Notification> notifications, NotificationCategory category);

        /// <summary>
        /// Retrieves a list of notifications for the current user or system. This method allows users to view their notifications, which may include alerts,
        /// updates, or messages related to their interactions with the application. The retrieved notifications can be displayed in a user interface,
        /// providing users with timely information about relevant events or actions that require their attention.
        /// </summary>
        /// <returns>An <see cref="IResponse"/> containing the list of notifications for the current user or system.</returns>
        Task<IResponse> GetNotificationsAsync();

        /// <summary>
        /// Marks all notifications as read for the current user or system. This method is used to update the status of all unread notifications,
        /// indicating that they have been acknowledged by the user. Upon successful execution, all pending notifications will be marked with a read status.
        /// </summary>
        /// <returns>An <see cref="IResponse"/> indicating the success or failure of the operation to mark all notifications as read.</returns>
        Task<IResponse> MarkAllAsReadNotificationsAsync();
    }
}