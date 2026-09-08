namespace Infrastructure
{
    using Application;

    using Domain;

    using Microsoft.EntityFrameworkCore;

    using SharedServices;

    using static SharedServices.Constants;

    /// <summary>
    /// Provides services for retrieving, filtering, and interpreting user and administrative activity logs within the
    /// application.
    /// </summary>
    /// <remarks>The ActivityLogService enables querying activity logs with support for filtering by user,
    /// action, module, status, and date range. It supports both user-specific and administrative views of activity
    /// logs, and provides methods to retrieve detailed information for individual log entries. This service is
    /// typically used to support audit trails, security reviews, and user activity monitoring features. Thread safety
    /// depends on the underlying DatabaseContext implementation; instances are generally intended to be used per
    /// request in web applications.</remarks>
    public class ActivityLogService : IActivityLogService
    {
        private readonly DatabaseContext dbContext;
        private readonly AuditCompareBuilder compareBuilder;
        private readonly AuditInterpreter auditInterpreter;
        private readonly IResponse response;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityLogService"/> class with the specified dependencies.
        /// </summary>
        /// <param name="dbContext">The database context used to access and manage activity log data. Cannot be null.</param>
        /// <param name="response">The response handler used to format or send responses. Cannot be null.</param>
        /// <param name="compareBuilder">The builder used to construct audit comparisons for activity logs. Cannot be null.</param>
        /// <param name="auditInterpreter">The interpreter used to process and interpret audit data. Cannot be null.</param>
        public ActivityLogService(
            DatabaseContext dbContext,
            IResponse response,
            AuditCompareBuilder compareBuilder,
            AuditInterpreter auditInterpreter)
        {
            this.dbContext = dbContext;
            this.response = response;
            this.compareBuilder = compareBuilder;
            this.auditInterpreter = auditInterpreter;
        }

        /// <summary>
        /// Asynchronously retrieves a paged list of activity logs based on the specified filter criteria.
        /// </summary>
        /// <remarks>If a user ID is provided in the filter request, only logs for that user are returned;
        /// otherwise, logs for all users are included. The response includes pagination and total count information. If
        /// the specified user does not exist, the response indicates failure.</remarks>
        /// <param name="request">An object containing filter parameters for the activity logs, such as user ID, date range, and pagination
        /// settings. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation. Optional.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IResponse"/> with
        /// the filtered and paged activity logs. If no logs are found, the response data will be an empty list.</returns>
        public async Task<IResponse> GetActivityLogsAsync(
            ActivityLogFilterRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                IQueryable<ActivityLogRaw> query;

                // USER MODE
                if (request.UserId.HasValue)
                {
                    var user = await this.dbContext.Users
                        .FirstOrDefaultAsync(x => x.Id == request.UserId.Value, cancellationToken);

                    if (user == null)
                    {
                        return this.response.SetFailure("User not found.");
                    }

                    query = this.BuildRegularUserQuery(user);
                }
                else
                {
                    query = this.BuildAdminUserQuery();
                }

                // Apply filters BEFORE projection
                query = this.ApplyFilters(query, request);

                query = query.OrderByDescending(x => x.Timestamp);

                var totalCount = await query.CountAsync(cancellationToken);

                var paged = await query
                    .Skip(request.Skip)
                    .Take(request.Take)
                    .ToListAsync(cancellationToken); // materialize FIRST

                // Batch-preload all FK entity display values across the page to avoid N+1 queries
                var auditLogEntries = paged
                    .Where(r => r.SourceTable == "AuditLog" && r.HasDetails)
                    .Select(r => (r.EntityName, r.Changes));

                await this.auditInterpreter.PreloadBatchAsync(auditLogEntries);

                // Projection AFTER materialization and preloading (IMPORTANT)
                var items = new List<ActivityLogDto>(paged.Count);
                foreach (var raw in paged)
                {
                    items.Add(await this.MapToDtoAsync(raw));
                }

                this.response.Data = items.ToListResponse(request, totalCount);
                this.response.IsSuccess = true;
                this.response.Message = ListSuccess.FormatWith("Activity Log");

                return this.response;
            }
            catch (Exception ex)
            {
                this.response.IsSuccess = false;
                this.response.Message = $"Failed to retrieve activity logs: {ex.Message}";
                return this.response;
            }
        }

        /// <summary>
        /// Asynchronously retrieves a single activity log entry by its unique identifier.
        /// </summary>
        /// <remarks>The returned response includes details about the activity, such as the action
        /// performed, status, user information, timestamp, and a summary of changes. If the specified activity log
        /// entry does not exist, the response indicates failure.</remarks>
        /// <param name="id">The unique identifier of the activity log entry to retrieve.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IResponse"/> with
        /// the activity log data if found; otherwise, a failure response indicating that the activity log was not
        /// found.</returns>
        public async Task<IResponse> GetActivityLogByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var raw = await
                         (
                             from log in this.dbContext.AuditLogs
                             where log.Id == id
                             join user in this.dbContext.Users
                                 on log.UserId equals user.Id.ToString()
                                 into userJoin
                             from user in userJoin.DefaultIfEmpty() // LEFT JOIN
                             select new ActivityLogRaw
                             {
                                 Id = log.Id,
                                 Timestamp = log.Timestamp,
                                 Action = log.Action,
                                 Module = log.EntityName,
                                 Status = ActivityLogStatuses.Update,
                                 Username = user != null ? user.GetDisplayName() : log.Username,
                                 Email = user != null ? user.Email : string.Empty,
                                 IPAddress = log.IPAddress ?? ListingEmpty,
                                 Description = log.Action + " " + log.EntityName,
                                 EntityName = log.EntityName,
                                 EntityId = log.EntityId,
                                 Changes = log.Changes,
                                 SourceTable = "AuditLog",
                             })
                         .FirstOrDefaultAsync(cancellationToken);

                if (raw == null)
                {
                    return this.response.SetFailure("Activity log not found.");
                }

                var dto = await this.MapToDtoAsync(raw);
                var rows = this.compareBuilder.Build(dto.Action, dto.Details);

                this.response.Data = new
                {
                    Title = $"{dto.Module} Change",
                    Action = dto.Action,
                    Status = dto.Status,
                    Email = dto.Email,
                    Timestamp = dto.FormattedDate + " " + dto.FormattedTime,
                    User = dto.Username,
                    Changes = rows,
                }.ToSaveRequest();

                this.response.IsSuccess = true;
                this.response.Message = "Activity log retrieved successfully";

                return this.response;
            }
            catch (Exception ex)
            {
                this.response.IsSuccess = false;
                this.response.Message = $"Failed to retrieve activity log: {ex.Message}";
                return this.response;
            }
        }

        /// <summary>
        /// Retrieves the names of all non-owned entity types defined in the current database context model.
        /// </summary>
        /// <remarks>Owned entity types are excluded from the result. The returned names correspond to the
        /// CLR type names of the entities as defined in the model.</remarks>
        /// <returns>A list of strings containing the names of all non-owned entity types in the model, sorted alphabetically.
        /// The list is empty if no such entities are defined.</returns>
        public List<string> GetEntityNames()
        {
            using var context = this.dbContext;

            return context.Model
                .GetEntityTypes()
                .Where(e => !e.IsOwned())
                .Select(e => e.ClrType.Name)
                .Distinct()
                .OrderBy(n => n)
                .ToList();
        }

        /// <summary>
        /// Retrieves the available filter options for activity log queries, including actions, modules, and statuses.
        /// </summary>
        /// <remarks>The returned filter options can be used to construct queries or user interface
        /// elements for filtering activity logs by action, module, or status. The set of modules includes both
        /// predefined modules and additional entity names retrieved at runtime.</remarks>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IResponse"/> with
        /// the filter options data for activity logs.</returns>
        public Task<IResponse> GetFilterOptions()
        {
            var options = new
            {
                Actions = new List<string>
                {
                    ActivityLogActions.Login,
                    ActivityLogActions.PasswordChanged,
                    ActivityLogActions.Added,
                    ActivityLogActions.Modified,
                    ActivityLogActions.Deleted,
                },
                Modules = new List<string>
                {
                    ActivityLogModules.Authentication,
                    ActivityLogModules.Security,
                },
                Statuses = new List<string>
                {
                    ActivityLogStatuses.Success,
                    ActivityLogStatuses.Failed,
                    ActivityLogStatuses.Update,
                },
            };

            options.Modules.AddRange(this.GetEntityNames());

            return Task.FromResult(this.response.SetSuccess(data: options.ToSaveRequest(), message: "Filter-options retreived Successfully"));
        }

        /// <summary>
        /// Applies filtering criteria from the specified filter request to the activity log query.
        /// </summary>
        /// <remarks>This method does not execute the query; it returns a queryable object with the
        /// applied filters. The returned query can be further composed or executed as needed. Filtering includes
        /// matching actions, modules, statuses, search text within certain fields, and an optional date range. If a
        /// filter criterion is not specified, it is ignored.</remarks>
        /// <param name="query">The initial queryable collection of activity log records to filter.</param>
        /// <param name="request">An object containing filter criteria such as actions, modules, statuses, search text, and date range to
        /// apply to the query. Cannot be null.</param>
        /// <returns>An <see cref="IQueryable{ActivityLogRaw}"/> representing the filtered activity log records that match the
        /// specified criteria.</returns>
        private IQueryable<ActivityLogRaw> ApplyFilters(IQueryable<ActivityLogRaw> query, ActivityLogFilterRequest request)
        {
            if (request.Actions != null && request.Actions.Count != 0)
            {
                query = query.Where(x => request.Actions.Contains(x.Action));
            }

            if (request.Modules != null && request.Modules.Count != 0)
            {
                query = query.Where(x => request.Modules.Contains(x.Module));
            }

            if (request.Statuses != null && request.Statuses.Count != 0)
            {
                query = query.Where(x => request.Statuses.Contains(x.Status));
            }

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var s = request.SearchText.Trim();
                query = query.Where(x =>
                    x.Username.Contains(s) ||
                    x.Description.Contains(s) ||
                    x.IPAddress.Contains(s));
            }

            System.DateTime? fromDate = null;
            System.DateTime? toDate = null;

            if (request.FromYear.HasValue && request.FromMonth.HasValue && request.FromDay.HasValue)
            {
                fromDate = new System.DateTime(
                    request.FromYear.Value,
                    request.FromMonth.Value,
                    request.FromDay.Value,
                    0,
                    0,
                    0);
            }

            if (request.ToYear.HasValue && request.ToMonth.HasValue)
            {
                var day = request.ToDay ?? System.DateTime.DaysInMonth(request.ToYear.Value, request.ToMonth.Value);

                toDate = new System.DateTime(
                    request.ToYear.Value,
                    request.ToMonth.Value,
                    day,
                    23,
                    59,
                    59);
            }

            if (fromDate.HasValue || toDate.HasValue)
            {
                var fromTimestamp = fromDate?.ToTimeStamp();
                var toTimestamp = toDate?.ToTimeStamp();

                query = query.Where(x =>
                    (!fromTimestamp.HasValue || x.Timestamp >= fromTimestamp.Value) &&
                    (!toTimestamp.HasValue || x.Timestamp <= toTimestamp.Value));
            }

            return query;
        }

        /// <summary>
        /// Asynchronously maps an ActivityLogRaw entity to an ActivityLogDto for use in application logic or
        /// presentation.
        /// </summary>
        /// <remarks>If the source table of the raw log is "AuditLog", the Details property of the
        /// resulting DTO is populated asynchronously using the audit interpreter; otherwise, Details is null. String
        /// fields that are null or whitespace are replaced with a default empty listing value.</remarks>
        /// <param name="raw">The ActivityLogRaw instance containing the raw activity log data to be mapped. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an ActivityLogDto populated with
        /// the mapped data from the specified raw entity.</returns>
        private async Task<ActivityLogDto> MapToDtoAsync(ActivityLogRaw raw)
        {
            return new ActivityLogDto
            {
                Id = raw.Id,
                Timestamp = raw.Timestamp.ToDateTime(),
                TimestampUnix = raw.Timestamp,
                Action = raw.Action,
                Module = raw.Module,
                Status = raw.Status,
                Username = string.IsNullOrWhiteSpace(raw.Username) ? ListingEmpty : raw.Username,
                Email = string.IsNullOrWhiteSpace(raw.Email) ? ListingEmpty : raw.Email,
                IPAddress = string.IsNullOrWhiteSpace(raw.IPAddress) ? ListingEmpty : raw.IPAddress,
                Description = string.IsNullOrWhiteSpace(raw.Description) ? ListingEmpty : raw.Description,
                Details = raw.SourceTable == "AuditLog"
                    ? await this.auditInterpreter.BuildAsync(raw.Action, raw.Changes, raw.EntityName)
                    : null,
                FormattedDate = raw.Timestamp.ToFormattedDateTime(DateTime.ListingFormatDate),
                FormattedTime = raw.Timestamp.ToFormattedDateTime(DateTime.ListingFormatTime),
                HasDetails = raw.HasDetails,
            };
        }

        /// <summary>
        /// Builds a query that retrieves activity log entries relevant to the specified regular user, including login
        /// attempts, audit actions, and password changes.
        /// </summary>
        /// <remarks>The returned query combines data from multiple sources and excludes audit log entries
        /// related to password changes from the general audit log results. Password change events are included as
        /// separate entries. The query is not executed until enumerated.</remarks>
        /// <param name="currentUser">The user for whom to retrieve activity log entries. Must not be null.</param>
        /// <returns>An <see cref="IQueryable{ActivityLogRaw}"/> representing the combined activity logs for the specified user.
        /// The query includes login attempts, audit actions (excluding password changes), and password change events.</returns>
        private IQueryable<ActivityLogRaw> BuildRegularUserQuery(Domain.User currentUser)
        {
            var userIdStr = currentUser.Id.ToString();

            var loginQuery =
                from log in this.dbContext.UserLoginLogs
                where log.UserId == currentUser.Id
                join user in this.dbContext.Users on log.UserId equals user.Id into uj
                from user in uj.DefaultIfEmpty()
                select new ActivityLogRaw
                {
                    Id = log.Id,
                    Timestamp = log.AttemptedAt,
                    Action = log.IsSuccessful ? ActivityLogActions.Login : ActivityLogActions.LoginFailed,
                    Module = ActivityLogModules.Authentication,
                    Status = log.IsSuccessful ? ActivityLogStatuses.Success : ActivityLogStatuses.Failed,
                    Username = user != null ? (user.FirstName + " " + user.LastName) : log.Email,
                    Email = log.Email,
                    IPAddress = log.IPAddress,
                    Description = log.Message,
                    UserAgent = log.UserAgent,
                    EntityName = null,
                    EntityId = null,
                    Changes = null,
                    SourceTable = "UserLoginLog",
                    HasDetails = false,
                };

            var auditQuery =
                from log in this.dbContext.AuditLogs
                where log.UserId == userIdStr &&
                      !(log.EntityName == "User" && log.Changes.Contains("PasswordHash"))
                select new ActivityLogRaw
                {
                    Id = log.Id,
                    Timestamp = log.Timestamp,
                    Action = log.Action,
                    Module = log.EntityName,
                    Status = ActivityLogStatuses.Update,
                    Username = log.Username,
                    Email = log.Username,
                    IPAddress = log.IPAddress ?? ListingEmpty,
                    Description = log.Action + " " + log.EntityName,
                    UserAgent = null,
                    EntityName = log.EntityName,
                    EntityId = log.EntityId,
                    Changes = log.Changes,
                    SourceTable = "AuditLog",
                    HasDetails = log.Changes != null,
                };

            var passwordQuery =
                from log in this.dbContext.AuditLogs
                where log.EntityId == userIdStr &&
                      log.EntityName == "User" &&
                      log.Changes.Contains("PasswordHash")
                select new ActivityLogRaw
                {
                    Id = log.Id,
                    Timestamp = log.Timestamp,
                    Action = ActivityLogActions.PasswordChanged,
                    Module = ActivityLogModules.Security,
                    Status = ActivityLogStatuses.Update,
                    Username = log.Username,
                    Email = log.Username,
                    IPAddress = log.IPAddress ?? ListingEmpty,
                    Description = "Password changed successfully",
                    UserAgent = null,
                    EntityName = "User",
                    EntityId = log.EntityId,
                    Changes = null,
                    SourceTable = "AuditLog",
                    HasDetails = false,
                };

            return loginQuery.Concat(auditQuery).Concat(passwordQuery);
        }

        /// <summary>
        /// Builds a query that retrieves a unified set of activity log entries relevant to administrative user actions,
        /// including logins, audit events, and password changes.
        /// </summary>
        /// <remarks>The returned query can be further filtered or projected before execution. Password
        /// change events are included as distinct entries, and audit log entries related to password changes are
        /// excluded from the general audit log results to avoid duplication.</remarks>
        /// <returns>An <see cref="IQueryable{ActivityLogRaw}"/> representing the combined activity logs for administrative
        /// users. The query includes login attempts, audit log entries (excluding password changes), and password
        /// change events.</returns>
        private IQueryable<ActivityLogRaw> BuildAdminUserQuery()
        {
            var loginQuery =
                from log in this.dbContext.UserLoginLogs
                join user in this.dbContext.Users on log.UserId equals user.Id into uj
                from user in uj.DefaultIfEmpty()
                select new ActivityLogRaw
                {
                    Id = log.Id,
                    Timestamp = log.AttemptedAt,
                    Action = log.IsSuccessful ? ActivityLogActions.Login : ActivityLogActions.LoginFailed,
                    Module = ActivityLogModules.Authentication,
                    Status = log.IsSuccessful ? ActivityLogStatuses.Success : ActivityLogStatuses.Failed,
                    Username = user != null ? (user.FirstName + " " + user.LastName) : log.Email,
                    Email = log.Email,
                    IPAddress = log.IPAddress,
                    Description = log.Message,
                    UserAgent = log.UserAgent,
                    EntityName = null,
                    EntityId = null,
                    Changes = null,
                    SourceTable = "UserLoginLog",
                    HasDetails = false,
                };

            var auditQuery =
                from log in this.dbContext.AuditLogs
                where !(log.EntityName == "User" && log.Changes.Contains("PasswordHash"))
                select new ActivityLogRaw
                {
                    Id = log.Id,
                    Timestamp = log.Timestamp,
                    Action = log.Action,
                    Module = log.EntityName,
                    Status = ActivityLogStatuses.Update,
                    Username = log.Username,
                    Email = log.Username,
                    IPAddress = log.IPAddress ?? ListingEmpty,
                    Description = log.Action + " " + log.EntityName,
                    UserAgent = null,
                    EntityName = log.EntityName,
                    EntityId = log.EntityId,
                    Changes = log.Changes,
                    SourceTable = "AuditLog",
                    HasDetails = log.Changes != null,
                };

            var passwordQuery =
                from log in this.dbContext.AuditLogs
                where log.EntityName == "User" && log.Changes.Contains("PasswordHash")
                select new ActivityLogRaw
                {
                    Id = log.Id,
                    Timestamp = log.Timestamp,
                    Action = ActivityLogActions.PasswordChanged,
                    Module = ActivityLogModules.Security,
                    Status = ActivityLogStatuses.Update,
                    Username = log.Username,
                    Email = log.Username,
                    IPAddress = log.IPAddress ?? ListingEmpty,
                    Description = "Password changed successfully",
                    UserAgent = null,
                    EntityName = "User",
                    EntityId = log.EntityId,
                    Changes = null,
                    SourceTable = "AuditLog",
                    HasDetails = false,
                };

            return loginQuery.Concat(auditQuery).Concat(passwordQuery);
        }
    }
}