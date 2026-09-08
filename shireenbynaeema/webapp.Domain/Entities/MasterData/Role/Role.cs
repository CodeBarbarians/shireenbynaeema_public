namespace Domain
{
    using System.ComponentModel.DataAnnotations.Schema;

    using SharedServices;

    /// <summary>
    /// Represents a Role entity in the system with related users and permissions.
    /// </summary>
    [Table("Role")]
    [EntityDisplayName("Role")]
    public class Role : Auditable, IIdentifiable, ISoftDeletable
    {
        /// <summary>Gets or sets primary key of the Role entity.</summary>
        public Guid Id { get; set; }

        /// <summary>Gets or sets name of the role.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Gets or sets description of the role and its purpose.</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets distinguish if Role is for Organization or Facility.
        /// </summary>
        [AuditDisplay(UseEnumDisplay = true)]
        public RoleScope? Scope { get; set; }

        /// <summary>Gets or sets status of the role (1 = active, 0 = inactive).</summary>
        [AuditDisplay(UseEnumDisplay = true)]
        public StatusType Status { get; set; } = StatusType.Active;

        /// <summary>Gets or sets collection of UserRole entities associated with this role.</summary>
        public ICollection<UserRole> UserRoles { get; set; } = [];

        /// <summary>Gets or sets collection of RolePermission entities assigned to this role.</summary>
        public ICollection<RolePermission> RolePermissions { get; set; } = [];

        /// <summary>
        /// Gets or sets a value indicating whether the entity has been marked as deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the timestamp indicating when the entity was deleted.
        /// </summary>
        public long? DeletedOn { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who deleted the entity.
        /// </summary>
        public Guid? DeletedBy { get; set; }
    }
}