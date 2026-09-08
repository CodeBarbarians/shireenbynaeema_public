namespace Application
{
    using Domain;
    using SharedServices;

    /// <summary>
    /// Service responsible for managing roles and their associated permissions.
    /// Provides operations for creating, updating, retrieving, listing, and deleting roles.
    /// </summary>
    public interface IRoleService
    {
        /// <summary>
        /// Creates a new role.
        /// </summary>
        /// <param name="request">Role creation request containing role details.</param>
        /// <returns>Response indicating the result of the operation.</returns>
        Task<IResponse> Add(SaveRequest<Role_AddEdit> request);

        /// <summary>
        /// Retrieves a filtered and paginated list of roles.
        /// </summary>
        /// <param name="request">Filtering and paging parameters.</param>
        /// <returns>Response containing the list of roles.</returns>
        Task<IResponse> List(RoleListRequest request);

        /// <summary>
        /// Updates an existing role.
        /// </summary>
        /// <param name="request">Role update request containing modified details.</param>
        /// <returns>Response indicating the result of the update operation.</returns>
        Task<IResponse> Update(SaveRequest<Role_AddEdit> request);

        /// <summary>
        /// Deletes a role by its unique identifier.
        /// </summary>
        /// <param name="id">Unique identifier of the role.</param>
        /// <returns>Response indicating the result of the operation.</returns>
        Task<IResponse> Delete(Guid id);

        /// <summary>
        /// Retrieves a role by its unique identifier.
        /// </summary>
        /// <param name="id">Unique identifier of the role.</param>
        /// <returns>Response containing the role details.</returns>
        Task<IResponse> Retrieve(Guid id);

        /// <summary>
        /// Performs a bulk update to set multiple entities as active or inactive based on the specified request.
        /// </summary>
        /// <param name="request">An object containing the details of the entities to update and their desired active or inactive status.
        /// Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a response indicating the
        /// outcome of the bulk update.</returns>
        Task<IResponse> BulkActiveInActive(ActiveInActiveRequest request);

        /// <summary>
        /// Retrieves all available permissions from the system asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IResponse"/>
        /// object with the collection of permissions. The response may be empty if no permissions are defined.</returns>
        Task<IResponse> GetAllPermissions();
    }
}