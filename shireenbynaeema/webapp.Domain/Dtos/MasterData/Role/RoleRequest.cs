namespace Domain
{
    using SharedServices;

    using static SharedServices.Permissions;

    /// <summary>
    /// Model used for creating or updating role information.
    /// Includes role details and associated permissions.
    /// </summary>
    public class Role_AddEdit
    {
        /// <summary>Gets or sets unique identifier of the role.</summary>
        public Guid? Id { get; set; }

        /// <summary>Gets or sets name of the role.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Gets or sets description of the role.</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Gets or sets status of the role.</summary>
        public StatusType Status { get; set; }

        /// <summary>
        /// Gets or sets the scope associated with the role.
        /// </summary>
        /// <remarks>Use this property to specify or retrieve the scope in which the role applies. The
        /// value may be null if no scope is defined.</remarks>
        public RoleScope? Scope { get; set; }

        /// <summary>Gets or sets list of permission identifiers assigned to the role.</summary>
        public ICollection<string>? RolePermissions { get; set; }

        /// <summary>Gets or sets grouped permissions for display purposes.</summary>
        public List<UserGroupedPermissions> GroupedPermissions { get; set; } = [];
    }

    /// <summary>
    /// Request model for retrieving roles filtered by permissions.
    /// </summary>
    public class RoleListRequest : ListRequest
    {
        /// <summary>Gets or sets list of permission identifiers used for filtering.</summary>
        public List<string>? Permissions { get; set; }

        /// <summary>
        /// Gets or sets the status code associated with the current operation.
        /// </summary>
        public StatusType? Status { get; set; }
    }

    /// <summary>
    /// Request model for retrieving Role lookup data filtered by Scope.
    /// </summary>
    public class RoleLookupListRequest : LookupListRequest
    {
        /// <summary>Gets or sets a value indicating whether identifier of the organization used for filtering.</summary>
        public bool IsFacilityUser { get; set; }
    }

    /// <summary>
    /// Request model for retrieving Role lookup data filtered by Scope.
    /// </summary>
    public class UserEditRoleLookupListRequest : LookupListRequest
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user.
        /// </summary>
        public Guid UserId { get; set; }
    }
}