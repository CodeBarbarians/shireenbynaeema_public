namespace SharedServices
{
    using System.Text.Json;

    using Hangfire.Common;
    using Hangfire.Server;

    /// <summary>
    /// Hangfire filter attribute to capture and store the return value of a background job.
    /// </summary>
    public class CaptureReturnValueAttribute : JobFilterAttribute, IServerFilter
    {
        /// <summary>
        /// Called before the job is executed. This method does not perform any actions, but it is required by the IServerFilter interface. The actual capturing of the return value
        /// will occur in the OnPerformed method.
        /// </summary>
        /// <param name="context">PerformingContext.</param>
        public void OnPerforming(PerformingContext context)
        {
            // Nothing before execution
        }

        /// <summary>
        /// Called after the job has been executed. This method checks if the job execution was successful and if there is a return value. If so, it serializes the return value to a string
        /// and saves it to the Hangfire job parameters.
        /// </summary>
        /// <param name="context">PerformedContext.</param>
        public void OnPerformed(PerformedContext context)
        {
            // If job succeeded and has a result
            if (context.Exception == null && context.Result != null)
            {
                var resultString = JsonSerializer.Serialize(context.Result);

                // Save to Hangfire job parameters (persistent)
                using (var connection = context.Connection)
                {
                    connection.SetJobParameter(context.BackgroundJob.Id, "ReturnedValue", resultString);
                }

                // Also log for visibility in Dashboard logs
                Console.WriteLine($"[Hangfire Result] {resultString}");
            }
        }
    }
}