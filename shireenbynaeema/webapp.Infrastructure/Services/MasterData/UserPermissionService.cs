namespace Services
{
    using Application;
    using Domain;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using SharedServices;

    /// <summary>
    /// Provides services for managing user permissions, including retrieving, updating, and validating both direct and
    /// role-based permissions for users.
    /// </summary>
    /// <remarks>This service supports operations such as listing users with their permissions, retrieving and
    /// updating a user's direct permissions, adding or removing individual permissions, and aggregating all permissions
    /// (including those inherited from roles). It is intended to be used in scenarios where fine-grained permission
    /// management and validation are required for application users.</remarks>
    public class UserPermissionService : IUserPermissionService
    {
        private readonly DatabaseContext dbContext;
        private readonly IResponse response;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserPermissionService"/> class with the specified database context and.
        /// response handler.
        /// </summary>
        /// <param name="dbContext">The database context used to access user and permission data. Cannot be null.</param>
        /// <param name="response">The response handler used to format or manage service responses. Cannot be null.</param>
        public UserPermissionService(DatabaseContext dbContext, IResponse response)
        {
            this.dbContext = dbContext;
            this.response = response;
        }

        /// <summary>
        /// Retrieves a paginated list of users along with their direct permissions based on the provided filters.
        /// This method supports filtering by user ID, search text (matching name, email, or permission),
        /// and specific permissions. The results include the user's display name, email,
        /// a comma-separated list of assigned permissions, and the total permission count.
        /// </summary>
        /// <param name="request">The request object containing pagination and filtering parameters.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IResponse> List(UserPermissionListRequest request)
        {
            IQueryable<User> query = this.dbContext.Users.Include(u => u.UserPermissions);

            if (request.UserId.HasValue)
            {
                query = query.Where(u => u.Id == request.UserId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var search = request.SearchText.Trim();
                query = query.Where(u => u.Email.Contains(search)
                                      || u.FirstName.Contains(search)
                                      || u.LastName.Contains(search)
                                      || u.UserPermissions.Any(up => up.PermissionName.Contains(search)));
            }

            if (request.Permissions != null && request.Permissions.Count != 0)
            {
                query = query.Where(u => u.UserPermissions.Any(up => request.Permissions.Contains(up.PermissionName)));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip(request.Skip)
                .Take(request.Take)
                .Select(u => new UserPermission_Listing
                {
                    UserId = u.Id,
                    UserName = u.DisplayName ?? $"{u.FirstName} {u.LastName}",
                    Email = u.Email,
                    Permissions = string.Join(", ", u.UserPermissions.Select(up => up.PermissionName)),
                    PermissionCount = u.UserPermissions.Count,
                })
                .ToListAsync();

            this.response.Data = items.ToListResponse(request, totalCount);
            this.response.IsSuccess = true;
            this.response.Message = "User permissions retrieved successfully.";
            return this.response;
        }

        /// <summary>
        /// Retrieves the direct permissions assigned to a specific user identified by their unique identifier.
        /// This method includes both a flat list of permissions and a grouped hierarchical structure.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IResponse> Retrieve(Guid userId)
        {
            var user = await this.dbContext.Users
                .Include(u => u.UserPermissions)
                .Where(u => u.Id == userId)
                .Select(u => new UserPermission_AddEdit
                {
                    UserId = u.Id,
                    Permissions = u.UserPermissions.Select(up => up.PermissionName).ToList(),
                    GroupedPermissions = Permissions.GetGroupedForUser(
                        u.UserPermissions.Select(up => up.PermissionName).ToList()),
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return this.response.SetFailure($"User not found with Id {userId}");
            }

            return this.response.SetSuccess("User permissions retrieved successfully.", user.ToRetrieveResponse());
        }

        /// <summary>
        /// Updates the direct permissions of a user by replacing all existing permissions
        /// with a new set provided in the request.
        /// </summary>
        /// <param name="request">The request object containing the user ID and updated permissions.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IResponse> Update(SaveRequest<UserPermission_AddEdit> request)
        {
            var user = await this.dbContext.Users
                .Include(u => u.UserPermissions)
                .FirstOrDefaultAsync(u => u.Id == request.Entity.UserId);

            if (user == null)
            {
                return this.response.SetFailure("User not found.");
            }

            var validPermissions = request.Entity.Permissions
                .Where(p => Permissions.All.Contains(p))
                .ToList();

            user.UserPermissions.Clear();

            foreach (var perm in validPermissions)
            {
                user.UserPermissions.Add(new UserPermission
                {
                    UserId = user.Id,
                    PermissionName = perm,
                });
            }

            await this.dbContext.SaveChangesAsync();
            return this.response.SetSuccess("User permissions updated successfully.");
        }

        /// <summary>
        /// Adds a single permission to a user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="permissionName">The name of the permission to add.</param>
        /// <param name="createdBy">The unique identifier of the user who created this permission assignment.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IResponse> AddPermission(Guid userId, string permissionName, Guid createdBy)
        {
            if (!Permissions.All.Contains(permissionName))
            {
                return this.response.SetFailure($"Invalid permission: {permissionName}");
            }

            if (!await this.dbContext.Users.AnyAsync(u => u.Id == userId))
            {
                return this.response.SetFailure("User not found.");
            }

            if (await this.dbContext.UserPermissions.AnyAsync(up => up.UserId == userId && up.PermissionName == permissionName))
            {
                return this.response.SetFailure("Permission already assigned to user.");
            }

            this.dbContext.UserPermissions.Add(new UserPermission
            {
                UserId = userId,
                PermissionName = permissionName,
            });

            await this.dbContext.SaveChangesAsync();
            return this.response.SetSuccess("Permission added successfully.");
        }

        /// <summary>
        /// Removes a single permission from a user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="permissionName">The name of the permission to remove.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IResponse> RemovePermission(Guid userId, string permissionName)
        {
            var permission = await this.dbContext.UserPermissions
                .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionName == permissionName);

            if (permission == null)
            {
                return this.response.SetFailure("Permission not found for user.");
            }

            this.dbContext.UserPermissions.Remove(permission);
            await this.dbContext.SaveChangesAsync();

            return this.response.SetSuccess("Permission removed successfully.");
        }

        /// <summary>
        /// Retrieves all permissions for a user including inherited role permissions.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IResponse> GetAllPermissions(Guid userId)
        {
            if (!await this.dbContext.Users.AnyAsync(u => u.Id == userId))
            {
                return this.response.SetFailure("User not found.");
            }

            if (await this.dbContext.Users.AnyAsync(u => u.Id == userId && u.IsSuperAdmin))
            {
                return this.response.SetSuccess("User has all permissions (SuperAdmin).", new
                {
                    IsSuperAdmin = true,
                    Permissions = Permissions.All,
                    GroupedPermissions = Permissions.GetAllGrouped(),
                });
            }

            var directPermissions = await this.dbContext.UserPermissions
                .Where(up => up.UserId == userId)
                .Select(up => up.PermissionName)
                .ToListAsync();

            var rolePermissions = await this.dbContext.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(this.dbContext.RolePermissions, ur => ur.RoleId, rp => rp.RoleId, (ur, rp) => rp.PermissionName)
                .ToListAsync();

            var allPermissions = directPermissions.Concat(rolePermissions).Distinct().ToList();

            return this.response.SetSuccess("User permissions retrieved successfully.", new
            {
                AllPermissions = allPermissions,
                GroupedPermissions = Permissions.GetGroupedForUser(allPermissions),
            });
        }

        /// <summary>
        /// Determines whether the specified user has any of the required permissions, either directly or through
        /// assigned roles.
        /// </summary>
        /// <remarks>A user is considered to have a permission if they are a super administrator, have the
        /// permission assigned directly, or belong to an active role that grants the permission.</remarks>
        /// <param name="userId">The unique identifier of the user whose permissions are being checked.</param>
        /// <param name="permissions">An array of permission names to check for the user. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains <see langword="true"/> if the
        /// user has at least one of the required permissions; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> HasPermissionAsync(Guid userId, string[] permissions)
        {
            var isSuperAdmin = await this.dbContext.Users
                .Where(u => u.Id == userId)
                .Select(u => u.IsSuperAdmin)
                .FirstOrDefaultAsync();

            if (isSuperAdmin)
            {
                return true;
            }

            var hasDirectPermission = await this.dbContext.UserPermissions
                .AnyAsync(up =>
                    up.UserId == userId &&
                    permissions.Contains(up.PermissionName));

            if (hasDirectPermission)
            {
                return true;
            }

            return await this.dbContext.UserRoles
                .Where(ur =>
                    ur.UserId == userId &&
                    ur.Role.Status == StatusType.Active)
                .SelectMany(ur => ur.Role.RolePermissions)
                .AnyAsync(rp =>
                    permissions.Contains(rp.PermissionName));
        }

        /// <summary>
        /// Asynchronously retrieves the set of permissions assigned to the specified user, including both direct and
        /// role-based permissions.
        /// </summary>
        /// <remarks>Permissions are aggregated from both direct user assignments and active roles
        /// associated with the user. The result contains unique permission names. This method does not return null; if
        /// the user has no permissions, an empty set is returned.</remarks>
        /// <param name="userId">The unique identifier of the user whose permissions are to be retrieved.</param>
        /// <returns>A set of permission names assigned to the user. If the user is a super administrator, all available
        /// permissions are returned.</returns>
        public async Task<HashSet<string>> GetUserPermissionsAsync(Guid userId)
        {
            if (await this.dbContext.Users.AnyAsync(u => u.Id == userId && u.IsSuperAdmin))
            {
                return Permissions.All.ToHashSet();
            }

            var permissions = await this.dbContext.UserPermissions
                .Where(up => up.UserId == userId)
                .Select(up => up.PermissionName)
                .Union(
                    this.dbContext.UserRoles
                        .Where(ur => ur.UserId == userId && ur.Role.Status == StatusType.Active)
                        .Join(this.dbContext.RolePermissions, ur => ur.RoleId, rp => rp.RoleId, (ur, rp) => rp.PermissionName))
                .ToListAsync();

            return permissions.ToHashSet();
        }

        /// <summary>
        /// Asynchronously retrieves the list of permission names assigned to the specified role.
        /// </summary>
        /// <param name="roleId">The unique identifier of the role for which to retrieve permissions.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of permission names
        /// associated with the specified role. The list is empty if the role has no permissions.</returns>
        public async Task<List<string>> GetRolePermissionsAsync(Guid roleId)
        {
            return await this.dbContext.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.PermissionName)
                .ToListAsync();
        }
    }
}