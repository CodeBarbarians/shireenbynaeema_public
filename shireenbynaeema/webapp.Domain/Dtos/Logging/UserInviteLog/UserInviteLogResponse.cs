namespace Domain
{
    using SharedServices;

    /// <summary>
    /// Listing model representing summarized user invite log information.
    /// Used for displaying invite activity in list or grid views.
    /// </summary>
    public class UserInviteLog_Listing : Base_Listing
    {
        /// <summary>Gets or sets unique identifier of the invite log.</summary>
        public Guid Id { get; set; }

        /// <summary>Gets or sets name of the organization the user was invited to.</summary>
        public string? OrganizationName { get; set; }

        /// <summary>Gets or sets name of the assigned role.</summary>
        public string? RoleName { get; set; }

        /// <summary>Gets or sets date and time when the invite was sent.</summary>
        public DateTime? SentOn { get; set; }

        /// <summary>Gets or sets indicates whether the invite was successfully delivered.</summary>
        public string? IsSuccess { get; set; }
    }
}