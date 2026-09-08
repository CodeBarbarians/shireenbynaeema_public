namespace Domain
{
    /// <summary>
    /// Data transfer object representing a formatted activity log entry
    /// suitable for presentation in UI or reports.
    /// </summary>
    public class ActivityLogDto
    {
        /// <summary>Gets or sets unique identifier of the log entry.</summary>
        public Guid Id { get; set; }

        /// <summary>Gets or sets timestamp of the activity in UTC.</summary>
        public DateTime Timestamp { get; set; }

        /// <summary>Gets or sets unix timestamp used for sorting and filtering.</summary>
        public long TimestampUnix { get; set; }

        /// <summary>Gets or sets action performed (e.g., Login, Update, Delete).</summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>Gets or sets module where the action occurred.</summary>
        public string Module { get; set; } = string.Empty;

        /// <summary>Gets or sets status of the activity (e.g., Success, Failed).</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>Gets or sets username of the actor.</summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>Gets or sets email of the actor.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Gets or sets iP address of the client performing the action.</summary>
        public string IPAddress { get; set; } = string.Empty;

        /// <summary>Gets or sets human-readable description of the activity.</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the date as a formatted string suitable for display.
        /// </summary>
        public string FormattedDate { get; set; } = string.Empty; // e.g., "Jan 13, 2026"

        /// <summary>
        /// Gets or sets the time value formatted as a string suitable for display.
        /// </summary>
        public string FormattedTime { get; set; } = string.Empty; // e.g., "9:48 AM"

        /// <summary>
        /// Gets or sets a value indicating whether additional details are available for the current item.
        /// </summary>
        public bool HasDetails { get; set; }

        /// <summary>
        /// Gets or sets the collection of activity log changes associated with this entry.
        /// </summary>
        public List<ActivityLogChangeView>? Details { get; set; }
    }

    /// <summary>
    /// Raw activity log model representing stored log data before formatting.
    /// Typically used for database mapping or internal processing.
    /// </summary>
    public class ActivityLogRaw
    {
        /// <summary>Gets or sets unique identifier of the log entry.</summary>
        public Guid Id { get; set; }

        /// <summary>Gets or sets unix timestamp of the activity.</summary>
        public long Timestamp { get; set; }

        /// <summary>Gets or sets action performed.</summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>Gets or sets module where the action occurred.</summary>
        public string Module { get; set; } = string.Empty;

        /// <summary>Gets or sets status of the activity.</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>Gets or sets username of the actor.</summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>Gets or sets email of the actor.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Gets or sets iP address of the client.</summary>
        public string IPAddress { get; set; } = string.Empty;

        /// <summary>Gets or sets description of the activity.</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Gets or sets user agent of the client device.</summary>
        public string? UserAgent { get; set; }

        /// <summary>Gets or sets name of the affected entity.</summary>
        public string? EntityName { get; set; }

        /// <summary>Gets or sets identifier of the affected entity.</summary>
        public string? EntityId { get; set; }

        /// <summary>Gets or sets serialized change details.</summary>
        public string? Changes { get; set; }

        /// <summary>Gets or sets source database table of the log.</summary>
        public string SourceTable { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether additional details are available for the current item.
        /// </summary>
        public bool HasDetails { get; set; }
    }

    /// <summary>
    /// Represents filter options for querying activity logs, including actions, modules, and statuses.
    /// </summary>
    /// <remarks>Use this class to specify criteria when retrieving activity log entries. Each list property
    /// allows filtering by its respective category. If a list is empty, no filtering is applied for that
    /// category.</remarks>
    public class ActivityLogFilterOptionsDto
    {
        /// <summary>
        /// Gets or sets the collection of action names associated with this instance.
        /// </summary>
        /// <remarks>The list contains the names of actions that can be performed or are relevant in the
        /// current context. Modifying the collection affects which actions are available. The list may be empty if no
        /// actions are defined.</remarks>
        public List<string> Actions { get; set; } = [];

        /// <summary>
        /// Gets or sets the collection of module names associated with the current instance.
        /// </summary>
        public List<string> Modules { get; set; } = [];

        /// <summary>
        /// Gets or sets the collection of status strings associated with the current instance.
        /// </summary>
        public List<string> Statuses { get; set; } = [];
    }
}