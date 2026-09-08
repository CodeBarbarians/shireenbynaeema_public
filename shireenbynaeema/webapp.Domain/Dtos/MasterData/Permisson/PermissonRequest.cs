namespace Domain
{
    using SharedServices;

    /// <summary>
    /// Model used for creating or updating permission information.
    /// </summary>
    public class Permission_AddEdit
    {
        /// <summary>Gets or sets unique identifier of the permission.</summary>
        public Guid? Id { get; set; }

        /// <summary>Gets or sets name of the permission.</summary>
        public string? Name { get; set; }

        /// <summary>Gets or sets description of the permission.</summary>
        public string? Description { get; set; }
    }

    /// <summary>
    /// Request model for retrieving permissions filtered by specific identifiers.
    /// </summary>
    public class PermissionListRequest : ListRequest
    {
        /// <summary>Gets or sets collection of permission identifiers used for filtering.</summary>
        public List<Guid>? PermissionIds { get; set; }
    }
}