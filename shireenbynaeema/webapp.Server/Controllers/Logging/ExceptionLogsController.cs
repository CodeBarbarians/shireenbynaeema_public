namespace Server
{
    using Application;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using SharedServices;

    /// <summary>
    /// Provides API endpoints for retrieving and managing exception logs within the application.
    /// </summary>
    /// <remarks>All actions in this controller require authentication. Use this controller to query exception
    /// logs based on various filter criteria, such as date range, severity, or pagination options. This controller is
    /// intended for administrative or support scenarios where access to application error information is
    /// required.</remarks>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ExceptionLogController : ControllerBase
    {
        private readonly IExceptionLogService exceptionLogService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionLogController"/> class with the specified exception log service.
        /// </summary>
        /// <param name="exceptionLogService">The service used to manage and retrieve exception log entries. Cannot be null.</param>
        public ExceptionLogController(IExceptionLogService exceptionLogService)
        {
            this.exceptionLogService = exceptionLogService;
        }

        /// <summary>
        /// Handles HTTP GET request to retrieve a list of exception logs
        /// based on the provided filter criteria in the ListRequest object.
        /// </summary>
        /// <param name="request">
        /// The request object containing filtering, sorting, or pagination criteria
        /// for retrieving exception logs.
        /// </param>
        /// <returns>
        /// Returns 200 OK with the list of exception logs matching the specified criteria.
        /// </returns>
        [HttpGet]
        [Route("List")]
        public async Task<ActionResult> List(ListRequest request)
        {
            var response = await this.exceptionLogService.GetAllAsync(request);
            return this.Ok(response);
        }
    }
}