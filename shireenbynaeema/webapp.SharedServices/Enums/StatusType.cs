namespace SharedServices
{
    using System.ComponentModel.DataAnnotations;
    using System.Reflection;

    /// <summary>
    /// Specifies the type of media associated with an operation, such as photos taken before or after an event, other
    /// media, or a signature.
    /// </summary>
    public enum MediaType
    {
        /// <summary>
        /// A photo captured before the work or event has started.
        /// </summary>
        [Display(Name = "Before Photo")]
        BeforePhoto = 1,

        /// <summary>
        /// A photo captured after the work or event has been completed.
        /// </summary>
        [Display(Name = "After Photo")]
        AfterPhoto = 2,

        /// <summary>
        /// Any other media type not specifically categorized.
        /// </summary>
        [Display(Name = "Other")]
        Other = 3,

        /// <summary>
        /// A captured signature from the client or technician.
        /// </summary>
        [Display(Name = "Signature")]
        Signature = 4,
    }

    /// <summary>
    /// Enumeration representing different types of media that can be associated with an estimate, such as estimate attachments and additional attachments.
    /// This enum is used to categorize and manage media files related to estimates within the application, allowing for clear identification and organization
    /// of media based on its type. Each value corresponds to a specific media type, enabling efficient handling and processing of media files in relation to
    /// estimates and their associated workflows.
    /// </summary>
    public enum EstimateMediaType
    {
        /// <summary>
        /// Primary attachments related to the estimate.
        /// </summary>
        [Display(Name = "Estimate Attachments")]
        EstimateAttachments = 1,

        /// <summary>
        /// Additional or supplementary attachments for the estimate.
        /// </summary>
        [Display(Name = "Additional Attachments")]
        AdditionalAttachments = 2,
    }

    /// <summary>
    /// Enumeration representing different types of work orders, such as time and materials or estimate. This enum is used to categorize and manage work orders based on their type within the application,
    /// allowing for clear identification and handling of work orders according to their specific characteristics and requirements. Each value corresponds to a
    /// specific type of work order, enabling efficient processing and management of work orders based on their type.
    /// </summary>
    public enum WorkOrderType
    {
        /// <summary>
        /// Work order billed based on actual time and materials used.
        /// </summary>
        [Display(Name = "Time and Materials")]
        TimeandMaterials = 1,

        /// <summary>
        /// Work order based on a predefined estimate.
        /// </summary>
        [Display(Name = "Estimate")]
        Estimate = 2,
    }

    /// <summary>
    /// Enumeration representing reasons for being unable to perform a certain action, such as client refusal, client unavailability, or other reasons. This enum is used to capture and categorize the reasons for inability to perform an action within the application,
    /// allowing for better handling and communication of these reasons throughout the system.
    /// </summary>
    public enum UnableReason
    {
        /// <summary>
        /// The client refused the service or work.
        /// </summary>
        [Display(Name = "Client Refused")]
        ClientRefused = 1,

        /// <summary>
        /// The client was not available at the time of service.
        /// </summary>
        [Display(Name = "Client not available")]
        ClientNotAvailable = 2,

        /// <summary>
        /// Any other reason not specifically defined.
        /// </summary>
        [Display(Name = "Other")]
        Other = 3,
    }

    /// <summary>
    /// Enumeration representing whether a signature has been obtained or not, with values for obtained and not obtained. This enum is used to track the status of signature collection within the application,
    /// allowing for clear identification and management of signature-related workflows.
    /// </summary>
    public enum SignatureObtained
    {
        /// <summary>
        /// Signature has been successfully obtained.
        /// </summary>
        [Display(Name = "Obtained")]
        Obtained = 1,

        /// <summary>
        /// Signature was not obtained.
        /// </summary>
        [Display(Name = "Not Obtained")]
        NotObtained = 2,
    }

    /// <summary>
    /// Enumeration representing the steps involved in a process, such as signing in, taking photos, selecting job outcomes, obtaining signatures, etc. This enum is used to track the current step of a process within the application,
    /// allowing for clear communication and management of the process as it progresses through different stages. Each value corresponds to a specific step,
    /// enabling efficient handling and processing of the workflow based on the current step. Additionally, there are values for unknown steps, finished processes,
    /// and signed-out states to account for various scenarios that may occur during the workflow.
    /// </summary>
    public enum Step
    {
        /// <summary>
        /// Step 1: User signs in to start the process.
        /// </summary>
        [Display(Name = "Sign In")]
        Step1_SignIn = 1,

        /// <summary>
        /// Step 2: Capture before photos prior to starting work.
        /// </summary>
        [Display(Name = "Before Photos")]
        Step2_BeforePhotos = 2,

        /// <summary>
        /// Step 3: Select the job outcome or work result.
        /// </summary>
        [Display(Name = "Job Outcome Selection")]
        Step3_JobOutcomeSelect = 3,

        /// <summary>
        /// Step 4: Capture after photos once work is completed.
        /// </summary>
        [Display(Name = "After Photos")]
        Step4_AfterPhotos = 4,

        /// <summary>
        /// Step 5: Capture customer or technician signature.
        /// </summary>
        [Display(Name = "Signature Capture")]
        Step5_Signature = 5,

        /// <summary>
        /// Step 2.1: Resume the process after interruption.
        /// </summary>
        [Display(Name = "Resume")]
        Step2_1_Resume = 6,

        /// <summary>
        /// Unknown or undefined step.
        /// </summary>
        [Display(Name = "Unknown")]
        Unknown = 99,

        /// <summary>
        /// Indicates the workflow has been completed.
        /// </summary>
        [Display(Name = "Finished")]
        Finished = 100,

        /// <summary>
        /// User has signed out of the workflow.
        /// </summary>
        [Display(Name = "Signed Out")]
        SignedOut = 101,
    }

    /// <summary>
    /// Enumeration representing various status values used across the application.
    /// </summary>
    public enum StatusType
    {
        /// <summary>
        /// Entity is inactive and not currently in use.
        /// </summary>
        [Display(Name = "Inactive")]
        InActive = 0,

        /// <summary>
        /// Entity is active and available for use.
        /// </summary>
        [Display(Name = "Active")]
        Active = 1,

        /// <summary>
        /// Entity is pending action or approval.
        /// </summary>
        [Display(Name = "Pending")]
        Pending = 2,

        /// <summary>
        /// Entity has been accepted.
        /// </summary>
        [Display(Name = "Accepted")]
        Accepted = 3,

        /// <summary>
        /// Entity has been approved.
        /// </summary>
        [Display(Name = "Approved")]
        Approved = 4,

        /// <summary>
        /// Entity has been rejected.
        /// </summary>
        [Display(Name = "Rejected")]
        Rejected = 5,

        /// <summary>
        /// Entity is locked and cannot be modified.
        /// </summary>
        [Display(Name = "Locked")]
        Locked = 6,

        /// <summary>
        /// Entity is marked as not used.
        /// </summary>
        [Display(Name = "Not Used")]
        NotUsed = 7,

        /// <summary>
        /// Entity is currently in use.
        /// </summary>
        [Display(Name = "Used")]
        Used = 8,

        /// <summary>
        /// Entity has been invited (e.g., user invitation).
        /// </summary>
        [Display(Name = "Invited")]
        Invited = 9,

        /// <summary>
        /// Entity has been deleted (soft delete).
        /// </summary>
        [Display(Name = "Deleted")]
        Deleted = 10,
    }

    /// <summary>
    /// Enumeration representing actions that can be performed, such as adding, updating, or deleting records. This enum is commonly used in audit logging to
    /// track the type of action taken on an entity, providing a clear and standardized way to record changes made within the application. Each value corresponds
    /// to a specific action, allowing for easy categorization and analysis of audit logs based on the type of operation performed.
    /// </summary>
    public enum Action
    {
        /// <summary>
        /// Indicates creation of a new entity.
        /// </summary>
        [Display(Name = "Add")]
        Add = 1,

        /// <summary>
        /// Indicates modification of an existing entity.
        /// </summary>
        [Display(Name = "Update")]
        Update = 2,

        /// <summary>
        /// Indicates deletion of an entity.
        /// </summary>
        [Display(Name = "Delete")]
        Delete = 3,
    }

    /// <summary>
    /// Enumeration representing the status of an estimate, which can be pending, accepted, or rejected. This enum is used to track the current state of an estimate within the application,
    /// allowing for clear communication and management of estimates as they progress through different stages of approval. Each value corresponds to a specific status,
    /// enabling efficient handling and processing of estimates based on their current state.
    /// </summary>
    public enum EstimateStatus
    {
        /// <summary>
        /// Estimate is created and awaiting client response.
        /// </summary>
        [Display(Name = "Pending")]
        Pending = 1,

        /// <summary>
        /// Estimate has been accepted by the client.
        /// </summary>
        [Display(Name = "Accepted")]
        Accepted = 2,

        /// <summary>
        /// Estimate has been rejected by the client.
        /// </summary>
        [Display(Name = "Rejected")]
        Rejected = 3,
    }

    /// <summary>
    /// Enumeration representing the type of estimate, which can be either an initial estimate or a post-approval estimate. This enum is used to differentiate between different types of estimates within the application,
    /// allowing for better organization and management of estimates based on their specific context and purpose.
    /// </summary>
    public enum EstimateType
    {
        /// <summary>
        /// Standard estimate created before approval.
        /// </summary>
        [Display(Name = "Estimate")]
        Estimate = 1,

        /// <summary>
        /// Estimate created or adjusted after approval.
        /// </summary>
        [Display(Name = "Post-Approval Estimate")]
        PostApproval = 2,
    }

    /// <summary>
    /// Enumeration representing the status of a work order, which can be in various stages such as new, dispatched, completed, etc. This enum is used to track the current state of a work order within the application,
    /// allowing for clear communication and management of work orders as they progress through different stages. Each value corresponds to a specific status,
    /// enabling efficient handling and processing of work orders based on their current state.
    /// </summary>
    public enum WorkOrderStatus
    {
        /// <summary>
        /// Indicates that no options are set.
        /// </summary>
        None = 0,

        /// <summary>
        /// Work order is newly created.
        /// </summary>
        [Display(Name = "New")]
        New = 1,

        /// <summary>
        /// Notification has been sent for the work order.
        /// </summary>
        [Display(Name = "Notification Sent")]
        NotificationSent = 2,

        /// <summary>
        /// Work order has been dispatched to a technician.
        /// </summary>
        [Display(Name = "Dispatched")]
        Dispatched = 3,

        /// <summary>
        /// Work order has been completed.
        /// </summary>
        [Display(Name = "Completed")]
        Completed = 4,

        /// <summary>
        /// Additional information has been requested.
        /// </summary>
        [Display(Name = "Information Request")]
        InformationRequest = 5,

        /// <summary>
        /// Approval has been requested for the work order.
        /// </summary>
        [Display(Name = "Approval Request")]
        ApprovalRequest = 6,

        /// <summary>
        /// Work order is currently in progress.
        /// </summary>
        [Display(Name = "Work In Progress")]
        WorkInProgress = 7,

        /// <summary>
        /// Work order is temporarily suspended.
        /// </summary>
        [Display(Name = "Suspended")]
        Suspended = 8,

        /// <summary>
        /// Work order has been declined.
        /// </summary>
        [Display(Name = "Declined")]
        Declined = 9,

        /// <summary>
        /// Work order has been cancelled.
        /// </summary>
        [Display(Name = "Cancelled")]
        Cancelled = 26,

        /// <summary>
        /// Estimate has been requested for the work order.
        /// </summary>
        [Display(Name = "Estimate Requested")]
        EstimateRequested = 27,

        /// <summary>
        /// Estimate has been approved.
        /// </summary>
        [Display(Name = "Estimate Approved")]
        EstimateApproved = 28,

        /// <summary>
        /// Estimate has been submitted.
        /// </summary>
        [Display(Name = "Estimate Submitted")]
        EstimateSubmitted = 29,

        /// <summary>
        /// Work order is pending verification.
        /// </summary>
        [Display(Name = "Pending Verification")]
        PendingVerification = 30,

        /// <summary>
        /// Estimate is required before proceeding.
        /// </summary>
        [Display(Name = "Estimate Required")]
        EstimateRequired = 31,

        /// <summary>
        /// Technician is currently on site.
        /// </summary>
        [Display(Name = "On Site")]
        OnSite = 32,

        /// <summary>
        /// Technician is off site.
        /// </summary>
        [Display(Name = "Off Site")]
        OffSite = 33,

        /// <summary>
        /// Work order data is not fully synchronized.
        /// </summary>
        [Display(Name = "Not Fully Synced")]
        NotFullySynced = 99,
    }

    /// <summary>
    /// Static class providing a list of work order types with their display names and IDs for use in dropdowns,
    /// selection lists, or other UI components.
    /// </summary>
    public static class WorkOrderTypeEnumExtension
    {
        /// <summary>
        /// Gets a list of WOTypes objects representing the different work order types defined in the WorkOrderType enum.
        /// This property uses reflection to iterate through the enum values, retrieve their associated Display attributes, and create a list of WOTypes with the corresponding Id and Name properties.
        /// </summary>
        public static List<WOTypes> WorkOrderTypes { get; } =
        [.. Enum.GetValues<WorkOrderType>()
            .Cast<WorkOrderType>()
            .Select(e =>
            {
                var member = typeof(WorkOrderType).GetMember(e.ToString()).First();
                var display = member.GetCustomAttribute<DisplayAttribute>();

                return new WOTypes
                {
                    Id = (int)e,
                    Name = display?.Name ?? e.ToString(),
                };
            })];
    }

    /// <summary>
    /// Class representing work order types, with properties for Id and Name. This class is used to define and manage different types of work orders within the
    /// application, allowing for clear identification and categorization of work orders based on their type. The Id property serves as a unique identifier for
    /// each work order type, while the Name property provides a user-friendly name for display purposes. This class can be used in conjunction with the
    /// WorkOrderType enum to provide a more comprehensive representation of work order types in the application.
    /// </summary>
    public class WOTypes
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name associated with the object.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}