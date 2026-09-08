namespace Infrastructure
{
    using System.Linq.Expressions;

    using Application;
    using Hangfire;
    using SharedServices;

    /// <summary>
    /// Provides methods for enqueuing background jobs and creating continuation jobs using Hangfire, with
    /// response-based status reporting.
    /// </summary>
    /// <remarks>This class acts as a wrapper around Hangfire's job client, enabling job scheduling and
    /// chaining while returning standardized response objects. All job operations return an <see cref="IResponse"/>
    /// indicating success or failure, along with the Hangfire job ID or error message. Exceptions during job scheduling
    /// are handled internally and reflected in the response. This class is thread-safe for concurrent job
    /// scheduling.</remarks>
    public class CustomBackgroundJobClient : ICustomBackgroundJobClient
    {
        private readonly IBackgroundJobClient hangfireClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomBackgroundJobClient"/> class using the specified Hangfire background.
        /// job client.
        /// </summary>
        /// <param name="hangfireClient">The Hangfire background job client used to manage and enqueue background jobs. Cannot be null.</param>
        public CustomBackgroundJobClient(IBackgroundJobClient hangfireClient)
        {
            this.hangfireClient = hangfireClient;
        }

        /// <summary>
        /// Enqueues a background job using Hangfire and returns the job ID in the response message.
        /// </summary>
        /// <typeparam name="T">The type of the class containing the method to enqueue.</typeparam>
        /// <param name="methodCall">
        /// An expression representing the asynchronous method to execute as a background job.
        /// Example: <c>x => x.SomeMethodAsync(arg1, arg2)</c>.
        /// </param>
        /// <returns>
        /// An <see cref="IResponse"/> containing:
        /// <list type="bullet">
        ///   <item><c>IsSuccess</c>: True if the job was enqueued successfully; false if an exception occurred.</item>
        ///   <item><c>Message</c>: The Hangfire job ID if successful, otherwise the exception message.</item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method wraps Hangfire's <c>Enqueue</c> functionality. Any exceptions during job enqueueing
        /// will be caught and returned in the response.
        /// </remarks>
        public IResponse Enqueue<T>(Expression<Func<T, Task>> methodCall)
            where T : class
        {
            var response = new Response(); // your IResponse implementation

            try
            {
                var jobId = this.hangfireClient.Enqueue(methodCall);

                response.IsSuccess = true;
                response.Message = jobId;      // <<< Store jobId here
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Creates a continuation job in Hangfire that executes after a specified parent job completes.
        /// </summary>
        /// <typeparam name="T">The type of the class containing the method to enqueue as the continuation job.</typeparam>
        /// <param name="parentJob">
        /// The response object from the parent job.
        /// The parent job must have <see cref="IResponse.IsSuccess"/> set to true and a valid Hangfire job ID in <see cref="IResponse.Message"/>.
        /// </param>
        /// <param name="methodCall">
        /// An expression representing the asynchronous method to execute as the continuation job.
        /// Example: <c>x => x.SomeMethodAsync(arg1, arg2)</c>.
        /// </param>
        /// <returns>
        /// An <see cref="IResponse"/> containing:
        /// <list type="bullet">
        ///   <item><c>IsSuccess</c>: True if the continuation job was successfully created; false if the parent job was invalid or an exception occurred.</item>
        ///   <item><c>Message</c>: The Hangfire job ID of the new continuation job if successful, otherwise an error message.</item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method validates the parent job response before creating a continuation job.
        /// If the parent job failed or does not contain a valid job ID, the continuation will not be created.
        /// </remarks>
        public IResponse ContinueJobWith<T>(IResponse parentJob, Expression<Func<T, Task>> methodCall)
            where T : class
        {
            var response = new Response();

            try
            {
                if (!parentJob.IsSuccess || parentJob.Message == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Invalid or failed parent job.";
                    return response;
                }

                var parentJobId = parentJob.Message;
                var jobId = this.hangfireClient.ContinueJobWith(parentJobId.ToString(), methodCall);

                response.IsSuccess = true;
                response.Message = jobId;       // <<< new jobId
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }
    }
}