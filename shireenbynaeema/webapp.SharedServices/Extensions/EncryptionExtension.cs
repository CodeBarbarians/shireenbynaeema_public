namespace SharedServices
{
using System.Security.Cryptography;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.WebUtilities;

/// <summary>
/// Provides extension methods for password hashing, secure token generation, and password complexity validation using
/// cryptographic and security best practices.
/// </summary>
/// <remarks>This static class offers utility methods for common encryption-related operations, such as generating
/// BCrypt password hashes, creating secure random tokens, and validating password complexity. The methods are designed
/// for use in authentication and security-sensitive scenarios, and are compatible with standard cryptographic
/// libraries. All methods are thread-safe and can be used in multi-threaded applications.</remarks>
public static class EncryptionExtension
{
    /// <summary>
    /// Generates a hashed representation of the specified password using the BCrypt algorithm.
    /// </summary>
    /// <remarks>The generated hash includes a salt and is suitable for use in authentication systems.
    /// To verify a password, use the corresponding BCrypt verification method. The hash output format is compatible
    /// with standard BCrypt implementations.</remarks>
    /// <param name="password">The plain text password to be hashed. Cannot be null or empty.</param>
    /// <returns>A string containing the hashed password. The hash can be used for secure password storage and verification.</returns>
    public static string GeneratePasswordHash(this string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    /// <summary>
    /// Generates a secure random token.
    /// </summary>
    /// <returns>string.</returns>
    public static string GenerateSecureToken()
    {
        byte[] randomBytes = RandomNumberGenerator.GetBytes(32);
        return WebEncoders.Base64UrlEncode(randomBytes);
    }

    /// <summary>
    /// Evaluates whether the specified password meets complexity requirements, including minimum length, presence of
    /// lowercase letters, and inclusion of special characters.
    /// </summary>
    /// <remarks>The complexity check requires the password to be at least eight characters long, contain at
    /// least one lowercase letter, and include at least one special character. The method returns the first unmet
    /// requirement as an error message, allowing callers to provide specific feedback to users.</remarks>
    /// <param name="password">The password string to validate. Cannot be null, empty, or consist solely of whitespace.</param>
    /// <returns>A tuple containing a Boolean value indicating whether the password is complex, and an error message describing
    /// the first unmet requirement. If the password is complex, the error message is empty.</returns>
    public static (bool IsComplex, string ErrorMessage) CheckPasswordComplexity(this string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return (false, Constants.PasswordNullError);
        }

        if (password.Length < 8)
        {
            return (false, Constants.PasswordNotLongEnough);
        }

        if (!Regex.IsMatch(password, "[a-z]"))
        {
            return (false, Constants.PasswordNotWithLowercaseCharacter);
        }

        if (!Regex.IsMatch(password, "[^a-zA-Z0-9]"))
        {
            return (false, Constants.PasswordNotWithSpecialCharacter);
        }

        return (true, string.Empty);
    }
}
}