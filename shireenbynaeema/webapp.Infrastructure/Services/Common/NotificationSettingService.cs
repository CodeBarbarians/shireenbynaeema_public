namespace Infrastructure
{
    using Application;
    using Domain;
    using Microsoft.EntityFrameworkCore;
    using SharedServices;

    /// <summary>
    /// Provides services for managing notification settings for users, including creating, updating, and retrieving
    /// notification preferences.
    /// </summary>
    /// <remarks>This service enables applications to handle user notification settings for various notification
    /// types, such as email and internal notifications. It supports adding default settings for users, updating multiple
    /// settings in bulk, and retrieving the current user's notification preferences. All operations return a response
    /// indicating the outcome and relevant data. Thread safety depends on the underlying database context and repository
    /// implementations.</remarks>
    public class NotificationSettingService : Service<NotificationSetting>, INotificationSettingService
    {
        private readonly DatabaseContext databaseContext;
        private readonly IResponse response;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationSettingService"/> class with the specified repository, database.
        /// context, and <see cref="response"/> handler.
        /// </summary>
        /// <param name="repository">The repository used to access and manage NotificationSetting entities.</param>
        /// <param name="dbContext">The database context that provides access to the underlying data store.</param>
        /// <param name="resp">The response handler used to manage service responses and result formatting.</param>
        public NotificationSettingService(IRepository<NotificationSetting> repository, DatabaseContext dbContext, IResponse resp)
            : base(repository, resp)
        {
            this.databaseContext = dbContext;
            this.response = resp;
        }

        /// <summary>
        /// Adds default notification settings for a user if they do not already exist.
        /// </summary>
        /// <param name="userId">
        /// The <see cref="Guid"/> of the user for whom the notification settings will be created.
        /// </param>
        /// <returns>
        /// An <see cref="IResponse"/> indicating the result:
        /// <list type="bullet">
        /// <item>Creates new notification settings for missing types (EmailNotications, InternalNotications).</item>
        /// <item>If all settings already exist, returns a success response indicating no changes were made.</item>
        /// </list>
        /// </returns>
        public async Task<IResponse> AddNotificationSetting(Guid userId)
        {
            var existingSettings = await this.databaseContext.NotificationSettings
                .Where(x => x.UserId == userId)
                .Select(x => x.NotificationsType)
                .ToListAsync();

            var newSettings = new List<NotificationSetting>();

            if (!existingSettings.Contains(NotificationType.EmailNotications))
            {
                newSettings.Add(new NotificationSetting
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Status = StatusType.Active,
                    WorkOrderStatusChange = false,
                    QuoteProposalNotification = true,
                    NewPublicNoteonWorkOrder = false,
                    WorkOrderCompleted = false,
                    NotificationsType = NotificationType.EmailNotications,
                });
            }

            if (!existingSettings.Contains(NotificationType.InternalNotications))
            {
                newSettings.Add(new NotificationSetting
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Status = StatusType.Active,
                    WorkOrderStatusChange = false,
                    QuoteProposalNotification = true,
                    NewPublicNoteonWorkOrder = false,
                    WorkOrderCompleted = false,
                    NotificationsType = NotificationType.InternalNotications,
                });
            }

            if (newSettings.Count != 0)
            {
                return await this.AddRangeAsync(newSettings);
            }

            return this.response.SetSuccess("User already has notification settings");
        }

        /// <summary>
        /// Updates multiple notification settings for the current user based on the provided request.
        /// </summary>
        /// <param name="request">
        /// A <see cref="NotificationSettingRequest"/> containing:
        /// <list type="bullet">
        /// <item>A collection of notification settings to update.</item>
        /// <item>Each setting includes the flags for:
        /// <list type="bullet">
        /// <item>QuoteProposalNotification</item>
        /// <item>WorkOrderStatusChange</item>
        /// <item>NewPublicNoteonWorkOrder</item>
        /// <item>WorkOrderCompleted</item>
        /// </list>
        /// </item>
        /// </list>
        /// </param>
        /// <returns>
        /// An <see cref="IResponse"/> indicating the result:
        /// <list type="bullet">
        /// <item>If the current user has no existing settings, returns a failure response with a not found message.</item>
        /// <item>If settings are found, updates the specified flags and returns a success response.</item>
        /// </list>
        /// </returns>
        public async Task<IResponse> UpdateNotificationSettingRange(NotificationSettingRequest request)
        {
            var existingItem = await this.databaseContext.NotificationSettings.FirstOrDefaultAsync(x => x.UserId == CurrentUser.UserId);
            if (existingItem == null)
            {
                this.response.IsSuccess = Constants.ResponseFailure;
                this.response.Message = Constants.NotFound.FormatWith(this.ModuleDisplayName);
                return this.response;
            }

            var entities = new List<NotificationSetting>();

            foreach (var setting in request.Settings)
            {
                var entity = await this.databaseContext.NotificationSettings.FirstOrDefaultAsync(x => x.Id == setting.Id && x.UserId == setting.UserId);
                if (entity != null)
                {
                    entity.QuoteProposalNotification = setting.QuoteProposalNotification;
                    entity.WorkOrderStatusChange = setting.WorkOrderStatusChange;
                    entity.NewPublicNoteonWorkOrder = setting.NewPublicNoteonWorkOrder;
                    entity.WorkOrderCompleted = setting.WorkOrderCompleted;
                    entities.Add(entity);
                }
            }

            this.databaseContext.NotificationSettings.UpdateRange(entities);
            await this.databaseContext.SaveChangesAsync();

            this.response.IsSuccess = Constants.ResponseSuccess;
            this.response.Message = Constants.UpdateSuccess.FormatWith(this.ModuleDisplayName);
            return this.response;
        }

        /// <summary>
        /// Retrieves the notification settings for the currently logged-in user.
        /// </summary>
        /// <returns>
        /// An <see cref="IResponse"/> containing:
        /// <list type="bullet">
        /// <item>The list of notification settings for the current user.</item>
        /// <item>Each setting includes:
        /// <list type="bullet">
        /// <item>Id</item>
        /// <item>UserId</item>
        /// <item>NewPublicNoteonWorkOrder</item>
        /// <item>QuoteProposalNotification</item>
        /// <item>WorkOrderCompleted</item>
        /// <item>WorkOrderStatusChange</item>
        /// <item>NotificationsType</item>
        /// <item>Status</item>
        /// </list>
        /// </item>
        /// <item>A success message indicating the settings were retrieved.</item>
        /// </list>
        /// </returns>
        public async Task<IResponse> RetriveNotificationSetting()
        {
            var data = await (from nset in this.databaseContext.NotificationSettings
                              where nset.UserId == CurrentUser.UserId
                              select new NotificationSetting_AddEdit()
                              {
                                  Id = nset.Id,
                                  UserId = nset.UserId,
                                  NewPublicNoteonWorkOrder = nset.NewPublicNoteonWorkOrder,
                                  QuoteProposalNotification = nset.QuoteProposalNotification,
                                  WorkOrderCompleted = nset.WorkOrderCompleted,
                                  WorkOrderStatusChange = nset.WorkOrderStatusChange,
                                  NotificationsType = nset.NotificationsType,
                                  Status = nset.Status,
                              }).ToListAsync();

            this.response.IsSuccess = Constants.ResponseSuccess;
            this.response.Message = Constants.RetrieveSuccess.FormatWith(this.ModuleDisplayName);
            this.response.Data = new NotificationSettingRequest() { Settings = data }.ToRetrieveResponse();
            return this.response;
        }
    }
}