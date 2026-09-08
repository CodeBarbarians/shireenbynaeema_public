namespace Domain
{
    /// <summary>
    /// Listing model representing summarized user permission information.
    /// Used for displaying users along with their assigned permissions.
    /// </summary>
    public class UserPermission_Listing
    {
        /// <summary>Gets or sets identifier of the user.</summary>
        public Guid UserId { get; set; }

        /// <summary>Gets or sets name of the user.</summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>Gets or sets email address of the user.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Gets or sets concatenated permission names assigned to the user.</summary>
        public string Permissions { get; set; } = string.Empty;

        /// <summary>Gets or sets total number of permissions assigned to the user.</summary>
        public int PermissionCount { get; set; }
    }
}