namespace SharedServices
{
    /// <summary>
    /// Specifies the ordering of elements for sorting or processing purposes when applied to a code element.
    /// </summary>
    /// <remarks>Use this attribute to indicate the preferred order for items such as properties, fields, or methods.
    /// The attribute can be applied to any code element, but its effect depends on how it is interpreted by consuming
    /// frameworks or tools. This attribute does not enforce ordering by itself; it serves as metadata for consumers that
    /// support ordered processing.</remarks>
    [AttributeUsage(AttributeTargets.All)]
    public class OrderByAttribute : Attribute
    {
    }
}