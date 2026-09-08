namespace SharedServices
{
    using Application;
    using Domain;
    using Infrastructure;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Provides functionality to persist audit log entries to the application's data store.
    /// </summary>
    /// <remarks>This service is typically used to record audit events for tracking and compliance purposes.
    /// It supports both synchronous and asynchronous operations for saving audit logs. Instances of this class are
    /// intended to be used within the application's dependency injection framework.</remarks>
    public class AuditLogService : IAuditLogService
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditLogService"/> class using the specified service provider.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies required by the audit log service. Cannot be null.</param>
        public AuditLogService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Saves a collection of audit log entries to the database.
        /// </summary>
        /// <remarks>Each entry in <paramref name="auditLogs"/> is added to the database in a single
        /// operation. The method creates a new service scope for the database context. Changes are committed
        /// immediately. This method is not thread-safe.</remarks>
        /// <param name="auditLogs">The list of <see cref="AuditLog"/> entries to be persisted. Cannot be null.</param>
        public void SaveLogs(List<AuditLog> auditLogs)
        {
            using (var scope = this.serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                context.AuditLogs.AddRange(auditLogs);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Asynchronously saves a collection of audit log entries to the database.
        /// </summary>
        /// <param name="auditLogs">The list of <see cref="AuditLog"/> entries to be persisted. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        public async Task SaveLogsAsync(List<AuditLog> auditLogs)
        {
            using (var scope = this.serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                await context.AuditLogs.AddRangeAsync(auditLogs);
                await context.SaveChangesAsync();
            }
        }
    }

    /// <summary>
    /// Provides contextual information and configuration for an audit operation, including options to skip auditing and
    /// track entities or logs involved in the audit.
    /// </summary>
    /// <remarks>An instance of AuditScope is typically used to control and record the auditing process for a
    /// set of operations. It allows selective exclusion of entities from auditing and maintains a collection of audit
    /// logs generated during the scope's lifetime. This class is intended to be used as part of an auditing framework
    /// to manage audit-related state and behavior.</remarks>
    public class AuditScope : IAuditScope
    {
        /// <summary>
        /// Gets or sets a value indicating whether the current audit operation should be skipped. When set to true, no audit logs will be generated for the operations performed within this scope.
        /// </summary>
        public bool SkipAudit { get; set; }

        /// <summary>
        /// Gets a collection of entity types that should be excluded from auditing. Any operations involving entities of these types will not generate audit logs. This allows for fine-grained control over which parts of the application are audited, enabling developers to focus on critical entities while ignoring less important ones.
        /// </summary>
        public HashSet<Type> SkipEntities { get; } = [];

        /// <summary>
        /// Gets or sets the collection of audit logs currently available for review.
        /// </summary>
        public List<AuditLog>? CurrentAuditLogs { get; set; }
    }
}