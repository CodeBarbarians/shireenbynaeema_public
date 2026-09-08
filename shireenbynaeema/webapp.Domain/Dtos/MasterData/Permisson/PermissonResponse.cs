namespace Domain
{
    /// <summary>
    /// Listing model representing summarized permission information.
    /// Used for displaying permissions in list or grid views.
    /// </summary>
    public class Permission_Listing
    {
        /// <summary>Gets or sets unique identifier of the permission.</summary>
        public Guid Id { get; set; }

        /// <summary>Gets or sets name of the permission.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Gets or sets description of the permission.</summary>
        public string? Description { get; set; }

        /// <summary>Gets or sets creation date formatted as string.</summary>
        public string CreatedOn { get; set; } = string.Empty;
    }

    /// <summary>
    /// Lookup model representing basic permission information.
    /// Used for selection lists and reference mappings.
    /// </summary>
    public class Permission_Lookup
    {
        /// <summary>Gets or sets unique identifier of the permission.</summary>
        public Guid Id { get; set; }

        /// <summary>Gets or sets name of the permission.</summary>
        public string Name { get; set; } = string.Empty;
    }
}