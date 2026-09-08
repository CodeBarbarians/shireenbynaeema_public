namespace Domain
{
    using System.ComponentModel.DataAnnotations.Schema;

    using SharedServices;

    /// <summary>
    /// Stores user activity history for tracking data changes and security auditing.
    /// </summary>
    [Table("AuditLog")]
    [EntityDisplayName("Audit Log")]
    public class AuditLog
    {
        /// <summary>Gets or sets primary identifier of the audit record.</summary>
        public Guid Id { get; set; }

        /// <summary>Gets or sets unix timestamp when the action occurred.</summary>
        public long Timestamp { get; set; }

        /// <summary>Gets or sets identifier of the user who performed the action.</summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>Gets or sets display username of the acting user.</summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>Gets or sets iP address from which the action was executed.</summary>
        public string? IPAddress { get; set; }

        /// <summary>Gets or sets type of operation performed (Create, Update, Delete, etc.).</summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>Gets or sets name of the affected entity/table.</summary>
        public string EntityName { get; set; } = string.Empty;

        /// <summary>Gets or sets primary key value of the affected record.</summary>
        public string? EntityId { get; set; }

        /// <summary>Gets or sets serialized JSON describing field-level changes.</summary>
        public string Changes { get; set; } = string.Empty;
    }
}