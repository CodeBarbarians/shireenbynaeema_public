namespace Domain
{
    /// <summary>
    /// Email notification payload when a note is added to a work order for general users.
    /// </summary>
    public class NotesEmailNotification
    {
        /// <summary>Gets or sets recipient user.</summary>
        public User User { get; set; } = new User();

        /// <summary>Gets or sets work order identifier.</summary>
        public string? WorkOrderId { get; set; }

        /// <summary>Gets or sets name of the person who added the note.</summary>
        public string? AddedBy { get; set; }

        /// <summary>Gets or sets date the note was added.</summary>
        public string? AddedOn { get; set; }

        /// <summary>Gets or sets note content.</summary>
        public string? Note { get; set; }

        /// <summary>Gets or sets account manager name.</summary>
        public string? AccountManagerName { get; set; }

        /// <summary>Gets or sets account manager email.</summary>
        public string? AccountManagerEmail { get; set; }

        /// <summary>Gets or sets sub-client name.</summary>
        public string? ClientName { get; set; }

        /// <summary>Gets or sets portal link to the work order.</summary>
        public string? Link { get; set; }

        /// <summary>Gets or sets site address.</summary>
        public string? SiteAddress { get; set; }

        /// <summary>Gets or sets trade or service type.</summary>
        public string? Trade { get; set; }

        /// <summary>Gets or sets priority description.</summary>
        public string? Priority { get; set; }

        /// <summary>Gets or sets service description.</summary>
        public string? ServiceDescription { get; set; }

        /// <summary>Gets or sets resolution notes.</summary>
        public string? ResolutionNotes { get; set; }
    }

    /// <summary>
    /// Email notification payload when a note is added, targeted for facility team members.
    /// </summary>
    public class NotesEmailNotificationFacilityTeam
    {
        /// <summary>Gets or sets work order identifier.</summary>
        public string? WorkOrderId { get; set; }

        /// <summary>Gets or sets name of the person who added the note.</summary>
        public string? AddedBy { get; set; }

        /// <summary>Gets or sets sub-client name.</summary>
        public string? ClientName { get; set; }

        /// <summary>Gets or sets date the note was added.</summary>
        public string? AddedOn { get; set; }

        /// <summary>Gets or sets note content.</summary>
        public string? Note { get; set; }

        /// <summary>Gets or sets portal link to the work order.</summary>
        public string? Link { get; set; }

        /// <summary>Gets or sets account manager name.</summary>
        public string? AccountManagerName { get; set; }

        /// <summary>Gets or sets account manager email.</summary>
        public string? AccountManagerEmail { get; set; }
    }

    /// <summary>
    /// Email notification payload when a work order is completed.
    /// </summary>
    public class CompleteEmailNotification
    {
        /// <summary>Gets or sets recipient user.</summary>
        public User User { get; set; } = new User();

        /// <summary>Gets or sets work order identifier.</summary>
        public string? WorkOrderId { get; set; }

        /// <summary>Gets or sets portal link to the work order.</summary>
        public string? Link { get; set; }

        /// <summary>Gets or sets site address.</summary>
        public string? SiteAddress { get; set; }

        /// <summary>Gets or sets trade or service type.</summary>
        public string? Trade { get; set; }

        /// <summary>Gets or sets priority description.</summary>
        public string? Priority { get; set; }

        /// <summary>Gets or sets account manager name.</summary>
        public string? AccountManagerName { get; set; }

        /// <summary>Gets or sets account manager email.</summary>
        public string? AccountManagerEmail { get; set; }

        /// <summary>Gets or sets service description.</summary>
        public string? ServiceDescription { get; set; }

        /// <summary>Gets or sets resolution notes.</summary>
        public string? ResolutionNotes { get; set; }
    }

    /// <summary>
    /// Email notification payload when a work order status changes.
    /// </summary>
    public class StatusUpdateEmailNotification
    {
        /// <summary>Gets or sets recipient user.</summary>
        public User User { get; set; } = new User();

        /// <summary>Gets or sets work order identifier.</summary>
        public string WorkOrderId { get; set; } = string.Empty;

        /// <summary>Gets or sets portal link to the work order.</summary>
        public string Link { get; set; } = string.Empty;

        /// <summary>Gets or sets account manager name.</summary>
        public string AccountManagerName { get; set; } = string.Empty;

        /// <summary>Gets or sets account manager email.</summary>
        public string AccountManagerEmail { get; set; } = string.Empty;

        /// <summary>Gets or sets new status description.</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>Gets or sets date and time of the update.</summary>
        public string UpdatedOn { get; set; } = string.Empty;

        /// <summary>Gets or sets user who performed the update.</summary>
        public string UpdatedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response containing email sending errors.
    /// </summary>
    public class EmailErrorResponse
    {
        /// <summary>Gets or sets list of email errors.</summary>
        public List<Error> Errors { get; set; } = [];
    }

    /// <summary>
    /// Represents a single email error.
    /// </summary>
    /// <param name="Field">Field associated with the error.</param>
    /// <param name="Help">Help or resolution guidance.</param>
    public record Error(string? Field, string? Help)
    {
        /// <summary>Gets or sets error message.</summary>
        public string Message { get; set; } = string.Empty;
    }
}