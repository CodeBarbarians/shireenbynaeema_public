namespace Infrastructure
{
    using Application;
    using Domain;
    using Microsoft.EntityFrameworkCore;
    using SharedServices;

    using User = Domain.User;

    /// <summary>
    /// Provides user management operations such as creating, retrieving, updating, and listing users, including role
    /// assignments and notification settings. This service enforces business rules related to user scope, organization,
    /// and role integrity.
    /// </summary>
    /// <remarks>The UserService coordinates user-related operations, ensuring that users are created and
    /// updated with valid roles and organizational associations. It applies authorization checks based on the current
    /// user's scope and enforces constraints such as unique email addresses and consistent role scopes. All operations
    /// return an IResponse indicating the outcome and, where applicable, include relevant user data. This service is
    /// intended to be used within the application's business logic layer and is not thread-safe.</remarks>
    public class UserService : Service<User>, IUserService
    {
        private readonly DatabaseContext dbContext;

        private readonly IResponse response;
        private readonly INotificationSettingService notificationSettingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class with the specified repository, response handler,.
        /// database context, and notification setting service.
        /// </summary>
        /// <param name="repository">The repository used for accessing and managing User entities.</param>
        /// <param name="response">The response handler used to format or manage service responses.</param>
        /// <param name="dbContext">The database context used for data access operations related to users.</param>
        /// <param name="notificationSettingService">The service used to manage user notification settings.</param>
        public UserService(IRepository<User> repository, IResponse response, DatabaseContext dbContext, INotificationSettingService notificationSettingService)
            : base(repository, response)
        {
            this.response = response;
            this.dbContext = dbContext;
            this.notificationSettingService = notificationSettingService;
        }

        /// <summary>
        /// Adds a new user to the system after validating that the email does not already exist.
        /// This method also assigns any roles specified in the request and sets up default
        /// notification settings for the new user.
        /// </summary>
        /// <param name="request">
        /// A <see cref="SaveRequest{User_AddEdit}"/> containing the details of the user to be created,
        /// including name, email, status, and an optional list of role IDs to assign.
        /// </param>
        /// <returns>
        /// An <see cref="IResponse"/> indicating the result of the operation:
        /// <list type="bullet">
        ///   <item>IsSuccess: Indicates whether the user was added successfully.</item>
        ///   <item>Message: Contains a success message or reason for failure.</item>
        /// </list>
        /// If the email already exists, the response indicates that the user cannot be added.
        /// </returns>
        public async Task<IResponse> AddUser(SaveRequest<User_AddEdit> request)
        {
            await using var transaction = await this.dbContext.Database.BeginTransactionAsync();

            try
            {
                var isSuperAdmin = CurrentUser.IsSuperAdmin;
                var currentUserOrgId = CurrentUser.OrganizationId;

                if (request?.Entity == null)
                {
                    return this.response.SetFailure("Invalid request.");
                }

                var normalizedEmail = request.Entity.Email?.Trim();

                if (normalizedEmail == null)
                {
                    return this.response.SetFailure("User requires a valid email.");
                }

                if (await this.dbContext.Users.AnyAsync(u => u.Email == normalizedEmail))
                {
                    return this.response.SetFailure("Email already exists.");
                }

                var requestedRoleIds = request.Entity.UserRoles?
                    .Where(x => x != Guid.Empty)
                    .Distinct()
                    .ToList() ?? [];

                if (requestedRoleIds.Count == 0)
                {
                    return this.response.SetFailure("At least one role must be assigned.");
                }

                var roles = await this.dbContext.Roles
                    .Where(r => requestedRoleIds.Contains(r.Id))
                    .ToDictionaryAsync(r => r.Id);

                if (roles.Count != requestedRoleIds.Count)
                {
                    return this.response.SetFailure("Invalid role(s) provided.");
                }

                // 🔒 Prevent conflicting scopes
                var distinctScopes = roles.Values
                    .Select(r => r.Scope)
                    .Distinct()
                    .ToList();

                if (distinctScopes.Count > 1)
                {
                    return this.response.SetFailure("Cannot assign roles with different scopes together.");
                }

                bool hasOrganization =
                    request.Entity.OrganizationId.HasValue &&
                    request.Entity.OrganizationId != Guid.Empty;

                foreach (var role in roles.Values)
                {
                    // Role ↔ Organization integrity
                    if (role.Scope == RoleScope.Organization && !hasOrganization)
                    {
                        return this.response.SetFailure("Organization is required for Organization scoped role.");
                    }

                    if (role.Scope == RoleScope.Customer && hasOrganization)
                    {
                        return this.response.SetFailure("Organization must not be provided for Facility scoped role.");
                    }
                }

                var entity = new User(request.Entity)
                {
                    Email = normalizedEmail,
                    DisplayName = request.Entity.GetDisplayName(),
                    Scope = distinctScopes.First().HasValue ? (UserScope)distinctScopes.First()!.Value : null,
                };

                foreach (var roleId in requestedRoleIds)
                {
                    entity.UserRoles.Add(new UserRole
                    {
                        User = entity,
                        RoleId = roleId,
                    });
                }

                await this.dbContext.Users.AddAsync(entity);
                await this.dbContext.SaveChangesAsync();

                await this.notificationSettingService.AddNotificationSetting(entity.Id);

                await transaction.CommitAsync();

                this.response.IsSuccess = Constants.ResponseSuccess;
                this.response.Message = Constants.SaveSuccess.FormatWith(this.ModuleName);

                return this.response;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                this.response.IsSuccess = Constants.ResponseFailure;
                this.response.Message = ex.Message;
                return this.response;
            }
        }

        /// <summary>
        /// Retrieves a paginated list of users based on the filters provided in the <see cref="UserListRequest"/>.
        /// The method supports filtering by organization, role, invitation source, status, and search text
        /// (matching name, email, or phone number). It also applies sorting and pagination according to the request.
        /// </summary>
        /// <param name="request">
        /// A <see cref="UserListRequest"/> object containing filtering, sorting, and pagination parameters,
        /// such as search text, organization IDs, role IDs, invited-by IDs, status, skip, and take values.
        /// </param>
        /// <returns>
        /// An <see cref="IResponse"/> containing:
        /// <list type="bullet">
        ///   <item>IsSuccess: Indicates whether the operation succeeded.</item>
        ///   <item>Message: A message describing the result of the operation.</item>
        ///   <item>Data: A paginated list of <see cref="User_Listing"/> entries matching the filters.</item>
        /// </list>
        /// </returns>
        public async Task<IResponse> ListUsers(UserListRequest request)
        {
            var filterAndFormat =
                    from user in this.dbContext.Users
                    join inviter in this.dbContext.Users on user.InvitedBy equals inviter.Id into inviterGroup
                    from invitedBy in inviterGroup.DefaultIfEmpty()
                    select new
                    {
                        user,
                        InvitedBy = invitedBy,
                        UserRoles = (
                            from ur in this.dbContext.UserRoles
                            join r in this.dbContext.Roles on ur.RoleId equals r.Id
                            where ur.UserId == user.Id
                            select r.Name)
                        .Distinct(),
                    };

            if (request.UserRole != null && request.UserRole.Count != 0)
            {
                var userRoleIds = request.UserRole;
                filterAndFormat = filterAndFormat.Where(x =>
                    this.dbContext.UserRoles.Any(ur => ur.UserId == x.user.Id && userRoleIds.Contains(ur.RoleId)));
            }

            if (request.InvitedBy != null && request.InvitedBy.Count != 0)
            {
                filterAndFormat = filterAndFormat.Where(x =>
                    x.user.InvitedBy.HasValue && request.InvitedBy.Contains(x.user.InvitedBy.Value));
            }

            if (request.Status != null)
            {
                filterAndFormat = filterAndFormat.Where(x => x.user.Status == request.Status);
            }

            if (request.Scope != null)
            {
                filterAndFormat = filterAndFormat.Where(x => x.user.Scope == request.Scope);
            }

            // 🔍 Apply search filter (on name, email, phone)
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var normalizedSearch = request.SearchText
                    .Replace("(", string.Empty)
                    .Replace(")", string.Empty)
                    .Replace("-", string.Empty)
                    .Replace(" ", string.Empty);

                filterAndFormat = filterAndFormat.Where(x =>
                    x.user.DisplayName.Contains(request.SearchText) ||
                    x.user.Email.Contains(request.SearchText) ||
                    (
                        x.user.PhoneNo != null &&
                        x.user.PhoneNo
                            .Replace("(", string.Empty)
                            .Replace(")", string.Empty)
                            .Replace("-", string.Empty)
                            .Replace(" ", string.Empty) == normalizedSearch));
            }

            if (CurrentUser.IsSuperAdmin)
            {
                filterAndFormat = filterAndFormat.Where(x => x.user.Id != Constants.Seed.AdminUserId);
            }
            else
            {
                filterAndFormat = filterAndFormat.Where(x => !x.user.IsSuperAdmin);
            }

            // Project to view model
            var projectedQuery = filterAndFormat.Select(x => new User_Listing
            {
                Id = x.user.Id,
                FirstName = x.user.FirstName ?? Constants.ListingEmpty,
                LastName = x.user.LastName ?? Constants.ListingEmpty,
                Email = x.user.Email,
                DisplayName = (string.IsNullOrWhiteSpace(x.user.DisplayName) || string.IsNullOrEmpty(x.user.DisplayName)) ? Constants.ListingEmpty : x.user.DisplayName,
                Status = ((StatusType)x.user.Status).ToString(),
                PhoneNo = x.user.PhoneNo,
                InvitedBy = x.InvitedBy != null ? x.InvitedBy.DisplayName : null,
                InvitationDate = x.user.InvitationDate.HasValue
                    ? x.user.InvitationDate.Value.ToFormattedDateTime(Constants.DateTime.ListingFormatDate)
                    : Constants.ListingEmpty,
                InvitationDateUnix = x.user.InvitationDate,
                ApprovalDate = x.user.ApprovalDate.HasValue
                    ? x.user.ApprovalDate.Value.ToFormattedDateTime(Constants.DateTime.ListingFormatDate)
                    : Constants.ListingEmpty,
                ApprovalDateUnix = x.user.ApprovalDate,
                InvitationTime = x.user.InvitationDate.HasValue
                    ? x.user.InvitationDate.Value.ToFormattedDateTime(Constants.DateTime.ListingFormatTime)
                    : Constants.ListingEmpty,
                ApprovalTime = x.user.ApprovalDate.HasValue
                    ? x.user.ApprovalDate.Value.ToFormattedDateTime(Constants.DateTime.ListingFormatTime)
                    : Constants.ListingEmpty,
                UserRoles = string.Join(", ", x.UserRoles),
                CreatedOn = x.user.CreatedOn.ToFormattedDateTime(Constants.DateTime.ListingFormat),
                CreatedOnDate = x.user.CreatedOn.ToFormattedDateTime(Constants.DateTime.ListingFormatDate),
                CreatedOnTime = x.user.CreatedOn.ToFormattedDateTime(Constants.DateTime.ListingFormatTime),
                CreatedOnUnix = x.user.CreatedOn,
            });

            // 🔀 Apply ordering: prioritize matched column when search is active
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var search = request.SearchText;

                projectedQuery = projectedQuery
                    .OrderBy(x =>
                        (!string.IsNullOrEmpty(x.DisplayName) && x.DisplayName.Contains(search)) ? 0 :
                        (!string.IsNullOrEmpty(x.Email) && x.Email.Contains(search)) ? 1 :
                        (!string.IsNullOrEmpty(x.PhoneNo) && x.PhoneNo.Contains(search)) ? 2 :
                        3)
                    .ThenBy(x => x.DisplayName);
            }
            else
            {
                projectedQuery = projectedQuery.ApplyOrdering(request);
            }

            var pagedQuery = projectedQuery.Skip(request.Skip).Take(request.Take);

            var totalCount = await projectedQuery.CountAsync();
            var items = await pagedQuery.ToListAsync();

            this.response.Data = items.ToListResponse(request, totalCount);
            this.response.IsSuccess = Constants.ResponseSuccess;
            this.response.Message = Constants.RetrieveSuccess.FormatWith(this.ModuleName);

            return this.response;
        }

        /// <summary>
        /// Retrieves detailed information for a specific user identified by their unique identifier.
        /// This includes basic user details such as name, email, phone number, status, associated organization,
        /// and the roles assigned to the user.
        /// </summary>
        /// <param name="id">The unique identifier of the user to retrieve.</param>
        /// <returns>
        /// An <see cref="IResponse"/> containing:
        /// <list type="bullet">
        ///   <item>IsSuccess: Indicates whether the operation succeeded.</item>
        ///   <item>Message: A message describing the result of the retrieval.</item>
        ///   <item>Data: A <see cref="User_AddEdit"/> object containing the user's details and role IDs.</item>
        /// </list>
        /// </returns>
        public async Task<IResponse> RetrieveUser(Guid id)
        {
            var user = await (from usr in this.dbContext.Users
                              join ur in this.dbContext.UserRoles on usr.Id equals ur.UserId into userRoleGroup
                              from userRole in userRoleGroup.DefaultIfEmpty()
                              join r in this.dbContext.Roles on userRole.RoleId equals r.Id into roleGroup
                              from role in roleGroup.DefaultIfEmpty()
                              join inviter in this.dbContext.Users on usr.InvitedBy equals inviter.Id into inviterGroup
                              from invitedBy in inviterGroup.DefaultIfEmpty()
                              join updater in this.dbContext.Users on usr.UpdatedBy equals updater.Id into updaterGroup
                              from updatedBy in updaterGroup.DefaultIfEmpty()
                              join creator in this.dbContext.Users on usr.CreatedBy equals creator.Id into creatorGroup
                              from createdBy in creatorGroup.DefaultIfEmpty()
                              where usr.Id == id
                              select new User_AddEdit()
                              {
                                  Id = usr.Id,
                                  FirstName = usr.FirstName,
                                  LastName = usr.LastName,
                                  Email = usr.Email,
                                  Status = usr.Status,
                                  PhoneNo = usr.PhoneNo,
                                  UserRoles = (ICollection<Guid>)usr.UserRoles.Select(x => x.Role.Id),
                                  UserRoleNames = (ICollection<string>)usr.UserRoles.Select(x => x.Role.Name),
                                  StatusName = ((StatusType)usr.Status).ToString(),
                                  Scope = usr.Scope,
                                  UpdatedBy = updatedBy.DisplayName ?? createdBy.DisplayName,
                                  UpdatedOn = usr.UpdatedOn.ToDateTime() ?? usr.CreatedOn.ToDateTime(),
                              }).FirstOrDefaultAsync();
            if (user == null)
            {
                this.response.IsSuccess = Constants.ResponseFailure;
                this.response.Message = Constants.RetrieveFailed.FormatWith(this.ModuleName);
                return this.response;
            }

            var lastlogin = this.dbContext.UserLoginLogs
                                      .Where(x => x.UserId == id && x.IsSuccessful == true)
                                      .OrderByDescending(x => x.AttemptedAt)
                                      .Select(x => x.AttemptedAt);
            if (lastlogin.Any())
            {
                user.LastLogin = lastlogin.FirstOrDefault().ToDateTime();
            }

            var resp = user.ToRetrieveResponse();
            this.response.Data = resp;
            this.response.IsSuccess = Constants.ResponseSuccess;
            this.response.Message = Constants.RetrieveSuccess.FormatWith(this.ModuleName);
            return this.response;
        }

        /// <summary>
        /// Updates an existing user's details and their associated roles based on the provided request.
        /// This method performs the following:
        /// <list type="bullet">
        ///   <item>Updates basic user information such as first name, last name, email, phone number, and organization.</item>
        ///   <item>Updates the user's display name based on the provided information.</item>
        ///   <item>Synchronizes the user's roles by adding new roles and removing roles that are no longer assigned.</item>
        ///   <item>Performs the update within a transaction to ensure data integrity.</item>
        /// </list>
        /// </summary>
        /// <param name="request">A <see cref="SaveRequest{User_AddEdit}"/> containing the user's updated information and role assignments.</param>
        /// <returns>
        /// An <see cref="IResponse"/> indicating the result of the update operation:
        /// <list type="bullet">
        ///   <item>IsSuccess: true if the update was successful; false otherwise.</item>
        ///   <item>Message: A message describing the result of the operation.</item>
        ///   <item>Data: Optional additional information if applicable.</item>
        /// </list>
        /// </returns>
        public async Task<IResponse> UpdateUser(SaveRequest<User_AddEdit> request)
        {
            await using var transaction = await this.dbContext.Database.BeginTransactionAsync();

            try
            {
                var isSuperAdmin = CurrentUser.IsSuperAdmin;
                var currentUserOrgId = CurrentUser.OrganizationId;
                var entity = await this.dbContext.Users
                    .Include(x => x.UserRoles)
                    .FirstOrDefaultAsync(x => x.Id == request.EntityId);

                if (entity == null)
                {
                    return this.response.SetFailure("User not found.");
                }

                var normalizedEmail = request.Entity.Email?.Trim();

                if (request.Entity.FirstName == null || request.Entity.LastName == null || normalizedEmail == null)
                {
                    return this.response.SetFailure("User needs a valid, FirstName, LastName and an Email.");
                }

                var requestedRoleIds = request.Entity.UserRoles?
                    .Where(x => x != Guid.Empty)
                    .Distinct()
                    .ToList() ?? [];

                if (requestedRoleIds.Count == 0)
                {
                    return this.response.SetFailure("At least one role must be assigned.");
                }

                var roles = await this.dbContext.Roles
                    .Where(r => requestedRoleIds.Contains(r.Id))
                    .ToDictionaryAsync(r => r.Id);

                if (roles.Count != requestedRoleIds.Count)
                {
                    return this.response.SetFailure("Invalid role(s) provided.");
                }

                // 🔒 Prevent conflicting scopes
                var distinctScopes = roles.Values
                    .Select(r => r.Scope)
                    .Distinct()
                    .ToList();

                if (distinctScopes.Count > 1)
                {
                    return this.response.SetFailure("Cannot assign roles with different scopes together.");
                }

                bool hasOrganization =
                    request.Entity.OrganizationId.HasValue &&
                    request.Entity.OrganizationId != Guid.Empty;

                foreach (var role in roles.Values)
                {
                    if (role.Scope == RoleScope.Organization && !hasOrganization)
                    {
                        return this.response.SetFailure("Organization is required for Organization scoped role.");
                    }

                    if (role.Scope == RoleScope.Customer && hasOrganization)
                    {
                        return this.response.SetFailure("Organization must not be provided for Facility scoped role.");
                    }
                }

                // Update core fields
                entity.FirstName = request.Entity.FirstName;
                entity.LastName = request.Entity.LastName;
                entity.PhoneNo = request.Entity.PhoneNo;
                entity.Email = normalizedEmail;
                entity.DisplayName = request.Entity.GetDisplayName();
                entity.Scope = distinctScopes.First().HasValue ? (UserScope)((int)distinctScopes.First()!) : null; // derive from role

                // Update roles
                var existingRoleIds = entity.UserRoles.Select(x => x.RoleId).ToList();

                var rolesToAdd = requestedRoleIds.Except(existingRoleIds).ToList();
                foreach (var roleId in rolesToAdd)
                {
                    entity.UserRoles.Add(new UserRole
                    {
                        UserId = entity.Id,
                        RoleId = roleId,
                    });
                }

                var rolesToRemove = entity.UserRoles
                    .Where(x => !requestedRoleIds.Contains(x.RoleId))
                    .ToList();

                if (rolesToRemove.Count != 0)
                {
                    this.dbContext.UserRoles.RemoveRange(rolesToRemove);
                }

                await this.dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                this.response.IsSuccess = Constants.ResponseSuccess;
                this.response.Message = Constants.UpdateSuccess.FormatWith(this.ModuleName);

                return this.response;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                this.response.IsSuccess = Constants.ResponseFailure;
                this.response.Message = ex.Message;
                return this.response;
            }
        }
    }
}