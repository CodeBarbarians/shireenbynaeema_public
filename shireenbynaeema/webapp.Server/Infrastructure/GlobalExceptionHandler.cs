namespace Server
{
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    using Microsoft.AspNetCore.Diagnostics;
    using SharedServices;

    /// <summary>
    /// Provides a global exception handler for HTTP requests, mapping exceptions to appropriate HTTP status codes and
    /// standardized JSON error responses. Implements centralized error handling and logging for web applications.
    /// </summary>
    /// <remarks>This handler is intended to be registered as a global exception handler in ASP.NET Core
    /// applications. It ensures consistent error responses and logging across the application. The level of detail in
    /// error messages may vary depending on the hosting environment (development or production).</remarks>
    /// <param name="logger">The logger used to record exception details and error information.</param>
    /// <param name="response">The response object used to construct and return standardized error responses to the client.</param>
    /// <param name="environment">The web hosting environment, used to determine whether to include detailed error information in responses (e.g.,
    /// in development mode).</param>
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IResponse response, IWebHostEnvironment environment) : IExceptionHandler
    {
        /// <summary>
        /// Attempts to handle the specified exception by writing an appropriate JSON error response to the HTTP context
        /// asynchronously.
        /// </summary>
        /// <remarks>The response includes a status code and a JSON body describing the error. The method
        /// logs the exception at a level appropriate to the error type. The response is not written if the operation is
        /// canceled.</remarks>
        /// <param name="httpContext">The HTTP context for the current request. The response will be written to this context.</param>
        /// <param name="exception">The exception to handle and serialize into the response.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A value task that represents the asynchronous operation. The result is <see langword="true"/> if the
        /// exception was handled and a response was written; otherwise, <see langword="false"/>.</returns>
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            // Determine status code and message based on exception type
            var (statusCode, message) = this.GetStatusCodeAndMessage(exception);

            // Log the exception with appropriate level
            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                logger.LogError(exception, "Unhandled exception occurred.");
            }
            else if (statusCode == StatusCodes.Status400BadRequest)
            {
                logger.LogWarning(exception, "Validation error occurred.");
            }
            else if (statusCode == StatusCodes.Status409Conflict)
            {
                logger.LogWarning(exception, "Concurrency error occurred.");
            }
            else
            {
                logger.LogWarning(exception, "Request error occurred.");
            }

            httpContext.Response.StatusCode = statusCode;

            response.IsSuccess = false;
            response.Message = message;
            response.Code = this.GetErrorCode(statusCode);

            await httpContext.Response.WriteAsJsonAsync(response, options, cancellationToken);

            return true;
        }

        /// <summary>
        /// Determines the appropriate HTTP status code and message based on the exception type.
        /// </summary>
        /// <param name="exception">The exception to evaluate.</param>
        /// <returns>A tuple containing the HTTP status code and error message.</returns>
        private (int StatusCode, string Message) GetStatusCodeAndMessage(Exception exception)
        {
            return exception switch
            {
                // Validation errors - 400 Bad Request
                ValidationException => (
                    StatusCodes.Status400BadRequest,
                    exception.Message),

                // Concurrency errors - 409 Conflict
                Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException => (
                    StatusCodes.Status409Conflict,
                    "The record has been modified or deleted by another user. Please refresh and try again."),

                // Database update errors - 500 Internal Server Error
                Microsoft.EntityFrameworkCore.DbUpdateException dbEx => (
                    StatusCodes.Status500InternalServerError,
                    environment.IsDevelopment()
                        ? dbEx.InnerException?.Message ?? dbEx.Message
                        : "An error occurred while updating the database."),

                // Argument validation - 400 Bad Request
                ArgumentException or ArgumentNullException => (
                    StatusCodes.Status400BadRequest,
                    exception.Message),

                // Unauthorized - 401 Unauthorized
                UnauthorizedAccessException => (
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized access."),

                // Not found - 404 Not Found
                KeyNotFoundException => (
                    StatusCodes.Status404NotFound,
                    "Resource not found."),

                // All other exceptions - 500 Internal Server Error
                _ => (
                    StatusCodes.Status500InternalServerError,
                    environment.IsDevelopment()
                        ? exception.Message
                        : "An unexpected error occurred."),
            };
        }

        /// <summary>
        /// Gets a standardized error code based on the HTTP status code.
        /// </summary>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <returns>A string representing the error code.</returns>
        private string GetErrorCode(int statusCode)
        {
            return statusCode switch
            {
                StatusCodes.Status400BadRequest => "BadRequest",
                StatusCodes.Status401Unauthorized => "Unauthorized",
                StatusCodes.Status404NotFound => "NotFound",
                StatusCodes.Status409Conflict => "Conflict",
                StatusCodes.Status500InternalServerError => "InternalServerError",
                _ => "Error",
            };
        }
    }
}