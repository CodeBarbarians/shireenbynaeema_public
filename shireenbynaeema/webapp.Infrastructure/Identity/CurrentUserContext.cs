namespace Infrastructure
{
    using System.Security.Claims;

    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Implementation of ICurrentUserContext that retrieves user information from the current HTTP context. This class provides properties to access the user's ID,
    /// email, username, role, display name, IP address, organization ID, and roles. It also includes methods to check if the user has specific roles and to
    /// retrieve the user's timezone from the request headers. The SubClients property retrieves a list of sub-clients associated with the user's organization,
    /// with special handling for super administrators who can access all sub-clients.
    /// </summary>
    public class CurrentUserContext : ICurrentUserContext
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentUserContext"/> class using the provided HTTP context accessor and.
        /// database context.
        /// </summary>
        /// <remarks>The user's time zone is determined from the 'User-Timezone' HTTP request header. If
        /// the header is missing or invalid, the time zone defaults to Coordinated Universal Time (UTC).</remarks>
        /// <param name="httpContextAccessor">The accessor used to retrieve the current HTTP context, which provides access to request headers and user
        /// information.</param>
        /// <param name="dbContext">The database context used for accessing user-related data within the application.</param>
        public CurrentUserContext(IHttpContextAccessor httpContextAccessor, DatabaseContext dbContext)
        {
            this.httpContextAccessor = httpContextAccessor;

            var headerTz = this.httpContextAccessor.HttpContext?
                .Request?
                .Headers["User-Timezone"]
                .ToString();

            if (!string.IsNullOrWhiteSpace(headerTz))
            {
                try
                {
                    this.TimeZone = TimeZoneInfo.FindSystemTimeZoneById(headerTz);
                }
                catch
                {
                    this.TimeZone = TimeZoneInfo.Utc; // fallback on invalid timezone
                }
            }
            else
            {
                this.TimeZone = TimeZoneInfo.Utc; // fallback if missing
            }
        }

        /// <summary>
        /// Gets public property to retrieve the user's ID from the claims. This property attempts to find the NameIdentifier claim, which typically contains the
        /// user's unique identifier (e.g., a GUID). If the claim is found and can be parsed as a GUID, it returns the user ID; otherwise, it returns null.
        /// </summary>
        public Guid? UserId
        {
            get
            {
                var userIdClaim = this.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdClaim, out var userId))
                {
                    return userId;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets public property to retrieve the user's email address from the claims. This property looks for the Email claim, which typically contains the user's email address. If the claim is found,
        /// it returns the email address; otherwise, it returns null.
        /// </summary>
        public string? Email => this.User?.FindFirst(ClaimTypes.Email)?.Value;

        /// <summary>
        /// Gets public property to retrieve the user's username from the claims. This property looks for the Name claim, which typically contains the user's username or display name. If the claim is found,
        /// it returns the username; otherwise, it returns null.
        /// </summary>
        public string? Username => this.User?.FindFirst(ClaimTypes.Name)?.Value;

        /// <summary>
        /// Gets public property to retrieve the user's role from the claims. This property looks for the Role claim, which typically contains the user's primary role or a single role if only one is assigned. If the claim is found,
        /// it returns the role; otherwise, it returns null.
        /// </summary>
        public string? Role => this.User?.FindFirst(ClaimTypes.Role)?.Value;

        /// <summary>
        /// Gets public property to retrieve the user's display name from the claims. This property looks for a custom claim named "displayName", which may contain the user's full name or a more user-friendly name. If the claim is found,
        /// it returns the display name; otherwise, it returns null.
        /// </summary>
        public string? DisplayName => this.User?.FindFirst("displayName")?.Value;

        /// <summary>
        /// Gets public property to retrieve the user's IP address from the connection information. This property accesses the RemoteIpAddress from the
        /// ConnectionInfo, which represents the client's IP address. If the connection information is available and contains a remote IP address,
        /// it returns the IP address as a string; otherwise, it returns null.
        /// </summary>
        public string? IPAddress => this.Connection?.RemoteIpAddress?.ToString();

        /// <summary>
        /// Gets public property to retrieve the user's organization ID from the claims. This property looks for a custom claim named "organizationId", which is
        /// expected to contain the unique identifier of the organization the user belongs to (e.g., a GUID). If the claim is found and can be parsed as a GUID,
        /// it returns the organization ID; otherwise, it returns null.
        /// </summary>
        public Guid? OrganizationId
        {
            get
            {
                var orgIdClaim = this.User?.FindFirst("organizationId")?.Value;
                if (Guid.TryParse(orgIdClaim, out var orgId))
                {
                    return orgId;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets public property to retrieve a list of the user's roles from the claims. This property looks for all claims of type Role, which may contain
        /// multiple roles if the user is assigned to more than one role. It returns a list of role names; if no role claims are found, it returns an empty list.
        /// </summary>
        public List<string> Roles =>
            this.User?.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList()
            ?? [];

        /// <summary>
        /// Gets a value indicating whether the current user has super administrator privileges.
        /// </summary>
        /// <remarks>Use this property to determine if the user is granted elevated access typically
        /// reserved for super administrators. This property is read-only and reflects the user's claims at the time of
        /// evaluation.</remarks>
        public bool IsSuperAdmin => this.User?.FindFirst("isSuperAdmin")?.Value == "true" ? true : false;

        /// <summary>
        /// Gets a value indicating whether convenience property to check if the current user has the "FacilityUser" role. This property uses the HasRole method to determine if
        /// the user has the "FacilityUser" role.
        /// </summary>
        public bool IsFacilityUser => this.User?.FindFirst("scope")?.Value == "2" ? true : false;

        /// <summary>
        /// Gets a value indicating whether convenience property to check if the current user has the "OrgAdmin" role. This property uses the HasRole method to determine if
        /// the user has the "OrgAdmin" role.
        /// </summary>
        public bool IsOrganizationUser => this.User?.FindFirst("scope")?.Value == "1" ? true : false;

        /// <summary>
        /// Gets public property to retrieve the user's timezone. This property is set in the constructor by reading the "User-Timezone" header from the HTTP request.
        /// If the header is present and contains a valid timezone ID, it attempts to find the corresponding TimeZoneInfo object. If the header is missing or contains
        /// an invalid timezone ID, it defaults to UTC. This property allows other parts of the application to access the user's timezone for purposes such as displaying dates
        /// and times in the user's local timezone.
        /// </summary>
        public TimeZoneInfo TimeZone { get; private set; }

        /// <summary>
        /// Gets private property to access the current user's claims principal from the HTTP context. This property retrieves the user information from the current HTTP request,
        /// including claims such as user ID, email, and roles.
        /// </summary>
        private ClaimsPrincipal? User => this.httpContextAccessor.HttpContext?.User;

        /// <summary>
        /// Gets private property to access the connection information from the HTTP context. This property retrieves details about the client's connection, such as the remote IP address,
        /// the local IP address, and the connection ID.
        /// </summary>
        private ConnectionInfo? Connection => this.httpContextAccessor.HttpContext?.Connection;

        /// <summary>
        /// Determines whether the user is assigned the specified role.
        /// </summary>
        /// <param name="role">The name of the role to check for membership. Comparison is case-insensitive. Cannot be null.</param>
        /// <returns>true if the user has the specified role; otherwise, false.</returns>
        public bool HasRole(string role) =>
            this.Roles.Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
    }
}