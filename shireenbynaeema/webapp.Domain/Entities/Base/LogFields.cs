namespace Domain
{
    /// <summary>
    /// Base class providing audit tracking fields for entity creation and modification.
    /// </summary>
    public abstract class Auditable
    {
        /// <summary>Gets or sets creation timestamp in Unix format.</summary>
        public long? CreatedOn { get; set; }

        /// <summary>Gets or sets last update timestamp in Unix format.</summary>
        public long? UpdatedOn { get; set; }

        /// <summary>Gets or sets identifier of the creator.</summary>
        public Guid? CreatedBy { get; set; }

        /// <summary>Gets or sets identifier of the last modifier.</summary>
        public Guid? UpdatedBy { get; set; }
    }

    /// <summary>
    /// Base class for entities supporting both audit tracking and soft deletion.
    /// </summary>
    public abstract class SoftDeletableAuditable : Auditable, ISoftDeletable
    {
        /// <summary>Gets or sets a value indicating whether indicates whether the entity has been soft deleted.</summary>
        public bool IsDeleted { get; set; }

        /// <summary>Gets or sets unix timestamp of deletion.</summary>
        public long? DeletedOn { get; set; }

        /// <summary>Gets or sets identifier of the user who deleted the record.</summary>
        public Guid? DeletedBy { get; set; }
    }

    /// <summary>
    /// Common logging fields used across entities for audit and status tracking.
    /// </summary>
    public class LogFields
    {
        /// <summary>Gets or sets creation timestamp.</summary>
        public long? CreatedOn { get; set; }

        /// <summary>Gets or sets last update timestamp.</summary>
        public long? UpdatedOn { get; set; }

        /// <summary>Gets or sets identifier of the creator.</summary>
        public Guid? CreatedBy { get; set; }

        /// <summary>Gets or sets identifier of the last modifier.</summary>
        public Guid? UpdatedBy { get; set; }

        /// <summary>Gets or sets status identifier.</summary>
        public int? Status { get; set; }
    }
}