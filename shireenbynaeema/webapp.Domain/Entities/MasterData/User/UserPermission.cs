namespace Domain
{
    using System.ComponentModel.DataAnnotations.Schema;

    using SharedServices;

    /// <summary>
    /// Represents a permission assigned to a specific user.
    /// </summary>
    [Table("UserPermission")]
    [EntityDisplayName("User Permission")]
    public class UserPermission : Auditable
    {
        /// <summary>Gets or sets foreign key to the associated User.</summary>
        [ForeignKey(nameof(User))]
        [AuditDisplay(typeof(User), nameof(User.DisplayName))]
        public Guid UserId { get; set; }

        /// <summary>Gets or sets navigation property to the User entity.</summary>
        public User User { get; set; } = default!;

        /// <summary>Gets or sets name of the permission assigned to the user.</summary>
        public string PermissionName { get; set; } = default!;
    }
}