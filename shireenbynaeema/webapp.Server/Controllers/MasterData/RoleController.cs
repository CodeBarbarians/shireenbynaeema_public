namespace Server
{
    using Application;
    using Domain;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using SharedServices;

    /// <summary>
    /// Provides API endpoints for managing roles and permissions, including operations to list, add, update, delete,
    /// and retrieve roles, as well as to manage role activation and retrieve available permissions.
    /// </summary>
    /// <remarks>All endpoints require authentication and appropriate permissions. This controller is intended
    /// for administrative use to manage role-based access control within the system.</remarks>
    /// <param name="roleService">The service used to perform role and permission management operations.</param>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoleController(IRoleService roleService) : ControllerBase
    {
        /// <summary>
        /// Handles HTTP POST request to retrieve a list of roles based on the provided request parameters.
        /// </summary>
        /// <param name="request">
        /// The request object containing filtering, sorting, and pagination criteria for the role list.
        /// </param>
        /// <returns>
        /// Returns 200 OK with the list of roles matching the specified criteria.
        /// </returns>
        [HttpPost]
        [Permission(Permissions.Admin.Roles.View)]
        [Route("List")]
        public async Task<ActionResult> List(RoleListRequest request)
        {
            var response = await roleService.List(request);
            return this.Ok(response);
        }

        /// <summary>
        /// Handles HTTP POST request to add a new role based on the provided request data.
        /// </summary>
        /// <param name="request">
        /// The request object containing the details of the role to be added.
        /// </param>
        /// <returns>
        /// Returns 200 OK with the result of the add operation, indicating success or failure.
        /// </returns>
        [HttpPost]
        [Permission(Permissions.Admin.Roles.Add)]
        [Route("Add")]
        public async Task<ActionResult> Add(SaveRequest<Role_AddEdit> request)
        {
            var response = await roleService.Add(request);
            return this.Ok(response);
        }

        /// <summary>
        /// Handles HTTP POST request to update an existing role
        /// based on the provided request data.
        /// </summary>
        /// <param name="request">
        /// The request object containing the updated details of the role.
        /// </param>
        /// <returns>
        /// Returns 200 OK with the result of the update operation, indicating success or failure.
        /// </returns>
        [HttpPost]
        [Permission(Permissions.Admin.Roles.Update)]
        [Route("Update")]
        public async Task<ActionResult> Update(SaveRequest<Role_AddEdit> request)
        {
            var response = await roleService.Update(request);
            return this.Ok(response);
        }

        /// <summary>
        /// Handles HTTP POST request to delete an existing role
        /// based on the provided unique identifier (id).
        /// </summary>
        /// <param name="id">
        /// The unique identifier (GUID) of the role to be deleted.
        /// </param>
        /// <returns>
        /// Returns 200 OK with the result of the delete operation, indicating success or failure.
        /// </returns>
        [HttpPost]
        [Permission(Permissions.Admin.Roles.Delete)]
        [Route("Delete/{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var response = await roleService.Delete(id);
            return this.Ok(response);
        }

        /// <summary>
        /// Handles HTTP GET request to retrieve the details of a specific role.
        /// </summary>
        /// <param name="id">
        /// The unique identifier (GUID) of the role to retrieve.
        /// </param>
        /// <returns>
        /// Returns 200 OK with the details of the requested role.
        /// </returns>
        [HttpGet]
        [Permission(Permissions.Admin.Roles.View)]
        [Route("Retrieve/{id}")]
        public async Task<ActionResult> Retrieve(Guid id)
        {
            var response = await roleService.Retrieve(id);
            return this.Ok(response);
        }

        /// <summary>
        /// Handles HTTP GET request to retrieve a list of all available permissions in the system.
        /// </summary>
        /// <returns>
        /// Returns 200 OK with the complete list of system permissions.
        /// </returns>
        [HttpGet("GetAllPermissions")]
        [Permission(Permissions.Admin.Roles.View)]
        public async Task<IActionResult> GetAllPermissions()
        {
            return this.Ok(await roleService.GetAllPermissions());
        }

        /// <summary>
        /// Handles HTTP POST request to bulk activate or deactivate multiple roles based on the provided request data.
        /// </summary>
        /// <param name="request">request.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpPost("BulkActiveInactive")]
        [Permission(Permissions.Admin.Roles.Update)]
        public async Task<IActionResult> BulkActiveInactive(SaveRequest<ActiveInActiveRequest> request)
        {
            ArgumentNullException.ThrowIfNull(request);
            return this.Ok(await roleService.BulkActiveInActive(request.Entity));
        }
    }
}