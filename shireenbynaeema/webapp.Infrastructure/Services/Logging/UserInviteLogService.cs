namespace Infrastructure
{
    using Application;
    using Domain;
    using Microsoft.EntityFrameworkCore;
    using SharedServices;

    /// <summary>
    /// Provides operations for retrieving and managing user invite log entries, including support for detailed
    /// retrieval and paginated listing.
    /// </summary>
    /// <remarks>This service is intended for use in scenarios where tracking, auditing, or displaying user
    /// invitation activity is required. It supports mapping log entries to data transfer objects (DTOs) for response
    /// formatting and integrates with a database context for data access. Thread safety depends on the underlying
    /// database context and repository implementations.</remarks>
    public class UserInviteLogService : Service<UserInviteLog>, IUserInviteLogService
    {
        private readonly DatabaseContext dbContext;
        private readonly IResponse response;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserInviteLogService"/> class with the specified repository, response.
        /// handler, and database context.
        /// </summary>
        /// <param name="repository">The repository used to access UserInviteLog entities.</param>
        /// <param name="response">The response handler for managing service responses.</param>
        /// <param name="dbContext">The database context used for data operations within the service.</param>
        public UserInviteLogService(IRepository<UserInviteLog> repository, IResponse response, DatabaseContext dbContext)
            : base(repository, response)
        {
            this.response = response;
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves a user invite log entry by its unique identifier and maps it to a DTO for response.
        /// </summary>
        /// <param name="id">The unique identifier of the user invite log.</param>
        /// <returns>An <see cref="IResponse"/> containing the mapped user invite log data.</returns>
        public async Task<IResponse> RetrieveUserInviteLog(Guid id)
        {
            var logs = await this.dbContext.UserInviteLogs
                .Where(o => o.Id == id)
                .Select(o => new UserInviteLog_AddEdit
                {
                    Id = o.Id,
                    RoleId = o.RoleId,
                    SentOn = o.SentOn,
                    IsSuccess = o.IsSuccess,
                })
                .FirstOrDefaultAsync();

            var resp = new RetrieveResponse<UserInviteLog_AddEdit>(logs);
            this.response.Data = resp;
            this.response.IsSuccess = Constants.ResponseSuccess;
            this.response.Message = Constants.RetrieveSuccess.FormatWith(this.ModuleName);
            return this.response;
        }

        /// <summary>
        /// Retrieves a paginated list of user invite log entries based on the specified request criteria.
        /// </summary>
        /// <param name="request">The request parameters that define filtering, paging, and sorting options for the user invite log listing.
        /// Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IResponse"/> with
        /// a paginated list of user invite log entries and related metadata.</returns>
        public async Task<IResponse> ListUserInviteLog(ListRequest request)
        {
            var query = (from log in this.dbContext.UserInviteLogs
                         join user in this.dbContext.Users on log.UserId equals user.Id into usergroup
                         from user in usergroup.DefaultIfEmpty()
                         join role in this.dbContext.Roles on log.RoleId equals role.Id into rolegroup
                         from role in rolegroup.DefaultIfEmpty()
                         where log.IsSuccess.HasValue
                         select new UserInviteLog_Listing()
                         {
                             Id = log.Id,
                             RoleName = role.Name,
                             SentOn = log.SentOn.ToDateTime(),
                             IsSuccess = log.IsSuccess.HasValue && Enum.IsDefined(this.StatusEnumType, log.IsSuccess!.Value) ? ((StatusType)log.IsSuccess!.Value).ToString() : Constants.ListingEmpty,
                         }).ApplyOrdering(request);

            // var filtered = query.ApplyQuickSearch(request.SearchText);
            var result = await query
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync().ConfigureAwait(false);

            var total = await query.CountAsync().ConfigureAwait(false);

            var resp = new ListResponse<UserInviteLog_Listing>(pageNumber: request.PageNumber, pageSize: request.PageSize)
            {
                Entities = result,
                TotalCount = total,
            };

            this.response.Data = resp;
            this.response.IsSuccess = Constants.ResponseSuccess;
            this.response.Message = Constants.RetrieveSuccess.FormatWith(this.ModuleName);
            return this.response;
        }
    }
}