namespace Infrastructure
{
    using Application;
    using Domain;
    using Microsoft.EntityFrameworkCore;
    using SharedServices;

    /// <summary>
    /// Provides notification and email services for users, including sending notifications, managing notification
    /// settings, and delivering bulk email alerts based on user preferences and notification categories.
    /// </summary>
    /// <remarks>This service supports both internal notifications and email notifications, honoring
    /// user-specific preferences for each notification type and category. It enables bulk operations for efficient
    /// processing and ensures notifications are delivered or skipped according to user settings. Thread safety and
    /// transactional consistency depend on the underlying database context implementation.</remarks>
    /// <param name="dbContext">The database context used to access and persist notification and notification settings data.</param>
    /// <param name="response">The response object used to construct and return operation results for notification and email actions.</param>
    public class NotificationService(DatabaseContext dbContext, IResponse response) : INotificationService
    {
        /// <summary>
        /// Sends a notification to a user based on their notification settings and the specified category.
        /// </summary>
        /// <param name="notification">
        /// A <see cref="Notification"/> object containing:
        /// <list type="bullet">
        /// <item>UserId: the recipient user's ID.</item>
        /// <item>Message / Content: the notification message to send.</item>
        /// <item>Other relevant notification metadata.</item>
        /// </list>
        /// </param>
        /// <param name="category">
        /// A <see cref="NotificationCategory"/> indicating the type of notification, e.g., UserOnBoarding, EstimateResponse, or InternalNotifications.
        /// </param>
        /// <returns>
        /// An <see cref="IResponse"/> containing:
        /// <list type="bullet">
        /// <item>A success flag indicating whether the notification was saved or skipped.</item>
        /// <item>The saved <see cref="Notification"/> object if persisted.</item>
        /// <item>A message describing the outcome, e.g., "Notification saved" or "Notification skipped (disabled by user)".</item>
        /// </list>
        /// </returns>
        public async Task<IResponse> NotifyAsync(Notification notification, NotificationCategory category)
        {
            if (category == NotificationCategory.UserOnBoarding || category == NotificationCategory.EstimateResponse || category == NotificationCategory.EstimateCreation)
            {
                dbContext.Notifications.Add(notification);
                await dbContext.SaveChangesAsync().ConfigureAwait(false);
            }
            else
            {
                var canNotify = await this.GetNotificationSetting(notification.UserId, NotificationType.InternalNotications, category).ConfigureAwait(false);

                if (!canNotify)
                {
                    return response.SetSuccess("Notification skipped (disabled by user)", null);
                }

                dbContext.Notifications.Add(notification);
                await dbContext.SaveChangesAsync().ConfigureAwait(false);
            }

            return response.SetSuccess("Notification saved", notification);
        }

        /// <summary>
        /// Sends bulk notifications to multiple users based on their individual notification settings and the specified category.
        /// </summary>
        /// <param name="notifications">
        /// A <see cref="List{Notification}"/> containing:
        /// <list type="bullet">
        /// <item>UserId: the recipient user's ID.</item>
        /// <item>Title: the notification title.</item>
        /// <item>Message: the notification message.</item>
        /// <item>NavigationUrl: optional URL for user navigation from the notification.</item>
        /// </list>
        /// </param>
        /// <param name="category">
        /// A <see cref="NotificationCategory"/> indicating the type of notifications, e.g., InternalNotifications or UserOnBoarding.
        /// </param>
        /// <returns>
        /// An <see cref="IResponse"/> containing:
        /// <list type="bullet">
        /// <item>The count of notifications successfully saved.</item>
        /// <item>The count of notifications skipped due to user settings.</item>
        /// <item>A message describing the overall processing outcome, e.g., "Notifications processed".</item>
        /// </list>
        /// </returns>
        public async Task<IResponse> BulkNotifyAsync(List<Notification> notifications, NotificationCategory category)
        {
            var validNotifications = new List<Notification>();

            foreach (var notification in notifications)
            {
                var canNotify = await this.GetNotificationSetting(notification.UserId, NotificationType.InternalNotications, category).ConfigureAwait(false);
                if (canNotify)
                {
                    validNotifications.Add(new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = notification.UserId,
                        Title = notification.Title,
                        NavigationUrl = notification.NavigationUrl,
                        Message = notification.Message,
                    });
                }
            }

            if (validNotifications.Count != 0)
            {
                dbContext.Notifications.AddRange(validNotifications);
                await dbContext.SaveChangesAsync().ConfigureAwait(false);
            }

            return response.SetSuccess("Notifications processed", new
            {
                Saved = validNotifications.Count,
                Skipped = notifications.Count - validNotifications.Count,
            });
        }

        /// <summary>
        /// Marks all notifications as read for the currently logged-in user.
        /// </summary>
        /// <returns>
        /// An <see cref="IResponse"/> containing:
        /// <list type="bullet">
        /// <item>A success message indicating that notifications have been marked as read.</item>
        /// </list>
        /// </returns>
        public async Task<IResponse> MarkAllAsReadNotificationsAsync()
        {
            var query = dbContext.Notifications
                .Where(n => n.UserId == CurrentUser.UserId);

            // mark as read in one DB call
            await query.ExecuteUpdateAsync(u => u.SetProperty(n => n.IsRead, true)).ConfigureAwait(false);

            return response.SetSuccess("Notifications Marked as Read");
        }

        /// <summary>
        /// Retrieves a list of notifications for the currently logged-in user, ordered by most recent first.
        /// </summary>
        /// <returns>
        /// An <see cref="IResponse"/> containing:
        /// <list type="bullet">
        /// <item>A paginated list of notifications with details such as Title, Message, NavigationUrl, read status, and formatted timestamps.</item>
        /// <item>The total count of notifications retrieved.</item>
        /// </list>
        /// </returns>
        public async Task<IResponse> GetNotificationsAsync()
        {
            var query = dbContext.Notifications
                .Where(n => n.UserId == CurrentUser.UserId);

            var notifications = await query
                .OrderByDescending(n => n.CreatedOn)
                .Select(x => new Notification_Listing
                {
                    Title = x.Title,
                    Message = x.Message,
                    NavigationUrl = x.NavigationUrl,
                    IsRead = x.IsRead,
                    CreatedOn = x.CreatedOn.ToFormattedDateTime(Constants.DateTime.ListingFormat),
                    CreatedOnDate = x.CreatedOn.ToFormattedDateTime(Constants.DateTime.ListingFormatDate),
                    CreatedOnTime = x.CreatedOn.ToFormattedDateTime(Constants.DateTime.ListingFormatTime),
                    CreatedOnUnix = x.CreatedOn,
                })
                .ToListAsync().ConfigureAwait(false);
            var totalCount = notifications.Count;
            var data = notifications.ToListResponse(totalCount);

            return response.SetSuccess("Notifications listed", data);
        }

        /// <summary>
        /// Retrieves the notification setting for a specific <see cref="User"/>, notification type, and category.
        /// </summary>
        /// <param name="user">
        /// The <see cref="User"/> for whom the notification setting is being checked.
        /// </param>
        /// <param name="type">
        /// The <see cref="NotificationType"/> type (e.g., EmailNotications, InternalNotications) to check.
        /// </param>
        /// <param name="category">
        /// The <see cref="NotificationCategory"/> indicating the category of notification (e.g., WorkOrderCompleted, NewPublicNote).
        /// </param>
        /// <returns>
        /// A <see cref="bool"/> indicating whether the user has enabled notifications for the given type and category.
        /// </returns>
        public async Task<bool> GetNotificationSetting(User user, NotificationType type, NotificationCategory category)
        {
            if (user == null)
            {
                return false;
            }

            return await this.GetNotificationSetting(user.Id, type, category);
        }

        /// <summary>
        /// Retrieves the notification setting for a specific user ID, notification type, and category.
        /// </summary>
        /// <param name="userId">
        /// The <see cref="Guid"/> representing the user whose notification setting is being checked.
        /// </param>
        /// <param name="type">
        /// The <see cref="NotificationType"/> type (e.g., EmailNotications, InternalNotications) to check.
        /// </param>
        /// <param name="category">
        /// The <see cref="NotificationCategory"/> indicating the category of notification (e.g., WorkOrderCompleted, NewPublicNote).
        /// </param>
        /// <returns>
        /// A <see cref="bool"/> indicating whether the user has enabled notifications for the given type and category.
        /// </returns>
        public async Task<bool> GetNotificationSetting(Guid userId, NotificationType type, NotificationCategory category)
        {
            var setting = await dbContext.NotificationSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(ns => ns.UserId == userId && ns.NotificationsType == type);

            if (setting == null)
            {
                return false;
            }

            return category switch
            {
                NotificationCategory.QuoteProposal => setting.QuoteProposalNotification,
                NotificationCategory.WorkOrderStatusChange => setting.WorkOrderStatusChange,
                NotificationCategory.NewPublicNote => setting.NewPublicNoteonWorkOrder,
                NotificationCategory.WorkOrderCompleted => setting.WorkOrderCompleted,
                _ => false,
            };
        }
    }
}