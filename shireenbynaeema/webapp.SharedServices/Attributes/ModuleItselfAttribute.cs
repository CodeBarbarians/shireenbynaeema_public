namespace SharedServices
{
    /// <summary>
    /// Indicates that a class represents the module itself within a modular application framework.
    /// </summary>
    /// <remarks>Apply this attribute to a class to designate it as the primary module definition. This is
    /// typically used to identify the entry point or main module in modular architectures. The attribute is not
    /// inherited and should be used only on classes.</remarks>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class ModuleItselfAttribute : Attribute
    {
    }
}