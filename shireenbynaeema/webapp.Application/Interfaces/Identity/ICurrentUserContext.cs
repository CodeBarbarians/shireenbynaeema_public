namespace Infrastructure
{
    /// <summary>
    /// Interface for current user context, providing properties and methods to access information about the currently authenticated user. This interface is designed to
    /// encapsulate all relevant user information and provide a consistent way to access it throughout the application. The properties include user ID, organization ID, email, role, username, IP address, display name, and a list of roles. The methods allow for checking if the user has a specific role,
    /// as well as determining the user's overall access level within the application.
    /// </summary>
    public interface ICurrentUserContext
    {
        /// <summary>
        /// Gets the unique identifier of the currently authenticated user. This property is typically used to identify the user in various operations,
        /// such as retrieving user-specific data or performing actions on behalf of the user. The user ID is usually a GUID (Globally Unique Identifier)
        /// that uniquely identifies the user across the application.
        /// </summary>
        Guid? UserId { get; }

        /// <summary>
        /// Gets the unique identifier of the organization that the currently authenticated user belongs to. This property is used to determine the organizational context of the user,
        /// such as retrieving organization-specific data or performing actions on behalf of the organization. The organization ID is usually a GUID (Globally Unique Identifier)
        /// that uniquely identifies the organization across the application.
        /// </summary>
        Guid? OrganizationId { get; }

        /// <summary>
        /// Gets the email address of the currently authenticated user. This property is used for communication purposes, such as sending notifications or
        /// displaying the user's email in the user interface. The email address is typically a string that follows the standard email format.
        /// </summary>
        string? Email { get; }

        /// <summary>
        /// Gets the primary role of the currently authenticated user. This property is used to determine the user's main access level and permissions within the application.
        /// The role is typically a string that represents the user's primary role, such as "Admin".
        /// </summary>
        string? Role { get; }

        /// <summary>
        /// Gets the username of the currently authenticated user. This property is used for identification and display purposes, such as showing the user's name
        /// in the user interface or logging user actions. The username is typically a string that uniquely identifies the user within the application, and
        /// it may be used in conjunction with the email address for authentication and communication purposes.
        /// </summary>
        string? Username { get; }

        /// <summary>
        /// Gets the IP address of the currently authenticated user. This property is used for security and auditing purposes, such as tracking user activity or
        /// identifying potential security threats. The IP address is typically a string that represents the user's current IP address, and it may be used in conjunction
        /// with other user information for security analysis.
        /// </summary>
        string? IPAddress { get; }

        /// <summary>
        /// Gets the display name of the currently authenticated user. This property is used for user-friendly representations of the user's name in the user interface,
        /// such as showing the user's name in a profile or account settings page.
        /// </summary>
        string? DisplayName { get; }

        /// <summary>
        /// Gets a list of roles that the currently authenticated user belongs to. This property is used to determine the user's access levels and permissions within the application,
        /// such as allowing or denying access to specific features or resources.
        /// </summary>
        List<string> Roles { get; }

        /// <summary>
        /// Gets a value indicating whether checks if the currently authenticated user is a super administrator. This method returns a boolean value indicating whether the user has the highest level
        /// of access and permissions within the application, typically allowing them to manage all aspects of the system without restrictions.
        /// </summary>
        bool IsSuperAdmin { get; }

        /// <summary>
        /// Gets a value indicating whether checks if the currently authenticated user is an organization administrator. This method returns a boolean value indicating whether the user has
        /// administrative privileges.
        /// </summary>
        bool IsOrganizationUser { get; }

        /// <summary>
        /// Gets a value indicating whether checks if the currently authenticated user is a facility user. This method returns a boolean value indicating whether the user has access to specific
        /// facilities or resources within the application, typically allowing them to perform actions related to those facilities or resources.
        /// </summary>
        bool IsFacilityUser { get; }

        /// <summary>
        /// Gets the time zone information of the currently authenticated user. This property is used to determine the user's local time zone, which can be important for displaying
        /// dates and times in a way that is relevant to the user.
        /// </summary>
        TimeZoneInfo TimeZone { get; }

        /// <summary>
        /// Checks if the currently authenticated user has a specific role. This method takes a role as input and returns a boolean value indicating whether the user belongs to that role or not.
        /// </summary>
        /// <param name="role">RoleName.</param>
        /// <returns>true or false.</returns>
        bool HasRole(string role);
    }
}