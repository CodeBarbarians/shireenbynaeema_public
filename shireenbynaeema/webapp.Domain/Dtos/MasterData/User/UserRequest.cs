namespace Domain
{
    using SharedServices;

    /// <summary>
    /// Model used for creating or updating user information.
    /// Contains personal details, organization association, roles, and status metadata.
    /// </summary>
    public class User_AddEdit
    {
        /// <summary>Gets or sets unique identifier of the user.</summary>
        public Guid? Id { get; set; }

        /// <summary>Gets or sets user first name.</summary>
        public string? FirstName { get; set; }

        /// <summary>Gets or sets user last name.</summary>
        public string? LastName { get; set; }

        /// <summary>Gets or sets user email address.</summary>
        public string? Email { get; set; }

        /// <summary>Gets or sets user password.</summary>
        public string? Password { get; set; }

        /// <summary>Gets or sets user phone number.</summary>
        public string? PhoneNo { get; set; }

        /// <summary>Gets or sets identifier of the user who sent the invitation.</summary>
        public Guid? InvitedBy { get; set; }

        /// <summary>Gets or sets associated organization identifier.</summary>
        public Guid? OrganizationId { get; set; }

        /// <summary>Gets or sets associated organization name.</summary>
        public string? OrganizationName { get; set; }

        /// <summary>Gets or sets status of the user.</summary>
        public int Status { get; set; }

        /// <summary>
        /// Gets or sets distinguish if the user is Facility User or Organization User.
        /// </summary>
        public UserScope? Scope { get; set; }

        /// <summary>Gets or sets date the invitation was sent.</summary>
        public DateTime? InvitationDate { get; set; }

        /// <summary>Gets or sets date the user was approved.</summary>
        public DateTime? ApprovalDate { get; set; }

        /// <summary>Gets or sets user who last updated the record.</summary>
        public string? UpdatedBy { get; set; }

        /// <summary>Gets or sets last update timestamp.</summary>
        public DateTime? UpdatedOn { get; set; }

        /// <summary>Gets or sets last login timestamp.</summary>
        public DateTime? LastLogin { get; set; }

        /// <summary>Gets or sets status name for display.</summary>
        public string? StatusName { get; set; }

        /// <summary>Gets or sets collection of role identifiers assigned to the user.</summary>
        public ICollection<Guid>? UserRoles { get; set; }

        /// <summary>Gets or sets collection of role names assigned to the user.</summary>
        public ICollection<string>? UserRoleNames { get; set; }

        /// <summary>Returns the display name of the user.</summary>
        /// <returns>Display name of the user.</returns>
        public string GetDisplayName()
        {
            return $@"{this.FirstName} {this.LastName}";
        }
    }

    /// <summary>
    /// Request model for retrieving users filtered by organization, role, inviter, and scope.
    /// </summary>
    public class UserListRequest : ListRequest
    {
        /// <summary>Gets or sets filters users by organization identifiers.</summary>
        public List<Guid>? Organization { get; set; }

        /// <summary>Gets or sets filters users by role identifiers.</summary>
        public List<Guid>? UserRole { get; set; }

        /// <summary>Gets or sets filters users by inviter identifiers.</summary>
        public List<Guid>? InvitedBy { get; set; }

        /// <summary>Gets or sets filters users by status.</summary>
        public int? Status { get; set; }

        /// <summary>Gets or sets defines the scope of users to retrieve.</summary>
        public UserScope? Scope { get; set; }
    }
}