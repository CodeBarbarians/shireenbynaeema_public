namespace Domain
{
    /// <summary>
    /// Model representing detailed user information along with notification settings.
    /// Used when retrieving a user's profile and preferences.
    /// </summary>
    public class UserDetails_Retrieve
    {
        /// <summary>Gets or sets user display name.</summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>Gets or sets user email address.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Gets or sets user primary role.</summary>
        public List<string> Role { get; set; } = [];

        /// <summary>Gets or sets associated organization name.</summary>
        public string Organization { get; set; } = string.Empty;

        /// <summary>Gets or sets user phone number.</summary>
        public string PhoneNo { get; set; } = string.Empty;

        /// <summary>Gets or sets invitation date in Unix format.</summary>
        public long? InvitationDateUnix { get; set; }

        /// <summary>Gets or sets name of the user who sent the invitation.</summary>
        public string InvitedBy { get; set; } = string.Empty;

        /// <summary>Gets or sets user notification settings.</summary>
        public IEnumerable<NotificationSetting_AddEdit> Settings { get; set; } = [];
    }
}