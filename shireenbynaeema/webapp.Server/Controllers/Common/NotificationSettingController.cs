namespace Server
{
    using Application;
    using Domain;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Represents an API controller that manages notification settings and profile details for the authenticated user.
    /// </summary>
    /// <remarks>All endpoints in this controller require authentication. Use this controller to update or
    /// retrieve notification preferences and profile information for the currently authenticated user.</remarks>
    /// <param name="notificationSettingService">The service used to manage notification settings for users.</param>
    /// <param name="profileDetailsService">The service used to retrieve profile details for users.</param>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationSettingController(INotificationSettingService notificationSettingService, IProfileDetailsService profileDetailsService) : ControllerBase
    {
        /// <summary>
        /// Handles HTTP POST request to update notification settings for the authenticated user.
        /// This endpoint allows users to modify their notification preferences by sending a request containing the new settings.
        /// </summary>
        /// <param name="request">request.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpPost]
        [Route("Update")]
        public async Task<IActionResult> Update(NotificationSettingRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            var resp = await notificationSettingService.UpdateNotificationSettingRange(request);
            return this.Ok(resp);
        }

        /// <summary>
        /// Handles HTTP GET request to retrieve the current notification settings for the authenticated user.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpGet]
        [Route("Retrieve")]
        public async Task<IActionResult> Retrieve()
        {
            var resp = await notificationSettingService.RetriveNotificationSetting();
            return this.Ok(resp);
        }

        /// <summary>
        /// Handles HTTP GET request to retrieve profile details for the authenticated user.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpGet]
        [Route("RetrieveProfileDetails")]
        public async Task<IActionResult> RetrieveProfileDetails()
        {
            var resp = await profileDetailsService.GetProfileDetials();
            return this.Ok(resp);
        }
    }
}