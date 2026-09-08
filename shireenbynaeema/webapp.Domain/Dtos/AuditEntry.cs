namespace Domain
{
    /// <summary>
    /// Represents a single property change captured during synchronization auditing.
    /// </summary>
    public class AuditEntry
    {
        /// <summary>Gets or sets name of the modified property.</summary>
        public string PropertyName { get; set; } = string.Empty;

        /// <summary>Gets or sets value before the change occurred.</summary>
        public object? OldValue { get; set; }

        /// <summary>Gets or sets value after the change occurred.</summary>
        public object? NewValue { get; set; }
    }

    /// <summary>
    /// Response model containing synchronization summary and detailed audit logs.
    /// </summary>
    public class SyncWorkOrderResponse
    {
        /// <summary>Gets or sets aggregated result statistics.</summary>
        public ResultSummaryData ResultSummary { get; set; } = new ResultSummaryData();

        /// <summary>Gets or sets audit trail entries for processed work orders.</summary>
        public List<AuditLogData> AuditLogs { get; set; } = [];
    }

    /// <summary>
    /// Summary counts of processed work order operations.
    /// </summary>
    public class ResultSummaryData
    {
        /// <summary>Gets or sets number of work orders dispatched.</summary>
        public int Dispatched { get; set; }

        /// <summary>Gets or sets number of status updates applied.</summary>
        public int StatusUpdates { get; set; }

        /// <summary>Gets or sets number of work orders completed.</summary>
        public int Completed { get; set; }

        /// <summary>Gets or sets number of newly created work orders.</summary>
        public int New { get; set; }
    }

    /// <summary>
    /// Audit log information associated with a specific work order.
    /// </summary>
    public class AuditLogData
    {
        /// <summary>Gets or sets work order identifier.</summary>
        public string WOID { get; set; } = string.Empty;

        /// <summary>Gets or sets action performed on the work order.</summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>Gets or sets collection of detected field changes.</summary>
        public List<AuditEntry> Changes { get; set; } = [];
    }

    /// <summary>
    /// Represents a view of a change in an activity log, including the affected field and related context information.
    /// </summary>
    public class ActivityLogChangeView
    {
        /// <summary>
        /// Gets or sets the value of the field.
        /// </summary>
        public string Field { get; set; } = default!;

        /// <summary>
        /// Gets or sets the text content that appears before the main element.
        /// </summary>
        public string? Before { get; set; }

        /// <summary>
        /// Gets or sets the cursor indicating the position after which results should be returned in a paginated query.
        /// </summary>
        public string? After { get; set; }
    }

    /// <summary>
    /// Represents a row in an audit comparison, containing the field name and its values before and after a change.
    /// </summary>
    public class AuditCompareRow
    {
        /// <summary>
        /// Gets or sets column name shown in UI (ex: "Assign Role").
        /// </summary>
        public string Field { get; set; } = default!;

        /// <summary>
        /// Gets or sets previous value.
        /// </summary>
        public string? Before { get; set; }

        /// <summary>
        /// Gets or sets new value.
        /// </summary>
        public string? After { get; set; }
    }

    /// <summary>
    /// Provides cached lookup data for audit operations, including user and entity mappings.
    /// </summary>
    /// <remarks>The cache enables efficient retrieval of display names for users and entities during audit
    /// processing. The contents are intended to be populated prior to audit queries and are read-only after
    /// initialization.</remarks>
    public class AuditLookupCache
    {
        /// <summary>
        /// Gets a collection of user identifiers and their associated names.
        /// </summary>
        public Dictionary<Guid, string> Users { get; } = [];

        /// <summary>
        /// Gets the collection of entities grouped by type and identified by unique IDs.
        /// </summary>
        /// <remarks>Each entry in the outer dictionary represents an entity type, with the key being the
        /// type name. The inner dictionary maps unique entity identifiers to their corresponding string values. The
        /// property is read-only and always returns a valid dictionary, which may be empty if no entities are
        /// present.</remarks>
        public Dictionary<string, Dictionary<Guid, string>> Entities { get; } = [];
    }
}