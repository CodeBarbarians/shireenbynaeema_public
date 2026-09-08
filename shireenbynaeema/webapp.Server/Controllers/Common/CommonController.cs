namespace Server
{
    using Application;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Provides common API endpoints for sending test emails and managing user notifications. Requires authentication
    /// for all actions.
    /// </summary>
    /// <param name="notificationService">The notification service used to retrieve and update user notifications.</param>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CommonController(INotificationService notificationService) : ControllerBase
    {
        /// <summary>
        /// Handles HTTP GET request to retrieve notifications
        /// for the authenticated user.
        /// </summary>
        /// <returns>
        /// Returns 200 OK with the list of user notifications.
        /// </returns>
        [HttpGet]
        [Route("GetNotifications")]
        public async Task<ActionResult> GetNotifications()
        {
            var resp = await notificationService.GetNotificationsAsync();
            return this.Ok(resp);
        }

        /// <summary>
        /// Handles HTTP POST request to mark all notifications
        /// as read for the authenticated user.
        /// </summary>
        /// <returns>
        /// Returns 200 OK after successfully marking all notifications as read.
        /// </returns>
        [HttpPost]
        [Route("MarkAllNotificationsAsRead")]
        public async Task<ActionResult> MarkAllAsReadNotificationsAsync()
        {
            var resp = await notificationService.MarkAllAsReadNotificationsAsync();
            return this.Ok(resp);
        }
    }
}