namespace Domain
{
    using SharedServices;

    using static SharedServices.Permissions;

    /// <summary>
    /// Model used for assigning or updating permissions for a specific user.
    /// Includes raw permission identifiers and grouped permissions for display.
    /// </summary>
    public class UserPermission_AddEdit
    {
        /// <summary>Gets or sets identifier of the user.</summary>
        public Guid UserId { get; set; }

        /// <summary>Gets or sets list of permission identifiers assigned to the user.</summary>
        public List<string> Permissions { get; set; } = [];

        /// <summary>Gets or sets grouped permissions for UI rendering.</summary>
        public List<ModuleGroup> GroupedPermissions { get; set; } = [];
    }

    /// <summary>
    /// Request model for retrieving user permissions filtered by user or permission identifiers.
    /// </summary>
    public class UserPermissionListRequest : ListRequest
    {
        /// <summary>Gets or sets identifier of the user used for filtering.</summary>
        public Guid? UserId { get; set; }

        /// <summary>Gets or sets collection of permission identifiers used for filtering.</summary>
        public List<string>? Permissions { get; set; }
    }
}