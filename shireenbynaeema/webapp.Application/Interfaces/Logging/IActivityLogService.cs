namespace Application
{
    using Domain;
    using SharedServices;

    /// <summary>
    /// Service interface for activity log operations.
    /// </summary>
    public interface IActivityLogService
    {
        /// <summary>
        /// Asynchronously retrieves activity logs that match the specified filter criteria.
        /// </summary>
        /// <param name="request">The filter criteria used to select which activity logs to retrieve. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a response with the filtered
        /// activity logs.</returns>
        Task<IResponse> GetActivityLogsAsync(
            ActivityLogFilterRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the available filter options that can be applied to the current data set.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IResponse"/> with
        /// the filter options. The response may be empty if no filter options are available.</returns>
        Task<IResponse> GetFilterOptions();

        /// <summary>
        /// Asynchronously retrieves the activity log entry with the specified unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the activity log entry to retrieve.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the response with the requested
        /// activity log entry if found; otherwise, an appropriate response indicating the entry was not found.</returns>
        Task<IResponse> GetActivityLogByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}