namespace Domain
{
    using System.ComponentModel.DataAnnotations.Schema;

    using SharedServices;

    /// <summary>
    /// Represents user-specific notification settings.
    /// </summary>
    [Table("NotificationSetting")]
    [EntityDisplayName("Notification Settings")]
    public class NotificationSetting : Auditable, IIdentifiable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationSetting"/> class.
        /// Default constructor.
        /// </summary>
        public NotificationSetting()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationSetting"/> class.
        /// Constructs an entity from the Add/Edit DTO.
        /// </summary>
        /// <param name="dto">DTO containing notification settings.</param>
        public NotificationSetting(NotificationSetting_AddEdit dto)
        {
            this.Id = dto.Id ?? Guid.NewGuid();
            this.UserId = dto.UserId;
            this.NotificationsType = dto.NotificationsType;
            this.QuoteProposalNotification = dto.QuoteProposalNotification;
            this.WorkOrderStatusChange = dto.WorkOrderStatusChange;
            this.NewPublicNoteonWorkOrder = dto.NewPublicNoteonWorkOrder;
            this.WorkOrderCompleted = dto.WorkOrderCompleted;
            this.Status = dto.Status ?? StatusType.Active;
        }

        /// <summary>
        /// Gets or sets primary identifier of the notification setting.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the user this notification setting belongs to.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets type of notifications.
        /// </summary>
        [AuditDisplay(UseEnumDisplay = true)]
        public NotificationType NotificationsType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether notify user for quote/proposal events.
        /// </summary>
        public bool QuoteProposalNotification { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether notify user when work order status changes.
        /// </summary>
        public bool WorkOrderStatusChange { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether notify user when a new public note is added on a work order.
        /// </summary>
        public bool NewPublicNoteonWorkOrder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether notify user when a work order is completed.
        /// </summary>
        public bool WorkOrderCompleted { get; set; }

        /// <summary>
        /// Gets or sets application-level status (active/inactive).
        /// </summary>
        public StatusType Status { get; set; } = StatusType.Active;
    }
}