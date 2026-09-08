namespace Domain
{
    /// <summary>
    /// Model used for creating or updating user invite log records.
    /// Stores invitation details including user, organization, role, and delivery status.
    /// </summary>
    public class UserInviteLog_AddEdit
    {
        /// <summary>Gets or sets unique identifier of the invite log.</summary>
        public Guid Id { get; set; }

        /// <summary>Gets or sets identifier of the invited user.</summary>
        public Guid UserId { get; set; }

        /// <summary>Gets or sets identifier of the organization the user is invited to.</summary>
        public Guid OrganizationId { get; set; }

        /// <summary>Gets or sets identifier of the assigned role.</summary>
        public Guid RoleId { get; set; }

        /// <summary>Gets or sets unix timestamp indicating when the invite was sent.</summary>
        public long? SentOn { get; set; }

        /// <summary>Gets or sets indicates whether the invite was successfully delivered.</summary>
        public int? IsSuccess { get; set; }

        /// <summary>Gets or sets status of the invitation (e.g., Pending, Accepted, Expired).</summary>
        public int? Status { get; set; }
    }
}