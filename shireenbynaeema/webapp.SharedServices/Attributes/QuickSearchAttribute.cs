namespace SharedServices
{
    /// <summary>
    /// Indicates that a property should be included in quick search operations within a user interface or data querying
    /// context.
    /// </summary>
    /// <remarks>Apply this attribute to properties that are intended to be searchable using quick or
    /// keyword-based search features. This attribute is typically used by frameworks or tools to identify searchable
    /// fields and does not affect runtime behavior unless explicitly handled by the consuming application.</remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public class QuickSearchAttribute : Attribute
    {
    }
}