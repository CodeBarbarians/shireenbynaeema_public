namespace Server
{
    using System.Security.Claims;

    using Application;
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// Handles authorization requirements by verifying whether a user possesses the necessary permissions as defined by
    /// a permission requirement.
    /// </summary>
    /// <remarks>This handler is typically used in conjunction with ASP.NET Core's authorization framework to
    /// enforce custom permission-based access control. It relies on an implementation of IUserPermissionService to
    /// determine if the current user meets the required permissions. The handler is intended for use in scenarios where
    /// fine-grained, permission-based authorization is needed beyond simple role checks.</remarks>
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IUserPermissionService permissionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionHandler"/> class.
        /// with the specified IUserPermissionService.
        /// </summary>
        /// <param name="permissionService">
        /// The service used to check and manage user permissions.
        /// </param>
        public PermissionHandler(IUserPermissionService permissionService)
        {
            this.permissionService = permissionService;
        }

        /// <summary>
        /// Handles the authorization requirement by checking if the user has the necessary permissions.
        /// </summary>
        /// <param name="context">
        /// The AuthorizationHandlerContext containing the user's identity and claims.
        /// </param>
        /// <param name="requirement">
        /// The PermissionRequirement specifying which permissions are required for access.
        /// </param>
        /// <returns>
        /// A Task representing the asynchronous operation of handling the authorization requirement.
        /// If the user has the required permissions, the requirement is marked as succeeded.
        /// </returns>
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                return;
            }

            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return;
            }

            if (await this.permissionService.HasPermissionAsync(userId, requirement.Permissions))
            {
                context.Succeed(requirement);
            }
        }
    }
}