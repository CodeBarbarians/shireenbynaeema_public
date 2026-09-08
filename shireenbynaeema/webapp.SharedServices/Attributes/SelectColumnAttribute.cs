namespace SharedServices
{
/// <summary>
/// Custom attribute used to mark properties or fields that should be included in select queries or projections.
/// </summary>
[AttributeUsage(AttributeTargets.All)]
public class SelectColumnAttribute : Attribute
{
}
}