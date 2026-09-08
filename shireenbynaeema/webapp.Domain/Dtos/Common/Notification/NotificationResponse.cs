namespace Domain
{
    using SharedServices;

    /// <summary>
    /// Represents a notification item returned in listing responses.
    /// Inherits common listing metadata from <see cref="Base_Listing"/>.
    /// </summary>
    public class Notification_Listing : Base_Listing
    {
        /// <summary>
        /// Gets or sets the heading or short title of the notification.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the detailed message body of the notification.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets optional URL to navigate to when the notification is clicked.
        /// Can be null or empty if no navigation is required.
        /// </summary>
        public string? NavigationUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether indicates whether the notification has been read by the user.
        /// True = read, False = unread.
        /// </summary>
        public bool IsRead { get; set; }
    }
}