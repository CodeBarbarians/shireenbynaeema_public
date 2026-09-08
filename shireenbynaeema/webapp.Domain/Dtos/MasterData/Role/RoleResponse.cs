namespace Domain
{
    using SharedServices;

    using static SharedServices.Permissions;

    /// <summary>
    /// Listing model representing summarized role information.
    /// Used for displaying roles and their permissions in list or grid views.
    /// </summary>
    public class Role_Listing : Base_Listing
    {
        /// <summary>Gets or sets unique identifier of the role.</summary>
        public Guid Id { get; set; }

        /// <summary>Gets or sets name of the role.</summary>
        public string? Name { get; set; }

        /// <summary>Gets or sets description of the role.</summary>
        public string? Description { get; set; }

        /// <summary>Gets or sets grouped permissions assigned to the role.</summary>
        public List<UserGroupedPermissions> GroupedPermissions { get; set; } = [];

        /// <summary>Gets or sets status of the role.</summary>
        public StatusType Status { get; set; }

        /// <summary>
        /// Gets or sets the scope of the role, indicating whether it's a system role or an organization-specific role.
        /// This property helps differentiate between roles that are globally available across the system and those that are specific to a particular organization.
        /// The scope can be used to determine the context in which the role is applicable and to enforce appropriate access controls based on the role's intended usage.
        /// </summary>
        public string? Scope { get; set; }

        /// <summary>Gets or sets concatenated permission names for display.</summary>
        public string? Permissions { get; set; }

        /// <summary>Gets or sets total number of permissions assigned to the role.</summary>
        public int PermissionCount { get; set; }
    }
}