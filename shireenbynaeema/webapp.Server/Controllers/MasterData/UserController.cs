namespace Server
{
    using Application;
    using Domain;
    using Infrastructure;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using SharedServices;

    /// <summary>
    /// Provides API endpoints for managing users, including retrieving user details, listing users, adding, updating,
    /// deleting users, performing bulk operations, and handling password reset and invitations. All endpoints require
    /// authentication and appropriate permissions.
    /// </summary>
    /// <remarks>This controller is secured with authorization and is intended for use by authenticated
    /// clients. Many actions require specific permissions in addition to authentication. Endpoints follow RESTful
    /// conventions and return standard HTTP status codes. All actions are accessible via routes prefixed with
    /// 'api/User'.</remarks>
    /// <param name="userService">The service used to perform core user management operations such as listing, adding, updating, and deleting
    /// users.</param>
    /// <param name="userExtensionService">The service used to perform extended user operations, including bulk status updates and user invitations.</param>
    /// <param name="authenticationService">The service used to handle authentication-related operations, such as sending password reset emails.</param>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController(IUserService userService, IUserExtensionService userExtensionService, IAuthenticationService authenticationService) : ControllerBase
    {
        /// <summary>
        /// Handles HTTP GET request to retrieve the details of the currently authenticated user.
        /// </summary>
        /// <returns>
        /// Returns 200 OK with information about the authenticated user, including UserId, Email, Username, DisplayName, and assigned Roles.
        /// </returns>
        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var currentUser = new CurrentUserDto
            {
                UserId = CurrentUser.UserId,
                Email = CurrentUser.Email,
                Username = CurrentUser.Username,
                DisplayName = CurrentUser.DisplayName,
                Roles = CurrentUser.Roles,
            };

            return this.Ok(currentUser);
        }

        /// <summary>
        /// Handles HTTP POST request to retrieve a list of users
        /// based on the provided filtering, sorting, and pagination criteria.
        /// </summary>
        /// <param name="request">
        /// The request object containing filtering, sorting, and pagination parameters for users.
        /// </param>
        /// <returns>
        /// Returns 200 OK with the list of users matching the specified criteria.
        /// </returns>
        [HttpPost]
        [Route("List")]
        [Permission(Permissions.Admin.Users.View)]
        public async Task<ActionResult> List(UserListRequest request)
        {
            var response = await userService.ListUsers(request);
            return this.Ok(response);
        }

        /// <summary>
        /// Handles HTTP POST request to add a new user
        /// based on the provided request data.
        /// </summary>
        /// <param name="request">
        /// The request object containing the details of the user to be added.
        /// </param>
        /// <returns>
        /// Returns 200 OK with the result of the add operation, indicating success or failure.
        /// </returns>
        [HttpPost]
        [Route("Add")]
        [Permission(Permissions.Admin.Users.Add)]
        public async Task<ActionResult> Add(SaveRequest<User_AddEdit> request)
        {
            var response = await userService.AddUser(request);
            return this.Ok(response);
        }

        /// <summary>
        /// Handles HTTP POST request to update an existing user's information.
        /// </summary>
        /// <param name="request">
        /// The request object containing the updated details of the user.
        /// </param>
        /// <returns>
        /// Returns 200 OK with the result of the update operation, indicating success or failure.
        /// </returns>
        [HttpPost]
        [Route("Update")]
        [Permission(Permissions.Admin.Users.Update)]
        public async Task<ActionResult> Update(SaveRequest<User_AddEdit> request)
        {
            var response = await userService.UpdateUser(request);
            return this.Ok(response);
        }

        /// <summary>
        /// Handles HTTP DELETE request to remove a user from the system
        /// based on their unique identifier (Id).
        /// </summary>
        /// <param name="id">
        /// The unique identifier (GUID) of the user to be deleted.
        /// </param>
        /// <returns>
        /// Returns 200 OK with the result of the delete operation, indicating success or failure.
        /// </returns>
        [HttpDelete]
        [Route("Delete/{Id}")]
        [Permission(Permissions.Admin.Users.Delete)]
        public async Task<ActionResult> Delete(Guid id)
        {
            var response = await userService.DeleteAsync(id);
            return this.Ok(response);
        }

        /// <summary>
        /// Handles HTTP GET request to retrieve the details of a specific user.
        /// </summary>
        /// <param name="id">
        /// The unique identifier (GUID) of the user to retrieve.
        /// </param>
        /// <returns>
        /// Returns 200 OK with the details of the requested user.
        /// </returns>
        [HttpGet]
        [Route("Retrieve/{Id}")]
        [Permission(Permissions.Admin.Users.View)]
        public async Task<ActionResult> Retrieve(Guid id)
        {
            var response = await userService.RetrieveUser(id);
            return this.Ok(response);
        }

        /// <summary>
        /// Handles HTTP POST request to perform a bulk update of user statuses (active/inactive).
        /// </summary>
        /// <param name="request">
        /// The request object containing a list of users and their desired active/inactive status updates.
        /// </param>
        /// <returns>
        /// Returns 200 OK with the result of the bulk status update operation.
        /// </returns>
        [HttpPost]
        [Route("BulkStatusUpdate")]
        [Permission(Permissions.Admin.Users.Update)]
        public async Task<ActionResult> BulkStatusUpdate(SaveRequest<ActiveInActiveUserRequest> request)
        {
            var resp = await userExtensionService.BulkActiveInActiveUser(request.Entity);
            return this.Ok(resp);
        }

        /// <summary>
        /// Handles HTTP POST request to send a password reset email
        /// to a user who has forgotten their password.
        /// </summary>
        /// <param name="request">
        /// The request object containing the email address of the user requesting a password reset.
        /// </param>
        /// <returns>
        /// Returns 200 OK with the result of the password reset email operation.
        /// </returns>
        [HttpPost]
        [Route("SendForgotPasswordEmail")]
        public async Task<ActionResult> SendForgetPasswordEmail(PasswordResetEmailRequest request)
        {
            var resp = await authenticationService.SendForgotPasswordEmailAsync(request.Email);
            return this.Ok(resp);
        }

        /// <summary>
        /// Handles HTTP POST request to perform a bulk invitation of users to the system.
        /// </summary>
        /// <param name="request">
        /// The request object containing user details for the bulk invitation,
        /// including an option to resend invitations if needed.
        /// </param>
        /// <returns>
        /// Returns 200 OK with the result of the bulk user invitation operation.
        /// </returns>
        [HttpPost]
        [Route("BulkUserInvite")]
        [Permission(Permissions.Admin.Users.Invite)]
        public async Task<ActionResult> BulkUserInvite(BulkUserInviteRequest request)
        {
            var resp = await userExtensionService.BulkInviteUsers(request);
            return this.Ok(resp);
        }
    }
}