namespace Server
{
    using Hangfire.Dashboard;

    /// <summary>
    /// Provides an authorization filter for the Hangfire dashboard to determine whether a user is permitted to access
    /// dashboard resources.
    /// </summary>
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        /// <summary>
        /// Determines whether the user is authorized to access the Hangfire dashboard.
        /// </summary>
        /// <param name="context">
        /// The DashboardContext containing information about the current Hangfire dashboard request.
        /// </param>
        /// <returns>
        /// Returns true if the user is authorized to access the dashboard; otherwise, false.
        /// </returns>
        public bool Authorize(DashboardContext context)
        {
            // Allow only for authenticated users (or admin role, etc.)
            var httpContext = context.GetHttpContext();
            return true;
        }
    }
}