namespace Application
{
    using Domain;

    /// <summary>
    /// Interface for audit log services, providing methods to manage audit logs. This service is designed to handle the saving of audit logs, allowing for both synchronous
    /// and asynchronous operations.
    /// </summary>
    public interface IAuditLogService
    {
        /// <summary>
        /// Persists a collection of audit log entries to the underlying storage.
        /// </summary>
        /// <remarks>If the list is empty, no logs are saved. This method does not return until all logs
        /// have been persisted. Thread safety depends on the implementation; consult the class documentation for
        /// details.</remarks>
        /// <param name="auditLogs">The list of audit log entries to be saved. Cannot be null or contain null elements.</param>
        void SaveLogs(List<AuditLog> auditLogs);

        /// <summary>
        /// Asynchronously saves a collection of audit log entries to the underlying storage.
        /// </summary>
        /// <remarks>This method does not block the calling thread. Await the returned task to ensure that
        /// all logs are persisted before proceeding.</remarks>
        /// <param name="auditLogs">The list of audit log entries to be saved. Cannot be null or contain null elements.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        Task SaveLogsAsync(List<AuditLog> auditLogs);
    }

    /// <summary>
    /// Interface for audit scope, providing properties to manage the current audit logs and control whether auditing should be skipped. This interface allows for
    /// fine-grained control over the auditing process, enabling developers to customize the behavior of the audit logging system based on specific requirements.
    /// </summary>
    public interface IAuditScope
    {
        /// <summary>
        /// Gets or sets a value indicating whether indicates whether auditing should be skipped for the current scope. Setting this property to true will prevent any audit logs from being saved for the operations
        /// within this scope.
        /// </summary>
        bool SkipAudit { get; set; }

        /// <summary>
        /// Gets or sets the current list of audit logs for the scope. This property allows for storing and retrieving the audit logs that are being generated within the current scope,
        /// providing a way to access and manipulate the audit logs as needed.
        /// </summary>
        List<AuditLog>? CurrentAuditLogs { get; set; } // ✅ new property

        /// <summary>
        /// Gets a set of entity types that should be skipped for auditing within the current scope. This property allows for specifying certain entities that should not be included in the audit logs,
        /// providing a way to exclude specific tables or entities from the auditing process.
        /// </summary>
        HashSet<Type> SkipEntities { get; } // skip specific tables per request
    }
}