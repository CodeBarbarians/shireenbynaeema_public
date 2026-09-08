namespace Application
{
    using System.Linq.Expressions;

    using SharedServices;

    /// <summary>
    /// Interface for a custom background job client, providing methods to enqueue background jobs and create continuation jobs. This interface is designed
    /// to abstract the underlying background job processing mechanism (such as Hangfire) and provide a consistent way to schedule and manage background tasks
    /// in the application. The methods include enqueuing a new background job based on a method call expression and creating a continuation job that runs after
    /// a specified parent job has completed. This allows for flexible scheduling of background tasks and the ability to chain multiple jobs together based on
    /// their dependencies.
    /// </summary>
    public interface ICustomBackgroundJobClient
    {
        /// <summary>
        /// Enqueues a new background job based on the provided method call expression. This method takes an expression representing a method call that returns a Task,
        /// and schedules it for execution in the background.
        /// </summary>
        /// <typeparam name="T">T.</typeparam>
        /// <param name="methodCall">methodCall.</param>
        /// <returns>IResponse.</returns>
        IResponse Enqueue<T>(Expression<Func<T, Task>> methodCall)
            where T : class;

        /// <summary>
        /// Creates a continuation job that runs after the specified parent job has completed. This method takes an IResponse representing the parent job and
        /// an expression representing`` a method call that returns a Task, and schedules the continuation job to run after the parent job finishes.
        /// This allows for chaining multiple background jobs together based on their dependencies.
        /// </summary>
        /// <typeparam name="T">T.</typeparam>
        /// <param name="parentJob">parentJob.</param>
        /// <param name="methodCall">methodCall.</param>
        /// <returns>IResponse.</returns>
        IResponse ContinueJobWith<T>(IResponse parentJob, Expression<Func<T, Task>> methodCall)
            where T : class;
    }
}