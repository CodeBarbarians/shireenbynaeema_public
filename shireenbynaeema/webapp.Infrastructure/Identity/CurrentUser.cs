namespace Infrastructure
{
    using System.Security.Claims;

    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Static helper class to access current user information from the HttpContext. This class provides properties to retrieve user details such as UserId,
    /// Email, Username, OrganizationId, Role, DisplayName, IPAddress, and TimeZone. It also includes methods to check user roles and retrieve associated
    /// sub-clients based on the user's organization. The Configure method must be called during application startup to set the IHttpContextAccessor and
    /// DatabaseContext instances for accessing user information and related data. This class is designed to simplify access to current user information throughout
    /// the application without needing to inject IHttpContextAccessor or DatabaseContext in every service or controller that requires it.
    /// </summary>
    public static class CurrentUser
    {
        private static IHttpContextAccessor? httpContextAccessor;

        /// <summary>
        /// Gets retrieves the current user's unique identifier (UserId) from the claims in the HttpContext. The UserId is expected to be stored as a claim
        /// of type ClaimTypes.NameIdentifier.
        /// </summary>
        public static Guid? UserId
        {
            get
            {
                var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdClaim, out var userId))
                {
                    return userId;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets retrieves the current user's email address from the claims in the HttpContext. The email is expected to be stored as a claim of type ClaimTypes.Email.
        /// </summary>
        public static string? Email => User?.FindFirst(ClaimTypes.Email)?.Value;

        /// <summary>
        /// Gets retrieves the current user's username from the claims in the HttpContext. The username is expected to be stored as a claim of type ClaimTypes.Name.
        /// </summary>
        public static string? Username => User?.FindFirst(ClaimTypes.Name)?.Value;

        /// <summary>
        /// Gets retrieves the current user's organization identifier (OrganizationId) from the claims in the HttpContext. The OrganizationId is expected to be stored as a claim
        /// of type "organizationId".
        /// </summary>
        public static Guid? OrganizationId
        {
            get
            {
                var orgIdClaim = User?.FindFirst("organizationId")?.Value;
                if (Guid.TryParse(orgIdClaim, out var orgId))
                {
                    return orgId;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether an organization is currently associated with the context.
        /// </summary>
        public static bool HasOrganization => OrganizationId != null;

        /// <summary>
        /// Gets retrieves the current user's role from the claims in the HttpContext. The role is expected to be stored as a claim of type ClaimTypes.Role.
        /// Note that a user may have multiple roles, so this property returns only the first role found. For users with multiple roles, consider using
        /// the Roles property to retrieve all roles assigned to the user.
        /// </summary>
        public static string Role => User?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        /// <summary>
        /// Gets retrieves the current user's display name from the claims in the HttpContext. The display name is expected to be stored as a claim of type "displayName".
        /// </summary>
        public static string DisplayName => User?.FindFirst("displayName")?.Value ?? string.Empty;

        /// <summary>
        /// Gets retrieves the current user's IP address from the connection information in the HttpContext. The IP address is obtained from the RemoteIpAddress
        /// property of the ConnectionInfo.
        /// </summary>
        public static string IPAddress => Connection?.RemoteIpAddress?.ToString() ?? string.Empty;

        /// <summary>
        /// Gets retrieves a list of all roles assigned to the current user from the claims in the HttpContext. The roles are expected to be stored as claims of
        /// type ClaimTypes.Role.
        /// </summary>
        public static List<string> Roles =>
            User?.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList()
            ?? [];

        /// <summary>
        /// Gets a value indicating whether convenience property to check if the current user has the "SuperAdmin" role. This property uses the HasRole method to determine if
        /// the user has the "SuperAdmin" role.
        /// </summary>
        public static bool IsSuperAdmin => User?.FindFirst("isSuperAdmin")?.Value == "true" ? true : false;

        /// <summary>
        /// Gets a value indicating whether convenience property to check if the current user has the "FacilityUser" role. This property uses the HasRole method to determine if
        /// the user has the "FacilityUser" role.
        /// </summary>
        public static bool IsFacilityUser => User?.FindFirst("scope")?.Value == "2" ? true : false;

        /// <summary>
        /// Gets a value indicating whether convenience property to check if the current user has the "OrgAdmin" role. This property uses the HasRole method to determine if
        /// the user has the "OrgAdmin" role.
        /// </summary>
        public static bool IsOrganizationUser => User?.FindFirst("scope")?.Value == "1" ? true : false;

        /// <summary>
        /// Gets retrieves the current user's time zone information based on a custom header "User-Timezone" in the HttpContext. If the header is not present or
        /// if the time zone ID is invalid, the property defaults to returning TimeZoneInfo.Utc. This allows the application to handle user-specific time zone
        /// settings gracefully, even in cases where the header is missing or contains invalid data.
        /// </summary>
        public static TimeZoneInfo TimeZone
        {
            get
            {
                try
                {
                    var tz = Headers?["User-Timezone"].ToString();

                    if (string.IsNullOrWhiteSpace(tz))
                    {
                        return TimeZoneInfo.Utc;
                    }

                    return TimeZoneInfo.FindSystemTimeZoneById(tz);
                }
                catch
                {
                    return TimeZoneInfo.Utc;
                }
            }
        }

        /// <summary>
        /// Gets retrieves the current user's time zone ID from a custom header "User-Timezone" in the HttpContext. If the header is not present or if the value is null or whitespace,
        /// the property defaults to returning "UTC".
        /// </summary>
        public static string TimeZoneId
        {
            get
            {
                // Get header value or fallback to UTC
                var tz = Headers?["User-Timezone"].ToString();
                return !string.IsNullOrWhiteSpace(tz) ? tz : "UTC";
            }
        }

        /// <summary>
        /// Gets private helper properties to access the current user's claims, connection information, and request headers from the HttpContext. These properties are used internally
        /// to simplify access to common user-related data.
        /// </summary>
        private static ClaimsPrincipal? User => httpContextAccessor?.HttpContext?.User;

        /// <summary>
        /// Gets connection information from the current HttpContext, used to retrieve the user's IP address. This property is accessed when retrieving
        /// the IPAddress property of the current user.
        /// </summary>
        private static ConnectionInfo? Connection => httpContextAccessor?.HttpContext?.Connection;

        /// <summary>
        /// Gets request headers from the current HttpContext, used to retrieve custom headers such as "User-Timezone". This property is accessed when retrieving the
        /// TimeZone and TimeZoneId.
        /// </summary>
        private static IHeaderDictionary? Headers => httpContextAccessor?.HttpContext?.Request?.Headers;

        /// <summary>
        /// Determines whether the current user is assigned the specified role.
        /// </summary>
        /// <remarks>Use this method to verify role-based access or permissions for the current user. The
        /// check is performed in a case-insensitive manner.</remarks>
        /// <param name="role">The name of the role to check for membership. Comparison is case-insensitive.</param>
        /// <returns>true if the user has the specified role; otherwise, false.</returns>
        public static bool HasRole(string role) =>
            Roles.Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Configures the static context by setting the HTTP context accessor and database context instances used by
        /// the application.
        /// </summary>
        /// <remarks>This method should be called during application startup to ensure that the required
        /// dependencies are available for subsequent operations. Calling this method multiple times will overwrite the
        /// previously configured instances.</remarks>
        /// <param name="httpContextAccessor">The accessor used to retrieve the current HTTP context. Cannot be null.</param>
        /// <param name="dbContext">The database context instance to be used for data operations. Cannot be null.</param>
        public static void Configure(IHttpContextAccessor httpContextAccessor, DatabaseContext dbContext)
        {
            CurrentUser.httpContextAccessor = httpContextAccessor;
        }
    }
}