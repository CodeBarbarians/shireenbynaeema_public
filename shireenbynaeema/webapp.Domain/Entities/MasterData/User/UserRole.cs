namespace Domain
{
    using System.ComponentModel.DataAnnotations.Schema;

    using SharedServices;

    /// <summary>
    /// Represents the assignment of a role to a specific user.
    /// Each entry links a User with a Role.
    /// </summary>
    [Table("UserRole")]
    [EntityDisplayName("User Role")]
    public class UserRole
    {
        /// <summary>
        /// Gets or sets the Id of the role assigned to the user.
        /// </summary>
        [ForeignKey(nameof(Role))]
        [AuditDisplay(typeof(Role), nameof(Role.Name))]
        public Guid RoleId { get; set; }

        /// <summary>
        /// Gets or sets navigation property for the associated role.
        /// </summary>
        public Role Role { get; set; } = default!;

        /// <summary>
        /// Gets or sets the Id of the user assigned to the role.
        /// </summary>
        [ForeignKey(nameof(User))]
        [AuditDisplay(typeof(User), nameof(User.DisplayName))]
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets navigation property for the associated user.
        /// </summary>
        public User User { get; set; } = default!;
    }
}