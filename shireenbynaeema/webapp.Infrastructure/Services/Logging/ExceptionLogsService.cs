namespace Infrastructure
{
    using Application;
    using Domain;
    using SharedServices;

    /// <summary>
    /// Provides operations for managing and retrieving exception log entries.
    /// </summary>
    /// <remarks>This service offers functionality for working with exception logs, such as querying, adding,
    /// or updating log entries. It is typically used to support error tracking and diagnostics within an
    /// application.</remarks>
    public class ExceptionLogService : Service<ExceptionLog>, IExceptionLogService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionLogService"/> class with the specified repository and response.
        /// handler.
        /// </summary>
        /// <param name="repository">The repository used to store and retrieve ExceptionLog entities. Cannot be null.</param>
        /// <param name="response">The response handler used to format or process service responses. Cannot be null.</param>
        public ExceptionLogService(IRepository<ExceptionLog> repository, IResponse response)
            : base(repository, response)
        {
        }
    }
}