namespace Domain
{
    using SharedServices;

    /// <summary>
    /// Listing model representing summarized user information.
    /// Used for displaying users in list or grid views.
    /// </summary>
    public class User_Listing : Base_Listing
    {
        /// <summary>Gets or sets unique identifier of the user.</summary>
        public Guid Id { get; set; }

        /// <summary>Gets or sets user first name.</summary>
        public string? FirstName { get; set; }

        /// <summary>Gets or sets user last name.</summary>
        public string? LastName { get; set; }

        /// <summary>Gets or sets user email address.</summary>
        [QuickSearch]
        public string? Email { get; set; }

        /// <summary>Gets or sets user display name.</summary>
        [QuickSearch]
        public string? DisplayName { get; set; }

        /// <summary>Gets or sets status of the user.</summary>
        public string? Status { get; set; }

        /// <summary>Gets or sets user phone number.</summary>
        [QuickSearch]
        public string? PhoneNo { get; set; }

        /// <summary>Gets or sets name of the inviter.</summary>
        public string? InvitedBy { get; set; }

        /// <summary>Gets or sets associated organization name.</summary>
        public string? OrganizationName { get; set; }

        /// <summary>Gets or sets invitation date in Unix format.</summary>
        public long? InvitationDateUnix { get; set; }

        /// <summary>Gets or sets formatted invitation date.</summary>
        public string? InvitationDate { get; set; }

        /// <summary>Gets or sets formatted invitation time.</summary>
        public string? InvitationTime { get; set; }

        /// <summary>Gets or sets approval date in Unix format.</summary>
        public long? ApprovalDateUnix { get; set; }

        /// <summary>Gets or sets formatted approval date.</summary>
        public string? ApprovalDate { get; set; }

        /// <summary>Gets or sets formatted approval time.</summary>
        public string? ApprovalTime { get; set; }

        /// <summary>Gets or sets user roles for filtering.</summary>
        [QuickFilter]
        public string? UserRoles { get; set; }
    }

    /// <summary>
    /// Lookup model representing basic user information.
    /// Used for selection lists and reference mappings.
    /// </summary>
    public class User_Lookup
    {
        /// <summary>Gets or sets unique identifier of the user.</summary>
        public Guid Id { get; set; }

        /// <summary>Gets or sets user display name.</summary>
        public string DisplayName { get; set; } = string.Empty;
    }
}