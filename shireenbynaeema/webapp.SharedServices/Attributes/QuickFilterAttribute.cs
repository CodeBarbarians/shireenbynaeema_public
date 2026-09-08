namespace SharedServices
{
    /// <summary>
    /// Specifies that a property should be included as a quick filter in user interfaces or query builders.
    /// </summary>
    /// <remarks>Apply this attribute to a property to indicate that it can be used for rapid filtering
    /// operations, such as search panels or dynamic query generation. This attribute is typically used by frameworks or
    /// tools that support automatic filter generation based on model metadata.</remarks>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class QuickFilterAttribute : Attribute
    {
    }
}