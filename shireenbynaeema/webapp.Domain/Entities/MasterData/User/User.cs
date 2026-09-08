namespace Domain
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using SharedServices;

    /// <summary>
    /// Represents a system user with authentication, roles, permissions, and organizational linkage.
    /// </summary>
    [Table("User")]
    [EntityDisplayName("User")]
    public class User : Auditable, IIdentifiable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// Default constructor initializes string fields to avoid nulls and empty collections.
        /// </summary>
        public User()
        {
            this.FirstName = string.Empty;
            this.LastName = string.Empty;
            this.Email = string.Empty;
            this.DisplayName = string.Empty;
            this.PasswordHash = string.Empty;
            this.UserRoles = [];
            this.UserPermissions = [];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// Constructs a User entity from a User_AddEdit DTO.
        /// Handles defaults, password hashing, and timestamps.
        /// </summary>
        /// <param name="addedit">Data transfer object with user input.</param>
        public User(User_AddEdit addedit)
        {
            this.Id = addedit.Id ?? Guid.NewGuid();
            this.FirstName = addedit.FirstName ?? string.Empty;
            this.LastName = addedit.LastName ?? string.Empty;
            this.Email = addedit.Email ?? string.Empty;
            this.DisplayName = $@"{this.FirstName} {this.LastName}";
            this.Status = addedit.Status;
            this.PasswordHash = (addedit.Password ?? string.Empty).GeneratePasswordHash();
            this.PhoneNo = addedit.PhoneNo;
            this.InvitedBy = addedit.InvitedBy;
            this.InvitationDate = addedit.InvitationDate.ToTimeStamp();
            this.ApprovalDate = addedit.ApprovalDate.ToTimeStamp();
            this.Scope = addedit.Scope;
            this.UserRoles = [];
            this.UserPermissions = [];
        }

        /// <summary>
        /// Gets or sets primary identifier for the user.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>Gets or sets user's first name.</summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>Gets or sets user's last name.</summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>Gets or sets user's email, used for login.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Gets or sets hashed password for authentication.</summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>Gets or sets concatenated display name (FirstName + LastName).</summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>Gets or sets optional phone number.</summary>
        public string? PhoneNo { get; set; }

        /// <summary>Gets or sets id of the user who invited this user, if applicable.</summary>
        public Guid? InvitedBy { get; set; }

        /// <summary>Gets or sets invitation date as Unix timestamp.</summary>
        public long? InvitationDate { get; set; }

        /// <summary>Gets or sets approval date as Unix timestamp.</summary>
        public long? ApprovalDate { get; set; }

        /// <summary>Gets or sets user status (active/inactive).</summary>
        public int Status { get; set; }

        /// <summary>Gets or sets roles assigned to this user.</summary>
        [QuickFilter]
        public ICollection<UserRole> UserRoles { get; set; }

        /// <summary>Gets or sets permissions directly assigned to this user.</summary>
        public ICollection<UserPermission> UserPermissions { get; set; }

        /// <summary>Gets or sets a value indicating whether indicates if the user is a super administrator.</summary>
        public bool IsSuperAdmin { get; set; }

        /// <summary>
        /// Gets or sets indicates if the user is a Facility User or Organization User.
        /// </summary>
        [AuditDisplay(UseEnumDisplay = true)]
        public UserScope? Scope { get; set; }

        /// <summary>
        /// Returns the full display name of the user.
        /// </summary>
        /// <returns>string.</returns>
        public string GetDisplayName() => $@"{this.FirstName} {this.LastName}";

        /// <summary>
        /// Determines if the user is the ultimate admin account.
        /// </summary>
        /// <returns>bool.</returns>
        public bool IsUltimateAdmin() => this.Email.Equals(Constants.Seed.AdminEmail, StringComparison.OrdinalIgnoreCase) && this.IsSuperAdmin == true;
    }
}