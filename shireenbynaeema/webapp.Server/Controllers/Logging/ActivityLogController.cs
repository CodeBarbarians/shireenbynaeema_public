namespace Server
{
    using Application;
    using Domain;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using SharedServices;

    /// <summary>
    /// API controller for activity log operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Add your auth policy
    public class ActivityLogController : ControllerBase
    {
        private readonly IActivityLogService activityLogService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityLogController"/> class with the specified activity log service.
        /// </summary>
        /// <param name="activityLogService">The service used to manage and retrieve activity log data. Cannot be null.</param>
        public ActivityLogController(IActivityLogService activityLogService)
        {
            this.activityLogService = activityLogService;
        }

        /// <summary>
        /// Get activity logs with filtering and pagination.
        /// </summary>
        /// <param name="request">Filter parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Paginated activity logs.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(IResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(IResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetActivityLogs(
            ActivityLogFilterRequest request,
            CancellationToken cancellationToken)
        {
            var response = await this.activityLogService.GetActivityLogsAsync(request, cancellationToken);

            if (response.IsSuccess)
            {
                return this.Ok(response);
            }

            return this.BadRequest(response);
        }

        /// <summary>
        /// Retrieves the activity log entry with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the activity log entry to retrieve.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>An <see cref="IActionResult"/> containing the activity log entry if found; otherwise, a bad request result.</returns>
        [HttpGet("GetActivityLogById/{id}")]
        public async Task<IActionResult> GetActivityLogById(Guid id, CancellationToken cancellationToken)
        {
            var response = await this.activityLogService.GetActivityLogByIdAsync(id, cancellationToken);
            if (response.IsSuccess)
            {
                return this.Ok(response);
            }

            return this.BadRequest(response);
        }

        /// <summary>
        /// Get available filter options for dropdowns.
        /// </summary>
        /// <returns>Filter options.</returns>
        [HttpGet("filter-options")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFilterOptions()
        {
            return this.Ok(await this.activityLogService.GetFilterOptions());
        }
    }
}