namespace Server
{
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// Custom authorization requirement that checks if the user has the required permissions to access a resource. This class implements the IAuthorizationRequirement interface,
    /// allowing it to be used in conjunction with an authorization handler that will evaluate whether the user meets the specified permission requirements. The Permissions property
    /// contains the list of permissions that the user must have.
    /// </summary>
    public class PermissionRequirement(string[] permissions) : IAuthorizationRequirement
    {
        /// <summary>
        /// Gets the set of permissions associated with the current user or context.
        /// </summary>
        public string[] Permissions { get; } = permissions;
    }
}