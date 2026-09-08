namespace Infrastructure
{
    using Application;
    using Domain;
    using Microsoft.EntityFrameworkCore;
    using SharedServices;

    using static SharedServices.Permissions;

    /// <summary>
    /// Provides role management operations, including creating, updating, deleting, retrieving, and listing roles, as
    /// well as managing role permissions and bulk activation or deactivation of roles.
    /// </summary>
    /// <remarks>The RoleService class encapsulates business logic for handling roles and their associated
    /// permissions within the system. It supports validation to prevent duplicate roles, dynamic filtering and
    /// pagination for role listings, and ensures data consistency when modifying roles or their permissions. Bulk
    /// operations are performed within database transactions to guarantee atomicity. Seeded roles are protected from
    /// being deactivated to maintain system integrity. All methods return standardized response objects indicating the
    /// outcome of each operation.</remarks>
    public class RoleService : Service<Role>, IRoleService
    {
        private readonly DatabaseContext dbContext;
        private readonly IResponse response;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoleService"/> class with the specified repository, response handler, and.
        /// database context.
        /// </summary>
        /// <param name="repository">The repository used to access and manage Role entities.</param>
        /// <param name="response">The response handler used to format or manage service responses.</param>
        /// <param name="dbContext">The database context used for direct data access operations related to roles.</param>
        public RoleService(IRepository<Role> repository, IResponse response, DatabaseContext dbContext)
            : base(repository, response)
        {
            this.dbContext = dbContext;
            this.response = response;
        }

        /// <summary>
        /// Adds a new role to the system after validating that no role with the same name already exists.
        /// This method creates the role entity, assigns the selected permissions,
        /// and persists it to the database.
        /// </summary>
        /// <param name="request">
        /// The request object containing the role details, including name, description, status,
        /// and the list of permissions to assign to the role.
        /// </param>
        /// <returns>
        /// An <see cref="IResponse"/> indicating success or failure of the operation.
        /// On success, the new role is saved to the database with its assigned permissions.
        /// If a role with the same name already exists, a failure response is returned.
        /// </returns>
        public async Task<IResponse> Add(SaveRequest<Role_AddEdit> request)
        {
            if (await this.dbContext.Roles.AnyAsync(r => r.Name == request.Entity.Name))
            {
                return this.response.SetFailure($"Role '{request.Entity.Name}' already exists.");
            }

            var role = new Role
            {
                Id = request.Entity.Id ?? Guid.NewGuid(),
                Name = request.Entity.Name,
                Scope = request.Entity.Scope,
                Status = request.Entity.Status,
                Description = request.Entity.Description,
            };

            var permissionIds = request.Entity.RolePermissions ?? [];
            var permissions = All.Where(p => permissionIds.Contains(p)).ToList();

            foreach (var perm in permissions)
            {
                role.RolePermissions.Add(new RolePermission { Role = role, PermissionName = perm });
            }

            await this.dbContext.Roles.AddAsync(role);
            await this.dbContext.SaveChangesAsync();

            return this.response.SetSuccess("Role added successfully.");
        }

        /// <summary>
        /// Retrieves a paginated list of roles from the database, with optional filtering
        /// by role name and assigned permissions. This method includes role permissions
        /// and supports dynamic pagination and search functionality.
        /// </summary>
        /// <param name="request">
        /// The request object containing pagination parameters (Skip, Take),
        /// optional search text for role names or permissions, and a list of
        /// specific permission filters to apply.
        /// </param>
        /// <returns>
        /// An <see cref="IResponse"/> containing the paginated list of roles,
        /// including their descriptions, status, and grouped permissions.
        /// The response also includes the total count of matching records
        /// and operation status information.
        /// </returns>
        public async Task<IResponse> List(RoleListRequest request)
        {
            IQueryable<Role> query = this.dbContext.Roles.Include(r => r.RolePermissions);

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var search = request.SearchText.Trim().ToLowerInvariant();
                query = query.Where(r => r.Name.Contains(search));
            }

            if (request.Permissions != null && request.Permissions.Count != 0)
            {
                query = query.Where(r => r.RolePermissions.Any(rp => request.Permissions.Contains(rp.PermissionName)));
            }

            if (request.Status != null)
            {
                query = query.Where(r => r.Status == request.Status);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip(request.Skip)
                .Take(request.Take)
                .Select(r => new Role_Listing
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    Status = r.Status,
                    Scope = r.Scope != null ? ((RoleScope)r.Scope).GetDisplayName() : string.Empty,
                    GroupedPermissions = Flatten(
                        GetGroupedForUser(
                            r.RolePermissions.Select(rp => rp.PermissionName).ToList())),
                })
                .ToListAsync();

            this.response.Data = items.ToListResponse(request, totalCount);
            this.response.IsSuccess = true;
            this.response.Message = "Roles retrieved successfully.";
            return this.response;
        }

        /// <summary>
        /// Updates an existing role's details and assigned permissions in the database.
        /// This method retrieves the role by its unique identifier, updates its name,
        /// description, and status, clears existing permissions, and applies the
        /// new set of permissions provided in the request.
        /// </summary>
        /// <param name="request">
        /// The request object containing the role ID, updated role details,
        /// and the list of permissions to assign to the role.
        /// </param>
        /// <returns>
        /// An <see cref="IResponse"/> indicating success or failure of the operation.
        /// On success, the role's details and permissions are updated in the database.
        /// If the role is not found, a failure response is returned.
        /// </returns>
        public async Task<IResponse> Update(SaveRequest<Role_AddEdit> request)
        {
            var role = await this.dbContext.Roles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Id == request.EntityId);

            if (role == null)
            {
                return this.response.SetFailure("Role not found.");
            }

            role.Name = request.Entity.Name;
            role.Description = request.Entity.Description;
            role.Status = request.Entity.Status;
            role.Scope = request.Entity.Scope;
            role.RolePermissions.Clear();

            var permissionIds = request.Entity.RolePermissions ?? [];
            var permissions = All.Where(p => permissionIds.Contains(p)).ToList();

            foreach (var perm in permissions)
            {
                role.RolePermissions.Add(new RolePermission { Role = role, PermissionName = perm });
            }

            await this.dbContext.SaveChangesAsync();
            return this.response.SetSuccess("Role updated successfully.");
        }

        /// <summary>
        /// Deletes a role from the database based on its unique identifier.
        /// This method also removes all associated role permissions to maintain data consistency.
        /// </summary>
        /// <param name="id">
        /// The unique identifier of the role to delete.
        /// </param>
        /// <returns>
        /// An <see cref="IResponse"/> indicating success or failure of the deletion operation.
        /// On success, the role and its associated permissions are removed from the database.
        /// If the role is not found, a failure response is returned.
        /// </returns>
        public async Task<IResponse> Delete(Guid id)
        {
            var role = await this.dbContext.Roles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
            {
                return this.response.SetFailure("Role not found.");
            }

            role.SoftDelete();
            await this.dbContext.SaveChangesAsync();

            return this.response.SetSuccess("Role deleted successfully.");
        }

        /// <summary>
        /// Retrieves the details of a single role from the database based on its unique identifier.
        /// This method includes the role's basic properties as well as its assigned permissions,
        /// grouped for display purposes using the hierarchical permission structure.
        /// </summary>
        /// <param name="id">
        /// The unique identifier of the role to retrieve.
        /// </param>
        /// <returns>
        /// An <see cref="IResponse"/> containing the role details and its associated permissions.
        /// On success, the response includes the mapped role data. If the role is not found,
        /// a failure response is returned indicating that no role exists with the specified ID.
        /// </returns>
        public async Task<IResponse> Retrieve(Guid id)
        {
            var role = await this.dbContext.Roles
                .Include(r => r.RolePermissions)
                .Where(r => r.Id == id)
                .Select(r => new Role_AddEdit
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    Status = r.Status,
                    Scope = r.Scope,
                    RolePermissions = r.RolePermissions.Select(rp => rp.PermissionName).ToList(),
                    GroupedPermissions = Flatten(
                    GetGroupedForUser(
                        r.RolePermissions.Select(rp => rp.PermissionName).ToList())),
                })
                .FirstOrDefaultAsync();

            if (role == null)
            {
                return this.response.SetFailure($"Role not found with Id {id}");
            }

            return this.response.SetSuccess("Role retrieved successfully", role.ToRetrieveResponse());
        }

        /// <summary>
        /// Retrieves all available permissions in the system using the hierarchical structure.
        /// The permissions are grouped into Module Groups → Modules → Permission Items
        /// for easier display and selection in the user interface.
        /// </summary>
        /// <returns>
        /// An <see cref="IResponse"/> containing the grouped permission structure.
        /// </returns>
        public async Task<IResponse> GetAllPermissions()
        {
            var grouped = Flatten(GetAllGrouped());

            return this.response.SetSuccess("All permissions retrieved successfully.", new
            {
                GroupedPermissions = grouped,
            });
        }

        /// <summary>
        /// Activates or deactivates multiple roles in bulk based on the provided request.
        /// Updates the status flag for each specified role and commits the changes within a database transaction.
        /// Ensures that all updates succeed together or are rolled back in case of an error.
        /// except for the seeded roles which are protected from being made inactive. The method returns a response indicating the success or failure of the operation.
        /// </summary>
        /// <param name="request">The request object containing the list of role IDs and the desired status flag.</param>
        /// <returns>
        /// An <see cref="IResponse"/> indicating the result of the bulk operation:
        /// - Success if all roles were updated successfully.
        /// - Failure if an exception occurred, in which case the transaction is rolled back.
        /// </returns>
        public async Task<IResponse> BulkActiveInActive(ActiveInActiveRequest request)
        {
            await using var transaction = await this.dbContext.Database.BeginTransactionAsync();

            try
            {
                var userIds = request.Ids.ToList();

                var resp = await this.dbContext.Roles
                    .Where(u => userIds.Contains(u.Id)).ToListAsync();

                var seedRoleIds = new List<Guid>() { Constants.Seed.SuperAdminRoleId };

                resp = resp.Where(u => !seedRoleIds.Contains(u.Id)).ToList();

                if (resp.Count == 0)
                {
                    this.response.IsSuccess = Constants.ResponseFailure;
                    this.response.Message = "Seeded roles cannot be made inactive.";
                    return this.response;
                }

                foreach (var user in resp)
                {
                    user.Status = request.Flag;
                }

                this.dbContext.UpdateRange(resp);

                await this.dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                this.response.IsSuccess = Constants.ResponseSuccess;
                this.response.Message = Constants.UpdateSuccess.FormatWith(typeof(Role).GetEntityDisplayName());
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
        /// Flattens a collection of module groups into a list of user grouped permissions, combining all modules and
        /// their associated permissions into a single list.
        /// </summary>
        /// <param name="groups">The list of module groups to flatten. Cannot be null.</param>
        /// <returns>A list of user grouped permissions representing all modules and their permissions from the specified groups.
        /// The list will be empty if no groups or modules are provided.</returns>
        private static List<UserGroupedPermissions> Flatten(List<ModuleGroup> groups)
        {
            return groups
                .SelectMany(g => g.Modules.Select(m => new UserGroupedPermissions
                {
                    Module = m.Name,
                    Permissions = m.Permissions,
                }))
                .ToList();
        }
    }
}