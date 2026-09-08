namespace Application
{
    using Domain;
    using SharedServices;

    /// <summary>
    /// Service responsible for managing user invite logs.
    /// Provides functionality to retrieve individual invite records
    /// and list invite activity history.
    /// </summary>
    public interface IUserInviteLogService : IService<UserInviteLog>
    {
        /// <summary>
        /// Retrieves a specific user invite log by its unique identifier.
        /// </summary>
        /// <param name="id">Unique identifier of the invite log.</param>
        /// <returns>Response containing the invite log details.</returns>
        Task<IResponse> RetrieveUserInviteLog(Guid id);

        /// <summary>
        /// Retrieves a paginated list of user invite logs based on the provided request filters.
        /// </summary>
        /// <param name="request">Filtering, paging, and sorting parameters.</param>
        /// <returns>Response containing the list of invite logs.</returns>
        Task<IResponse> ListUserInviteLog(ListRequest request);
    }
}