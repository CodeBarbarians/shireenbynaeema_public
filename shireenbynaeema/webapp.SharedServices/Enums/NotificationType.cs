namespace SharedServices
{
    /// <summary>
    /// Enumeration representing different types of notifications that can be sent within the application. This enum is used to categorize notifications based on
    /// their delivery method, such as email notifications or internal notifications. The values in this enum can be used to determine how a notification should
    /// be sent to the user, allowing for flexibility in the notification system and enabling the application to support multiple notification channels effectively.
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// Specifies that email notifications are enabled.
        /// </summary>
        EmailNotications = 1,

        /// <summary>
        /// Indicates that the notification type is for internal notifications within the system.
        /// </summary>
        InternalNotications = 2,
    }

    /// <summary>
    /// Enumeration representing different categories of notifications that can be sent within the application. This enum is used to classify notifications based
    /// on their context or purpose, such as notifications related to quote proposals, work order status changes, new public notes, work order completions,
    /// user onboarding, estimate creation and responses, and post-approval creation and responses. By categorizing notifications using this enum, the application
    /// can provide more targeted and relevant notifications to users based on the specific events or actions that occur within the system. This allows for a more
    /// personalized and effective notification experience for users, ensuring that they receive timely and pertinent information about important updates and activities
    /// within the application.
    /// </summary>
    public enum NotificationCategory
    {
        /// <summary>
        /// Indicates that the notification is related to a quote proposal, such as when a new quote is created or updated.
        /// </summary>
        QuoteProposal = 0,

        /// <summary>
        /// Indicates that the status of a work order has changed.
        /// </summary>
        WorkOrderStatusChange = 1,

        /// <summary>
        /// Indicates that a new public note has been added, which may be relevant to users who are following updates or discussions related to a specific work order or project.
        /// </summary>
        NewPublicNote = 2,

        /// <summary>
        /// Indicates that the work order has been completed.
        /// </summary>
        WorkOrderCompleted = 3,

        /// <summary>
        /// Indicates that the notification is related to user onboarding events.
        /// </summary>
        UserOnBoarding = 4,

        /// <summary>
        /// Indicates that the operation is related to the creation of an estimate.
        /// </summary>
        EstimateCreation = 5,

        /// <summary>
        /// Indicates that the operation is related to the response of an estimate, such as when an estimate is approved, rejected, or updated with new information.
        /// </summary>
        EstimateResponse = 6,

        /// <summary>
        /// Indicates that the operation is related to the creation of a post-approval, which may involve notifications about the initiation of a post-approval process or the creation of a new post-approval record.
        /// </summary>
        PostApprovalCreation = 7,

        /// <summary>
        /// Indicates that the operation is related to the response of a post-approval, such as when a post-approval is approved, rejected, or updated with new information.
        /// </summary>
        PostApprovalResponse = 8,
    }
}