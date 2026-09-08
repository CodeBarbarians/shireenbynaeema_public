namespace Application
{
    /// <summary>
    /// Service responsible for recording user login activity.
    /// Captures both successful and failed authentication attempts.
    /// </summary>
    public interface IUserLoginLogService
    {
        /// <summary>
        /// Records a login attempt for a user.
        /// </summary>
        /// <param name="userId">Unique identifier of the user if available.</param>
        /// <param name="email">Email used during the login attempt.</param>
        /// <param name="isSuccess">Indicates whether the login attempt was successful.</param>
        /// <param name="message">Optional message describing the login result or failure reason.</param>
        /// <returns>A task representing the asynchronous logging operation.</returns>
        Task LogAsync(Guid? userId, string email, bool isSuccess, string? message = null);
    }
}