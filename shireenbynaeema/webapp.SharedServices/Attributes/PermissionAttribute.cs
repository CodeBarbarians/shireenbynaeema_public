namespace SharedServices
{
using Microsoft.AspNetCore.Authorization;

/// <summary>
/// Custom authorization attribute for enforcing permission-based access control on API endpoints. This attribute allows developers to specify required
/// permissions for accessing specific controller actions or entire controllers by providing a list of permissions as parameters. The attribute constructs
/// a policy name based on the provided permissions, which can then be evaluated by the authorization system to determine if the user has the necessary
/// permissions to access the resource. By using this attribute, developers can implement fine-grained access control in their applications, ensuring
/// that only users with the appropriate permissions can perform certain actions or access specific resources.
/// </summary>
public class PermissionAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Specifies the prefix used for permission policy names.
    /// </summary>
    public const string POLICYPREFIX = "Permission";

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionAttribute"/> class with the specified permissions required for access.
    /// </summary>
    /// <remarks>Use this constructor to specify one or more permissions that must be granted for the
    /// associated member. The permissions are combined into a policy string used for authorization checks.</remarks>
    /// <param name="permissions">A list of permission names that are required. Each permission must be a non-empty string.</param>
    public PermissionAttribute(params string[] permissions)
    {
        this.Policy = $"{POLICYPREFIX}:{string.Join(",", permissions)}";
        this.Permissions = permissions;
    }

    /// <summary>
    /// Gets the list of permission names assigned to the current entity.
    /// </summary>
    public string[] Permissions { get; }
}
}