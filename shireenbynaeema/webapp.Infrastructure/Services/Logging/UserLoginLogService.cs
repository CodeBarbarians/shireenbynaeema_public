namespace Infrastructure
{
    using Application;
    using Domain;
    using Microsoft.AspNetCore.Http;
    using SharedServices;

    /// <summary>
    /// Provides functionality for recording user login attempts, including details such as user identity, login
    /// outcome, and request context information.
    /// </summary>
    /// <remarks>This service is typically used to audit authentication activity by persisting login attempts
    /// to the database. It captures contextual information such as IP address and user agent from the current HTTP
    /// request. The service is intended to be used in web application scenarios where tracking login events is required
    /// for security or compliance purposes.</remarks>
    public class UserLoginLogService : IUserLoginLogService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly DatabaseContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserLoginLogService"/> class with the specified HTTP context accessor and.
        /// database context.
        /// </summary>
        /// <param name="httpContextAccessor">An accessor to the current HTTP context, used to retrieve information about the current user or request.</param>
        /// <param name="dbContext">The database context used to access and manage user login log data.</param>
        public UserLoginLogService(IHttpContextAccessor httpContextAccessor, DatabaseContext dbContext)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Records a user login attempt in the database, capturing details such as the user ID, email,
        /// whether the login was successful, IP address, user agent, timestamp, and an optional message.
        /// </summary>
        /// <param name="userId">The unique identifier of the user attempting to log in (nullable for unknown users).</param>
        /// <param name="email">The email address used for the login attempt.</param>
        /// <param name="isSuccess">Indicates whether the login attempt was successful.</param>
        /// <param name="message">Optional message describing the login attempt or error.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task LogAsync(Guid? userId, string email, bool isSuccess, string? message = null)
        {
            var context = this.httpContextAccessor.HttpContext;

            var log = new UserLoginLog
            {
                UserId = userId,
                Email = email,
                IsSuccessful = isSuccess,
                AttemptedAt = DateTimeExtension.UtcNowUnixTimestamp,
                IPAddress = context?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown",
                UserAgent = context?.Request?.Headers["User-Agent"].ToString() ?? "Unknown",
                Status = (int)StatusType.NotUsed,
                Message = message ?? string.Empty,
            };

            this.dbContext.UserLoginLogs.Add(log);
            await this.dbContext.SaveChangesAsync();
        }
    }
}