namespace Application
{
    using Domain;
    using SharedServices;

    /// <summary>
    /// Service responsible for managing user permissions.
    /// Handles retrieval, assignment, removal, and evaluation of both direct and role-based permissions.
    /// </summary>
    public interface IUserPermissionService
    {
        /// <summary>
        /// Retrieves a filtered and paginated list of user permissions.
        /// </summary>
        /// <param name="request">Filtering and paging parameters.</param>
        /// <returns>Response containing the list of user permissions.</returns>
        Task<IResponse> List(UserPermissionListRequest request);

        /// <summary>
        /// Retrieves permissions assigned to a specific user.
        /// </summary>
        /// <param name="userId">Unique identifier of the user.</param>
        /// <returns>Response containing the user's permissions.</returns>
        Task<IResponse> Retrieve(Guid userId);

        /// <summary>
        /// Updates the permissions assigned to a user.
        /// </summary>
        /// <param name="request">Permission update request.</param>
        /// <returns>Response indicating the result of the update operation.</returns>
        Task<IResponse> Update(SaveRequest<UserPermission_AddEdit> request);

        /// <summary>
        /// Adds a specific permission to a user.
        /// </summary>
        /// <param name="userId">Unique identifier of the user.</param>
        /// <param name="permissionName">Permission to assign.</param>
        /// <param name="createdBy">Identifier of the user performing the assignment.</param>
        /// <returns>Response indicating the result of the operation.</returns>
        Task<IResponse> AddPermission(Guid userId, string permissionName, Guid createdBy);

        /// <summary>
        /// Removes a specific permission from a user.
        /// </summary>
        /// <param name="userId">Unique identifier of the user.</param>
        /// <param name="permissionName">Permission to remove.</param>
        /// <returns>Response indicating the result of the operation.</returns>
        Task<IResponse> RemovePermission(Guid userId, string permissionName);

        /// <summary>
        /// Retrieves all permissions for a user including both direct and role-based permissions.
        /// </summary>
        /// <param name="userId">Unique identifier of the user.</param>
        /// <returns>Response containing the combined permission set.</returns>
        Task<IResponse> GetAllPermissions(Guid userId);

        /// <summary>
        /// Determines whether a user has any of the specified permissions.
        /// </summary>
        /// <param name="userId">Unique identifier of the user.</param>
        /// <param name="permissions">Permissions to check.</param>
        /// <returns>True if the user has at least one of the permissions; otherwise false.</returns>
        Task<bool> HasPermissionAsync(Guid userId, string[] permissions);

        /// <summary>
        /// Retrieves all permissions assigned directly and indirectly to a user.
        /// </summary>
        /// <param name="userId">Unique identifier of the user.</param>
        /// <returns>Set of permission identifiers.</returns>
        Task<HashSet<string>> GetUserPermissionsAsync(Guid userId);

        /// <summary>
        /// Retrieves permissions associated with a specific role.
        /// </summary>
        /// <param name="roleId">Unique identifier of the role.</param>
        /// <returns>List of role permission identifiers.</returns>
        Task<List<string>> GetRolePermissionsAsync(Guid roleId);
    }
}