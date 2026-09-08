namespace SharedServices
{
    /// <summary>
    /// Specifies display metadata for an audited property by indicating the related entity type and the property to use
    /// for display purposes.
    /// </summary>
    /// <remarks>Apply this attribute to a property to associate it with a target entity and a display
    /// property, typically for use in audit logs or UI representations where a human-readable value is preferred over a
    /// raw identifier.</remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public class AuditDisplayAttribute : Attribute
    {
        /// <summary>
        /// Gets the type of the entity that this instance targets.
        /// </summary>
        public Type? TargetEntity { get; }

        /// <summary>
        /// Gets the value to display for this instance.
        /// </summary>
        public string? DisplayProperty { get; }

        /// <summary>
        /// Gets or sets a value indicating whether to use the display name of an enum value instead of its underlying integer value.
        /// </summary>
        public bool UseEnumDisplay { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditDisplayAttribute"/> class.
        /// </summary>
        public AuditDisplayAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditDisplayAttribute"/> class with the specified target entity type and.
        /// display property name.
        /// </summary>
        /// <param name="targetEntity">The type of the entity to which this attribute applies. Cannot be null.</param>
        /// <param name="displayProperty">The name of the property on the target entity to use for display purposes. Cannot be null or empty.</param>
        public AuditDisplayAttribute(Type targetEntity, string displayProperty)
        {
            this.TargetEntity = targetEntity;
            this.DisplayProperty = displayProperty;
        }
    }
}