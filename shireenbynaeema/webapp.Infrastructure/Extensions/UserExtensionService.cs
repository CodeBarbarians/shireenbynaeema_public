namespace Infrastructure
{
    using System.Net;

    using Application;
    using Domain;
    using Microsoft.EntityFrameworkCore;
    using SharedServices;

    /// <summary>
    /// Provides user-related extension operations such as email notifications, user activation, bulk invitations, and
    /// registration workflows. This service acts as an orchestrator for user onboarding, invite token validation, and
    /// status management, integrating with email, notification, and database services.
    /// </summary>
    /// <remarks>UserExtensionService coordinates multiple aspects of user management, including bulk
    /// operations and transactional workflows. It ensures consistency and validation across user onboarding,
    /// activation, and invitation processes. Thread safety is not guaranteed; concurrent operations should be managed
    /// externally if required.</remarks>
    /// <param name="response">The response object used to encapsulate operation results and messages for user extension actions.</param>
    /// <param name="emailService">The email service used to send notifications and invitations to users and administrators.</param>
    /// <param name="notificationSettingService">The notification setting service used to manage and configure user notification preferences during registration.</param>
    /// <param name="userInviteTokenService">The service responsible for generating, validating, and managing user invite tokens for onboarding and
    /// registration.</param>
    /// <param name="dbContext">The database context used for accessing and modifying user, role, organization, and related entities.</param>
    /// <param name="config">The application settings configuration providing URLs, email addresses, and other settings required for user
    /// extension operations.</param>
    public class UserExtensionService(
        IResponse response,
        IEmailService emailService,
        INotificationSettingService notificationSettingService,
        IUserInviteTokenService userInviteTokenService,
        DatabaseContext dbContext,
        IAppSettingsConfig config) : IUserExtensionService
    {
        /// <summary>
        /// Sends an email containing a one-time password (OTP) to the specified email address if a matching user
        /// exists.
        /// </summary>
        /// <remarks>If the specified email address does not correspond to an existing user, the method
        /// returns a failure response. The email content and subject are predefined and cannot be customized through
        /// this method.</remarks>
        /// <param name="email">The email address of the user to whom the OTP email will be sent. Cannot be null or empty.</param>
        /// <returns>A response object indicating the result of the email sending operation. The response will indicate failure
        /// if no user with the specified email address is found.</returns>
        public async Task<IResponse> SendEmail(string email)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == email).ConfigureAwait(false);
            if (user == null)
            {
                response.IsSuccess = Constants.ResponseFailure;
                response.Message = Constants.NotFound.FormatWith(typeof(User).GetEntityDisplayName());
                return response;
            }

            var resp = await emailService.SendEmail(user.Email, "OTP", "Test", "<strong>HELLO WORLD<strong/>").ConfigureAwait(false);
            return resp;
        }

        /// <summary>
        /// Validates the provided user invite token to ensure it is still valid and corresponds to the specified email address.
        /// </summary>
        /// <param name="request">The request object containing the invite token and associated email.</param>
        /// <returns>
        /// An <see cref="IResponse"/> indicating whether the token is valid:
        /// - Success if the token is valid and associated with the email.
        /// - Failure if the token is invalid, expired, or does not match the email.
        /// </returns>
        public async Task<IResponse> VerifyUserInviteToken(TokenVerificationRequest request)
        {
            return await userInviteTokenService.ValidateUserInviteTokenAsync(request.Token, request.Email).ConfigureAwait(false);
        }

        /// <summary>
        /// Activates or deactivates a user based on the provided request.
        /// Updates the user's status flag in the database to reflect the desired active or inactive state.
        /// </summary>
        /// <param name="request">The request object containing the user ID(s) and the desired status flag.</param>
        /// <returns>
        /// An <see cref="IResponse"/> indicating the result of the operation:
        /// - Success if the user's status was successfully updated.
        /// - Failure if the specified user was not found.
        /// </returns>
        public async Task<IResponse> ActiveInActiveUser(ActiveInActiveUserRequest request)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == request.Users.FirstOrDefault()).ConfigureAwait(false);
            if (user == null)
            {
                response.IsSuccess = false;
                response.Message = Constants.NotFound.FormatWith(typeof(User).GetEntityDisplayName());
                return response;
            }

            user.Status = request.Flag;

            // user.IsActive = request.Flag;
            dbContext.Update(user);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);

            response.IsSuccess = true;
            response.Message = Constants.UpdateSuccess.FormatWith("User Status");
            return response;
        }

        /// <summary>
        /// Activates or deactivates multiple users in bulk based on the provided request.
        /// Updates the status flag for each specified user and commits the changes within a database transaction.
        /// Ensures that all updates succeed together or are rolled back in case of an error.
        /// </summary>
        /// <param name="request">The request object containing the list of user IDs and the desired status flag.</param>
        /// <returns>
        /// An <see cref="IResponse"/> indicating the result of the bulk operation:
        /// - Success if all users were updated successfully.
        /// - Failure if an exception occurred, in which case the transaction is rolled back.
        /// </returns>
        public async Task<IResponse> BulkActiveInActiveUser(ActiveInActiveUserRequest request)
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);

            try
            {
                var userIds = request.Users.ToList();

                var resp = await dbContext.Users
                    .Where(u => userIds.Contains(u.Id)).ToListAsync().ConfigureAwait(false);

                foreach (var user in resp)
                {
                    user.Status = request.Flag;
                }

                dbContext.UpdateRange(resp);

                await dbContext.SaveChangesAsync().ConfigureAwait(false);
                await transaction.CommitAsync().ConfigureAwait(false);

                response.IsSuccess = Constants.ResponseSuccess;
                response.Message = Constants.UpdateSuccess.FormatWith(typeof(User).GetEntityDisplayName());
                return response;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                response.IsSuccess = Constants.ResponseFailure;
                response.Message = ex.Message;
                return response;
            }
        }

        /// <summary>
        /// Invites multiple users in bulk by sending invitation emails and assigning roles as specified in the request.
        /// </summary>
        /// <remarks>This method validates user emails, checks for duplicates, verifies role assignments,
        /// and enforces scope and organization constraints based on the current user's permissions. If any validation
        /// fails, the operation is aborted and an appropriate failure response is returned. When resending invitations,
        /// only users who are not already active will receive a new invitation. The method requires that the invitation
        /// URL is configured; otherwise, the operation will fail.</remarks>
        /// <param name="request">The bulk user invite request containing the list of users to invite, their roles, and invitation options.
        /// Cannot be null and must include at least one user.</param>
        /// <returns>A response indicating the result of the bulk invitation operation. The response includes details about which
        /// invitations succeeded or failed.</returns>
        public async Task<IResponse> BulkInviteUsers(BulkUserInviteRequest request)
        {
            var isSuperAdmin = CurrentUser.IsSuperAdmin;
            var currentUserOrgId = CurrentUser.OrganizationId;

            if (request.Users == null || request.Users.Count == 0)
            {
                return response.SetFailure("No users provided.");
            }

            request.Users.ForEach(u => u.Email = u.Email.ToLowerInvariant().Trim());
            var emails = request.Users.Select(x => x.Email).ToList();

            var invalidEmails = emails.Where(x => !x.IsValidEmail()).ToList();
            if (invalidEmails.Count != 0)
            {
                return response.SetFailure(Constants.Email.InValidEmail
                    .FormatWith(string.Join(", ", invalidEmails)));
            }

            var duplicateEmails = emails
                .GroupBy(x => x)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateEmails.Count != 0)
            {
                return response.SetFailure(Constants.Email.DuplicateEmail
                    .FormatWith(string.Join(", ", duplicateEmails)));
            }

            var now = DateTimeExtension.UtcNowUnixTimestamp;

            Dictionary<Guid, Role> rolesDict = [];

            if (!request.IsResendInvite)
            {
                var roleIds = request.Users
                    .SelectMany(x => x.Roles)
                    .Distinct()
                    .ToList();

                rolesDict = await dbContext.Roles
                    .Where(r => roleIds.Contains(r.Id))
                    .ToDictionaryAsync(r => r.Id).ConfigureAwait(false);
            }

            if (!request.IsResendInvite)
            {
                foreach (var entry in request.Users)
                {
                    var validRoleIds = entry.Roles
                        .Where(r => rolesDict.ContainsKey(r))
                        .ToList();

                    if (validRoleIds.Count == 0)
                    {
                        return response.SetFailure($"Invalid roles provided for {entry.Email}");
                    }

                    var roles = validRoleIds.Select(r => rolesDict[r]).ToList();

                    bool hasOrganization =
                        entry.OrganizationId.HasValue &&
                        entry.OrganizationId != Guid.Empty;

                    foreach (var role in roles)
                    {
                        if (role.Scope == RoleScope.Organization && !hasOrganization)
                        {
                            return response.SetFailure(
                                $"Organization is required when assigning Organization scoped role ({entry.Email}).");
                        }

                        if (role.Scope == RoleScope.Customer && hasOrganization)
                        {
                            return response.SetFailure(
                                $"Organization should not be provided when assigning Facility scoped role ({entry.Email}).");
                        }
                    }
                }
            }

            var existingUsers = await dbContext.Users
                .Where(u => emails.Contains(u.Email))
                .ToDictionaryAsync(u => u.Email).ConfigureAwait(false);

            var orgIds = request.Users
                .Where(x => x.OrganizationId.HasValue && x.OrganizationId != Guid.Empty)
                .Select(x => x.OrganizationId!.Value)
                .Distinct()
                .ToList();

            var usersToAdd = new List<User>();
            var usersToUpdate = new List<User>();
            var inviteLogs = new List<UserInviteLog>();

            var inviteUrlBase = config.UserInvite.Url;

            if (inviteUrlBase == null)
            {
                return response.SetFailure("Invitation URL is not configured.");
            }

            using var transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);

            foreach (var entry in request.Users)
            {
                existingUsers.TryGetValue(entry.Email, out var user);

                List<Guid> validRoleIds = [];
                List<Role> validRoles = [];

                if (!request.IsResendInvite)
                {
                    validRoleIds = entry.Roles
                        .Where(r => rolesDict.ContainsKey(r))
                        .ToList();

                    validRoles = validRoleIds.Select(r => rolesDict[r]).ToList();
                }

                if (request.IsResendInvite)
                {
                    if (user == null)
                    {
                        inviteLogs.Add(CreateFailedLog(Guid.Empty, entry, now));
                        continue;
                    }

                    if (user.Status == (int)StatusType.Active)
                    {
                        inviteLogs.Add(CreateFailedLog(user.Id, entry, now));
                        continue;
                    }

                    user.InvitationDate = now;
                    usersToUpdate.Add(user);

                    validRoleIds = await dbContext.UserRoles
                        .Where(x => x.UserId == user.Id)
                        .Select(x => x.RoleId)
                        .ToListAsync().ConfigureAwait(false);

                    validRoles = await dbContext.Roles
                        .Where(r => validRoleIds.Contains(r.Id))
                        .ToListAsync().ConfigureAwait(false);
                }

                if (!request.IsResendInvite)
                {
                    if (user == null)
                    {
                        var userId = Guid.NewGuid();

                        user = new User
                        {
                            Id = userId,
                            Email = entry.Email,
                            Status = (int)StatusType.Pending,
                            InvitationDate = now,
                            InvitedBy = CurrentUser.UserId,
                            Scope = validRoles.FirstOrDefault()?.Scope == RoleScope.Customer
                                ? UserScope.Customer
                                : UserScope.Organization,
                            UserRoles = validRoleIds.Select(roleId => new UserRole
                            {
                                UserId = userId,
                                RoleId = roleId,
                            }).ToList(),
                        };

                        usersToAdd.Add(user);
                        existingUsers[entry.Email] = user;
                    }
                    else
                    {
                        if (user.Status == (int)StatusType.Active)
                        {
                            inviteLogs.Add(CreateFailedLog(user.Id, entry, now));
                            continue;
                        }

                        user.Status = (int)StatusType.Pending;
                        user.InvitationDate = now;

                        var existingRoles = await dbContext.UserRoles
                            .Where(x => x.UserId == user.Id)
                            .ToListAsync().ConfigureAwait(false);

                        dbContext.UserRoles.RemoveRange(existingRoles);

                        var newRoles = validRoleIds.Select(roleId => new UserRole
                        {
                            UserId = user.Id,
                            RoleId = roleId,
                        });

                        await dbContext.UserRoles.AddRangeAsync(newRoles).ConfigureAwait(false);

                        usersToUpdate.Add(user);
                    }
                }

                var token = await userInviteTokenService
                    .SaveUserInviteTokenAsync(user!.Id).ConfigureAwait(false);

                var inviteUrl = inviteUrlBase.FormatWith(
                    token.Token,
                    WebUtility.UrlEncode(entry.Email));

                var roleName = validRoles.FirstOrDefault()?.Name ?? string.Empty;

                var subject = Constants.BulkInviteUsers
                    .InvitedToJoin.FormatWith(string.Empty);

                var emailResp = await emailService.SendEmail(
                    entry.Email,
                    subject,
                    string.Empty,
                    string.Empty).ConfigureAwait(false);

                inviteLogs.Add(new UserInviteLog
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    RoleId = validRoleIds.FirstOrDefault(),
                    SentOn = now,
                    IsSuccess = emailResp?.IsSuccess == true ? 1 : 0,
                    InvitedBy = CurrentUser.UserId,
                });
            }

            if (usersToAdd.Count != 0)
            {
                await dbContext.Users.AddRangeAsync(usersToAdd).ConfigureAwait(false);
            }

            if (usersToUpdate.Count != 0)
            {
                dbContext.Users.UpdateRange(usersToUpdate);
            }

            await dbContext.SaveChangesAsync().ConfigureAwait(false);

            await dbContext.UserInviteLogs.AddRangeAsync(inviteLogs).ConfigureAwait(false);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);

            await transaction.CommitAsync().ConfigureAwait(false);

            return this.BuildBulkResponse(inviteLogs);
        }

        /// <summary>
        /// Registers a new user with the specified registration details.
        /// </summary>
        /// <remarks>The method validates the email address and ensures that the user does not already
        /// exist. All specified roles must be valid and present in the system. If registration is successful, the user
        /// is created, assigned roles, and default notification settings are added. The operation is performed within a
        /// transaction to ensure consistency.</remarks>
        /// <param name="request">An object containing the user's registration information, including email, password, personal details,
        /// organization, and assigned roles. Cannot be null. The email must be valid, and all specified role IDs must
        /// exist.</param>
        /// <returns>A response indicating the result of the registration operation. The response includes a success flag and a
        /// message describing the outcome.</returns>
        public async Task<IResponse> RegisterUser(UserRegisterRequest request)
        {
            var response = new Response();

            if (!request.Email.IsValidEmail())
            {
                response.IsSuccess = Constants.ResponseFailure;
                response.Message = Constants.Email.InValidEmail.FormatWith(request.Email);
                return response;
            }

            if (dbContext.Users.Any(u => u.Email == request.Email))
            {
                response.IsSuccess = Constants.ResponseFailure;
                response.Message = Constants.AlreadyExists.FormatWith("User");
                return response;
            }

            using (var transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                try
                {
                    // Create user
                    var user = new User
                    {
                        Email = request.Email,
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        DisplayName = $@"{request.FirstName} {request.LastName}",
                        PasswordHash = request.Password.GeneratePasswordHash(),
                        PhoneNo = request.PhoneNo,
                        Status = (int)StatusType.Active,
                    };

                    // Assign roles
                    var roles = await dbContext.Roles
                        .Where(r => request.Roles.Contains(r.Id))
                        .ToListAsync().ConfigureAwait(false);

                    if (roles.Count != request.Roles.Count)
                    {
                        response.IsSuccess = Constants.ResponseFailure;
                        response.Message = Constants.VerifiedFailed.FormatWith("User Roles");
                        return response;
                    }

                    foreach (var role in roles)
                    {
                        user.UserRoles.Add(new UserRole { User = user, Role = role });
                    }

                    // Add user to DbContext
                    await dbContext.Users.AddAsync(user).ConfigureAwait(false);
                    await dbContext.SaveChangesAsync().ConfigureAwait(false); // Save to get UserId

                    // Create customer profile
                    var customer = new Customer
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        Phone = request.PhoneNo ?? string.Empty,
                        CreatedOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    };
                    await dbContext.Customers.AddAsync(customer).ConfigureAwait(false);
                    await dbContext.SaveChangesAsync().ConfigureAwait(false);

                    // Create default cart
                    var cart = new Cart
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        CreatedOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    };
                    await dbContext.Carts.AddAsync(cart).ConfigureAwait(false);
                    await dbContext.SaveChangesAsync().ConfigureAwait(false);

                    // Add notification settings
                    await notificationSettingService.AddNotificationSetting(user.Id).ConfigureAwait(false);

                    // Commit transaction
                    await transaction.CommitAsync().ConfigureAwait(false);

                    // Send welcome email
                    _ = SendWelcomeEmail(user.Email, user.GetDisplayName());

                    response.IsSuccess = Constants.ResponseSuccess;
                    response.Message = Constants.RegisterSuccess.FormatWith("User");
                    return response;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);
                    response.IsSuccess = Constants.ResponseFailure;
                    response.Message = $"Error registering user: {ex.Message}";
                    return response;
                }
            }
        }

        /// <summary>
        /// Completes the registration process for a user with a pending status using the provided registration details.
        /// </summary>
        /// <remarks>This method finalizes user registration by updating the user's status to active and
        /// marking any associated invite tokens as used. The operation is performed within a database transaction to
        /// ensure consistency. If the user is not found or is not in a pending state, the response indicates
        /// failure.</remarks>
        /// <param name="request">The registration details for the user, including email, name, phone number, and password. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a response indicating whether
        /// the registration was completed successfully.</returns>
        public async Task<IResponse> CompleteUserRegistration(UserRegisterRequest request)
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email && u.Status == (int)StatusType.Pending).ConfigureAwait(false);
                if (user == null)
                {
                    response.IsSuccess = Constants.ResponseFailure;
                    response.Message = Constants.NotFound.FormatWith("User");
                    return response;
                }

                user.Email = request.Email;
                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.PhoneNo = request.PhoneNo;
                user.DisplayName = $@"{request.FirstName} {request.LastName}";
                user.PasswordHash = request.Password.GeneratePasswordHash();
                user.ApprovalDate = DateTimeExtension.UtcNowUnixTimestamp;
                user.Status = (int)StatusType.Active;

                dbContext.Users.Update(user);
                await dbContext.SaveChangesAsync().ConfigureAwait(false);

                var resp = await dbContext.UserInviteTokens
                                      .Where(x => x.UserId == user.Id && x.IsUsed == false)
                                      .ExecuteUpdateAsync(u => u
                                          .SetProperty(u => u.IsUsed, u => true)).ConfigureAwait(false);

                // var sysAdmins = await dbContext.Users
                //    .Include(u => u.UserRoles)
                //    .ThenInclude(ur => ur.Role)
                //    .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "SuperAdmin"))
                //    .ToListAsync();
                // await SendEmailToSystemAdministrator(sysAdmins, CurrentUser.DisplayName, user.Organization.Name, user.Email, "");
                await transaction.CommitAsync().ConfigureAwait(false);
            }
            catch
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                throw;
            }

            response.IsSuccess = Constants.ResponseSuccess;
            response.Message = Constants.RegisterSuccess.FormatWith("User");
            return response;
        }

        /// <summary>
        /// Creates a new user invite log entry representing a failed invitation attempt.
        /// </summary>
        /// <param name="userId">The unique identifier of the user being invited. If null, the log will use an empty GUID.</param>
        /// <param name="entry">The invite entry containing details about the invitation, including the organization identifier.</param>
        /// <param name="now">The timestamp, in ticks, indicating when the invitation was sent.</param>
        /// <returns>A UserInviteLog instance populated with failure status and relevant invitation details.</returns>
        private static UserInviteLog CreateFailedLog(Guid? userId, UserInviteEntry entry, long now)
        {
            return new UserInviteLog
            {
                Id = Guid.NewGuid(),
                UserId = userId ?? Guid.Empty,
                SentOn = now,
                IsSuccess = 0,
                InvitedBy = CurrentUser.UserId,
            };
        }

        /// <summary>
        /// Builds a bulk user invitation response based on the results of multiple user invite operations.
        /// </summary>
        /// <remarks>The response indicates overall success if at least one invite was successful. The
        /// status message reflects whether any invites failed. Ensure that the list of logs accurately represents the
        /// invite outcomes to obtain correct aggregation.</remarks>
        /// <param name="logs">A list of user invitation log entries representing the outcome of each user invite attempt. Each entry
        /// should indicate whether the invite was successful or failed.</param>
        /// <returns>An object containing the aggregated results of the bulk user invitation operation, including counts of
        /// successful and failed invites, and a status message.</returns>
        private IResponse BuildBulkResponse(List<UserInviteLog> logs)
        {
            var success = logs.Count(x => x.IsSuccess == 1);
            var failed = logs.Count(x => x.IsSuccess == 0);

            response.Data = new BulkUserInviteResponse
            {
                Invited = logs.Count,
                Success = success,
                Failed = failed,
            };

            response.IsSuccess = success > 0;
            response.Message = failed > 0
                ? Constants.BulkUserInviteFailure
                : Constants.BulkUserInviteSuccess;

            return response;
        }

        private async Task SendWelcomeEmail(string email, string userName)
        {
            try
            {
                var templatePath = Path.Combine(AppContext.BaseDirectory, "Content", "WelcomeEmailTemplate.html");
                if (!File.Exists(templatePath)) return;
                var template = await File.ReadAllTextAsync(templatePath).ConfigureAwait(false);
                template = template.Replace("{1}", userName);
                template = template.Replace("{2}", "https://shireenbynaeema.com");

                await emailService.SendEmail(
                    email,
                    "Welcome to Shireen by Naeema",
                    $"Welcome {userName}! Thank you for joining Shireen by Naeema.",
                    template,
                    "Notifications").ConfigureAwait(false);
            }
            catch { }
        }
    }
}