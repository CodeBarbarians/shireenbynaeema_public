namespace Application
{
using Domain;

/// <summary>
/// Defines methods for generating JSON Web Tokens (JWT) for users and vendors to support authentication and
/// authorization scenarios.
/// </summary>
/// <remarks>Implementations of this interface are responsible for creating JWT tokens that encapsulate user or
/// vendor identity and claims. These tokens can be used to authenticate requests and control access to protected
/// resources. The interface provides overloads for generating tokens for general users, vendors (optionally scoped to a
/// work order), and for account-related actions such as password resets or user invitations.</remarks>
public interface ITokenService
{
    /// <summary>
    /// Generates a JWT token for the specified user. This method takes a User object as input and creates a JWT token that includes the user's information and claims.
    /// The generated token can be used for authenticating the user in subsequent requests to the server,
    /// allowing them to access protected resources based on their roles and permissions.
    /// The method returns the generated token as a string.
    /// </summary>
    /// <param name="user">User.</param>
    /// <returns>Token.</returns>
    Task<string> GenerateToken(User user);

    /// <summary>
    /// Generates a JWT token for a vendor based on their email and an optional Work Order ID (WOID). This method is designed to create a token that can be used by vendors to access specific resources or perform certain actions related to a work order. The token may include claims that identify the vendor and the associated work order, allowing for secure and controlled access to the relevant data and functionalities.
    /// The method returns the generated token as a string.
    /// </summary>
    /// <param name="email">Email.</param>
    /// <param name="woid">WOID.</param>
    /// <returns>Token.</returns>
    Task<string> GenerateTokenForVendor(string email, int? woid);

    /// <summary>
    /// Generates a JWT token for the specified user. This method takes a User object as input and creates a JWT token that includes the user's information and claims.
    /// The generated token can be used for authenticating the user in subsequent requests to the server,
    /// for apis like passwordReset and UserInvite that doesnt require any permissions but required for mantining proper audit log
    /// The method returns the generated token as a string.
    /// </summary>
    /// <param name="user">Email.</param>
    /// <returns>Token.</returns>
    Task<string> GenerateTokenForAccountAction(User user);
}
}