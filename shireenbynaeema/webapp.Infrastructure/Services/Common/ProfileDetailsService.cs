namespace Infrastructure
{
    using Application;
    using Domain;
    using Microsoft.EntityFrameworkCore;
    using SharedServices;

    /// <summary>
    /// Provides functionality to retrieve profile details for the currently authenticated user.
    /// </summary>
    /// <param name="dbContext">The database context used to access user, organization, and notification settings data.</param>
    /// <param name="response">The response object used to format and return profile information and status messages.</param>
    public class ProfileDetailsService(DatabaseContext dbContext, IResponse response) : IProfileDetailsService
    {
        /// <summary>
        /// Retrieves the profile details of the currently authenticated user.
        /// </summary>
        /// <returns>
        /// An <see cref="IResponse"/> containing:
        /// <list type="bullet">
        /// <item>The user's display name, email, and phone number.</item>
        /// <item>The role assigned to the user.</item>
        /// <item>The organization the user belongs to (if any).</item>
        /// <item>The invitation date and the name of the user who invited them (if applicable).</item>
        /// <item>The user's notification settings, including email and internal notification preferences.</item>
        /// <item>A success or failure message indicating whether the profile was found.</item>
        /// </list>
        /// </returns>
        public async Task<IResponse> GetProfileDetials()
        {
            var user = await (from usr in dbContext.Users
                              join invbyusr in dbContext.Users on usr.InvitedBy equals invbyusr.Id into invbyusrs
                              from invitedByUser in invbyusrs.DefaultIfEmpty()
                              where usr.Id == CurrentUser.UserId
                              select new UserDetails_Retrieve
                              {
                                  DisplayName = usr.DisplayName,
                                  Email = usr.Email,
                                  Role = (from usrrole in dbContext.UserRoles
                                          join roles in dbContext.Roles on usrrole.RoleId equals roles.Id
                                          where usrrole.UserId == CurrentUser.UserId
                                          select roles.Name).ToList(),
                                  PhoneNo = usr.PhoneNo ?? Constants.RetrieveEmpty,
                                  InvitationDateUnix = usr.InvitationDate,
                                  InvitedBy = invitedByUser.DisplayName,
                                  Settings = (from setting in dbContext.NotificationSettings
                                              where setting.UserId == usr.Id
                                              select new NotificationSetting_AddEdit
                                              {
                                                  Id = setting.Id,
                                                  UserId = setting.UserId,
                                                  WorkOrderStatusChange = setting.WorkOrderStatusChange,
                                                  QuoteProposalNotification = setting.QuoteProposalNotification,
                                                  NewPublicNoteonWorkOrder = setting.NewPublicNoteonWorkOrder,
                                                  WorkOrderCompleted = setting.WorkOrderCompleted,
                                                  NotificationsType = setting.NotificationsType,
                                              }).ToList(),
                              }).FirstOrDefaultAsync();

            return user != null
                ? response.SetSuccess(Constants.User.ProfileRetrieveSuccess, user.ToRetrieveResponse())
                : response.SetFailure(Constants.User.NotFound);
        }
    }
}