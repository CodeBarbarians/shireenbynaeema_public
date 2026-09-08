namespace Domain
{
    using System.ComponentModel.DataAnnotations.Schema;

    using SharedServices;

    /// <summary>
    /// Represents a permission assigned to a role.
    /// </summary>
    [Table("RolePermission")]
    [EntityDisplayName("Role Permission")]
    public class RolePermission : Auditable
    {
        /// <summary>Gets or sets foreign key to the associated Role.</summary>
        [ForeignKey(nameof(Role))]
        [AuditDisplay(typeof(Role), nameof(Role.Name))]
        public Guid RoleId { get; set; }

        /// <summary>Gets or sets navigation property to the Role entity.</summary>
        public Role Role { get; set; } = default!;

        /// <summary>Gets or sets name of the permission assigned to the role.</summary>
        public string PermissionName { get; set; } = default!;
    }
}