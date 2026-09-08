namespace Application
{
    using Domain;
    using SharedServices;

    /// <summary>
    /// Service responsible for managing users.
    /// Handles creation, retrieval, updating, and listing of user accounts.
    /// </summary>
    public interface IUserService : IService<User>
    {
        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="request">User creation request containing user details.</param>
        /// <returns>Response indicating the result of the operation.</returns>
        Task<IResponse> AddUser(SaveRequest<User_AddEdit> request);

        /// <summary>
        /// Retrieves a filtered and paginated list of users.
        /// </summary>
        /// <param name="request">Filtering and paging parameters.</param>
        /// <returns>Response containing the list of users.</returns>
        Task<IResponse> ListUsers(UserListRequest request);

        /// <summary>
        /// Retrieves a user by its unique identifier.
        /// </summary>
        /// <param name="id">Unique identifier of the user.</param>
        /// <returns>Response containing the user details.</returns>
        Task<IResponse> RetrieveUser(Guid id);

        /// <summary>
        /// Updates an existing user.
        /// </summary>
        /// <param name="request">User update request containing modified details.</param>
        /// <returns>Response indicating the result of the update operation.</returns>
        Task<IResponse> UpdateUser(SaveRequest<User_AddEdit> request);
    }
}