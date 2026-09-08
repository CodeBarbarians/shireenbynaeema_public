namespace Domain
{
    /// <summary>
    /// Data transfer object representing the authenticated user's basic identity and roles.
    /// </summary>
    public class CurrentUserDto
    {
        /// <summary>Gets or sets unique identifier of the user.</summary>
        public Guid? UserId { get; set; }

        /// <summary>Gets or sets user email address.</summary>
        public string? Email { get; set; }

        /// <summary>Gets or sets username used for authentication.</summary>
        public string? Username { get; set; }

        /// <summary>Gets or sets display name shown in the application.</summary>
        public string? DisplayName { get; set; }

        /// <summary>Gets or sets list of roles assigned to the user.</summary>
        public List<string> Roles { get; set; } = [];
    }
}