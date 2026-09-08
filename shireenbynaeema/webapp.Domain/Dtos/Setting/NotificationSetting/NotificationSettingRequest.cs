namespace Domain
{
using SharedServices;

/// <summary>
/// Model used for creating or updating user notification preferences.
/// Defines which events should trigger notifications for a user.
/// </summary>
public class NotificationSetting_AddEdit
{
    /// <summary>Gets or sets unique identifier of the notification setting.</summary>
    public Guid? Id { get; set; }

    /// <summary>Gets or sets identifier of the user.</summary>
    public Guid UserId { get; set; }

    /// <summary>Gets or sets type of notification configuration.</summary>
    public NotificationType NotificationsType { get; set; }

    /// <summary>Gets or sets a value indicating whether indicates whether quote proposal notifications are enabled.</summary>
    public bool QuoteProposalNotification { get; set; }

    /// <summary>Gets or sets a value indicating whether indicates whether work order status change notifications are enabled.</summary>
    public bool WorkOrderStatusChange { get; set; }

    /// <summary>Gets or sets a value indicating whether indicates whether notifications for new public notes on work orders are enabled.</summary>
    public bool NewPublicNoteonWorkOrder { get; set; }

    /// <summary>Gets or sets a value indicating whether indicates whether work order completion notifications are enabled.</summary>
    public bool WorkOrderCompleted { get; set; }

    /// <summary>Gets or sets status of the notification setting.</summary>
    public StatusType? Status { get; set; }
}

/// <summary>
/// Request model containing a collection of notification settings to be saved or updated.
/// </summary>
public class NotificationSettingRequest
{
    /// <summary>Gets or sets list of notification settings.</summary>
    public List<NotificationSetting_AddEdit> Settings { get; set; } = [];
}
}