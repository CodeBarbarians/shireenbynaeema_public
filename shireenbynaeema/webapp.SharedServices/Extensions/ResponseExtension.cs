namespace SharedServices
{
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Extension methods for the IResponse interface, providing convenient methods to set success and failure responses with optional logging. These extension methods
    /// allow for a more fluent and consistent way to create response objects throughout the application, while also providing the ability to log failure details when
    /// necessary. The SetSuccess method sets the response as successful with a message and optional data, while the SetFailure method sets the response as a failure
    /// with a message, optional data, an exception, and an optional logger to log the failure details. This promotes better code readability and maintainability by
    /// centralizing response handling logic in one place.
    /// </summary>
    public static class ResponseExtension
    {
        private const string Message = "Failure: {Message} | Data: {@Data}";

        /// <summary>
        /// Sets the response as successful and updates its message and data properties.
        /// </summary>
        /// <remarks>This extension method is typically used to standardize the process of marking a
        /// response as successful in application workflows. The method sets the response's IsSuccess property to <see
        /// langword="true"/> and updates the Message and Data properties accordingly.</remarks>
        /// <param name="response">The response object to update. Cannot be null.</param>
        /// <param name="message">The message to associate with the successful response.</param>
        /// <param name="data">An optional data object to include with the response. Can be null.</param>
        /// <returns>The updated response object with success status, message, and data set.</returns>
        public static IResponse SetSuccess(this IResponse response, string message, object? data = null)
        {
            response.IsSuccess = true;
            response.Message = message;
            response.Data = data;
            return response;
        }

        /// <summary>
        /// Sets the response as a failure with a message, optional data, an exception, and an optional logger to log the failure details. If a logger is provided,
        /// it will log the failure message and data, and if an exception is provided, it will log the exception with its stack trace for better debugging and\
        /// monitoring of failures within the application.
        /// </summary>
        /// <param name="response">resonse.</param>
        /// <param name="message">message.</param>
        /// <param name="data">data.</param>
        /// <param name="ex">ex.</param>
        /// <param name="logger">logger.</param>
        /// <returns>IResponse.</returns>
        public static IResponse SetFailure(this IResponse response, string message, object? data = null, Exception? ex = null, ILogger? logger = null)
        {
            response.IsSuccess = false;
            response.Message = message;
            response.Data = data;

            if (logger != null)
            {
                if (ex != null)
                {
                    // Logs exception with full stack trace
                    logger.LogError(ex, Message, message, data);
                }
                else
                {
                    // No exception, just log message and data
                    logger.LogError("Failure: {Message} | Data: {@Data}", message, data);
                }
            }

            return response;
        }
    }
}