namespace SharedServices
{
    /// <summary>
    /// Custom attribute to specify a display name for an entity. This can be used for logging, error messages, or any scenario where a user-friendly name
    /// is preferred over the technical class name. By applying this attribute to an entity class, you can provide a more descriptive and meaningful name
    /// that can be utilized throughout the application for better readability and user experience.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public sealed class EntityDisplayNameAttribute(string displayName) : Attribute
    {
        /// <summary>
        /// Gets the display name associated with the entity. This value is set through the constructor when the attribute is applied to a class or member.
        /// The display name can be used in various contexts, such as logging, error messages, or user interfaces, to provide a more user-friendly representation of the entity instead of using the technical class name.
        /// </summary>
        public string DisplayName { get; } = displayName;
    }
}