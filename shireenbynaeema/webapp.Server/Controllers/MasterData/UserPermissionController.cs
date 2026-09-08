namespace Server
{
    using Application;
    using Domain;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using SharedServices;

    /// <summary>
    /// Provides API endpoints for managing user permissions, including listing, retrieving, and updating permissions
    /// for users.
    /// </summary>
    /// <remarks>All actions require the caller to have the appropriate administrative permission. This
    /// controller is secured with authorization and is intended for use by authorized administrators managing user
    /// access rights.</remarks>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserPermissionController : ControllerBase
    {
        private readonly IUserPermissionService userPermissionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserPermissionController"/> class with the specified user permission service.
        /// </summary>
        /// <param name="userPermissionService">The service used to manage and query user permissions. Cannot be null.</param>
        public UserPermissionController(IUserPermissionService userPermissionService)
        {
            this.userPermissionService = userPermissionService;
        }

        /// <summary>
        /// Handles HTTP POST request to retrieve a list of user permissions
        /// based on the provided criteria in the UserPermissionListRequest object.
        /// </summary>
        /// <param name="request">
        /// The request object containing filtering, sorting, or pagination criteria for user permissions.
        /// </param>
        /// <returns>
        /// Returns 200 OK with the list of user permissions matching the specified criteria.
        /// </returns>
        [HttpPost("List")]
        [Permission(Permissions.Admin.Users.ManagePermissions)]
        public async Task<IActionResult> List([FromBody] UserPermissionListRequest request)
            => this.Ok(await this.userPermissionService.List(request));

        /// <summary>
        /// Handles HTTP GET request to retrieve the permissions of a specific user
        /// identified by the userId parameter.
        /// </summary>
        /// <param name="userId">
        /// The unique identifier (GUID) of the user whose permissions are being retrieved.
        /// </param>
        /// <returns>
        /// Returns 200 OK with the list of permissions assigned to the specified user.
        /// </returns>
        [HttpGet("/{userId}")]
        [Permission(Permissions.Admin.Users.ManagePermissions)]
        public async Task<IActionResult> Retrieve(Guid userId)
            => this.Ok(await this.userPermissionService.Retrieve(userId));

        /// <summary>
        /// Handles HTTP PUT request to update the permissions of a user
        /// based on the provided request data.
        /// </summary>
        /// <param name="request">
        /// The request object containing the updated permissions for the user.
        /// </param>
        /// <returns>
        /// Returns 200 OK with the result of the permission update operation.
        /// </returns>
        [HttpPut]
        [Permission(Permissions.Admin.Users.ManagePermissions)]
        public async Task<IActionResult> Update([FromBody] SaveRequest<UserPermission_AddEdit> request)
            => this.Ok(await this.userPermissionService.Update(request));
    }
}