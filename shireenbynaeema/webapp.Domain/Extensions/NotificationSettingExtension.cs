namespace Domain
{
    using SharedServices;

    /// <summary>
    /// Extension methods for converting NotificationSetting_AddEdit DTOs to NotificationSetting entities.
    /// </summary>
    public static class NotificationSettingExtensions
    {
        extension(List<NotificationSetting_AddEdit> request)
        {
            /// <summary>
            /// Converts a list of NotificationSetting_AddEdit DTOs to entities.
            /// </summary>
            /// <exception cref="ArgumentNullException">ArgumentNullException.</exception>
            public List<NotificationSetting> ToEntity()
            {
                ArgumentNullException.ThrowIfNull(request);

                return request.Select(r => new NotificationSetting
                {
                    Id = r.Id ?? Guid.NewGuid(),
                    UserId = r.UserId,
                    Status = r.Status ?? StatusType.Active,
                    WorkOrderStatusChange = r.WorkOrderStatusChange,
                    QuoteProposalNotification = r.QuoteProposalNotification,
                    NewPublicNoteonWorkOrder = r.NewPublicNoteonWorkOrder,
                    WorkOrderCompleted = r.WorkOrderCompleted,
                    NotificationsType = r.NotificationsType,
                }).ToList();
            }
        }
    }
}