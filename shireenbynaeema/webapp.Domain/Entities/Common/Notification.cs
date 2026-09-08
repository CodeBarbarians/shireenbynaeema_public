namespace Domain
{
    using System.ComponentModel.DataAnnotations.Schema;

    using SharedServices;

    /// <summary>
    /// Represents a user notification message stored in the system.
    /// </summary>
    public class Notification : Auditable, IIdentifiable
    {
        /// <summary>Gets or sets unique notification identifier.</summary>
        public Guid Id { get; set; }

        /// <summary>Gets or sets target user identifier.</summary>
        [ForeignKey(nameof(UserNavigation))]
        [AuditDisplay(typeof(User), nameof(User.DisplayName))]
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the user associated with this entity through navigation properties.
        /// </summary>
        public virtual User UserNavigation { get; set; } = default!;

        /// <summary>Gets or sets notification title.</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>Gets or sets notification message body.</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>Gets or sets optional navigation URL associated with the notification.</summary>
        public string? NavigationUrl { get; set; } = string.Empty;

        /// <summary>Gets or sets a value indicating whether indicates whether the notification has been read.</summary>
        public bool IsRead { get; set; }
    }
}