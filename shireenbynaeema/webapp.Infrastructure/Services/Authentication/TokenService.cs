namespace Infrastructure
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;

    using Application;
    using Domain;
    using Microsoft.IdentityModel.Tokens;
    using SharedServices;

    /// <summary>
    /// Provides functionality for generating JSON Web Tokens (JWT) for users and vendors, embedding essential claims
    /// for identification and authorization. Implements the ITokenService interface.
    /// </summary>
    /// <remarks>TokenService supports generating JWTs for both standard users and vendors, with claims
    /// tailored to each scenario. Tokens are signed using HMAC SHA256 and expire according to the intended use case
    /// (typically 1 hour for users, 2 days for vendors). The service relies on configuration values for security and
    /// token metadata. Ensure the configuration contains valid JWT settings before using this service.</remarks>
    /// <param name="config">The application settings configuration used to retrieve JWT secret keys, issuer, and audience information
    /// required for token generation. Cannot be null.</param>
    public class TokenService(IAppSettingsConfig config) : ITokenService
    {
        /// <summary>
        /// Generates a JSON Web Token (JWT) for the specified <see cref="User"/>, including essential claims for identification and authorization.
        /// </summary>
        /// <param name="user">
        /// The <see cref="User"/> object for which the JWT will be generated. Must include user ID, email, display name, organization info, and roles.
        /// </param>
        /// <returns>
        /// A <see cref="string"/> representing the generated JWT token, valid for one hour.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if the provided <paramref name="user"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the JWT key is missing in the configuration.
        /// </exception>
        /// <remarks>
        /// The token includes the following claims:
        /// <list type="bullet">
        /// <item><see cref="ClaimTypes.NameIdentifier"/> - User ID</item>
        /// <item><see cref="ClaimTypes.Name"/> - User email</item>
        /// <item><see cref="ClaimTypes.Email"/> - User email</item>
        /// <item>organizationId - ID of the user's organization</item>
        /// <item>organizationName - Name of the user's organization</item>
        /// <item>displayName - User's display name</item>
        /// <item><see cref="ClaimTypes.Role"/> - User's primary role</item>
        /// </list>
        /// The token is signed using HMAC SHA256 with the secret key from the configuration and is valid for 1 hour.
        /// </remarks>
        public async Task<string> GenerateToken(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User not found.");
            }

            var roles = user.UserRoles?
                .Select(ur => ur.Role.Name)
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Distinct()
                .ToList() ?? [];

            var payload = new Dictionary<string, object>
            {
                { ClaimTypes.NameIdentifier, user.Id.ToString() },
                { ClaimTypes.Name, user.Email },
                { ClaimTypes.Email, user.Email },
                { "displayName", user.DisplayName ?? string.Empty },
                { "scope", ((int?)user.Scope).ToString() ?? string.Empty },
                { "isSuperAdmin", user.IsSuperAdmin },

                // 🔥 Always array
                { ClaimTypes.Role, roles },
            };

            return await Task.FromResult(
                this.BuildToken(payload, DateTimeExtension.UtcNow.AddHours(1)));
        }

        /// <summary>
        /// Generates a JSON Web Token (JWT) for a vendor user, embedding their email and associated work order ID (WOID) as claims.
        /// </summary>
        /// <param name="email">
        /// The vendor's email address to include in the token claims.
        /// </param>
        /// <param name="woid">
        /// The work order ID associated with the vendor. Must not be null; otherwise, an <see cref="Exception"/> is thrown.
        /// </param>
        /// <returns>
        /// A <see cref="string"/> representing the generated JWT token, valid for 2 days.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if <paramref name="woid"/> is null, indicating a required work order ID is missing.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the JWT secret key is missing from the configuration.
        /// </exception>
        /// <remarks>
        /// The token includes the following claims:
        /// <list type="bullet">
        /// <item><see cref="ClaimTypes.Name"/> - Vendor email</item>
        /// <item><see cref="ClaimTypes.Email"/> - Vendor email</item>
        /// <item>WOID - The vendor's work order ID</item>
        /// <item><see cref="ClaimTypes.Role"/> - Fixed as "Vendor"</item>
        /// </list>
        /// The token is signed using HMAC SHA256 with the secret key from the configuration and is set to expire in 2 days.
        /// </remarks>
        public async Task<string> GenerateTokenForVendor(string email, int? woid)
        {
            if (woid == null)
            {
                throw new ArgumentNullException(nameof(woid), "WOID is required for vendor link.");
            }

            var payload = new Dictionary<string, object>
            {
                { ClaimTypes.Name, email },
                { ClaimTypes.Email, email },
                { "WOID", woid.Value.ToString() },

                // 🔥 Always array
                { ClaimTypes.Role, new List<string> { "Vendor" } },
            };

            return await Task.FromResult(
                this.BuildToken(payload, DateTimeExtension.UtcNow.AddDays(2)));
        }

        /// <summary>
        /// Generates a JSON Web Token (JWT) for the specified <see cref="User"/>, including essential claims for identification and authorization.
        /// </summary>
        /// <param name="user">
        /// The <see cref="User"/> object for which the JWT will be generated. Must include user ID, email, display name.
        /// </param>
        /// <returns>
        /// A <see cref="string"/> representing the generated JWT token, valid for one hour.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if the provided <paramref name="user"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the JWT key is missing in the configuration.
        /// </exception>
        /// <remarks>
        /// The token includes the following claims:
        /// <list type="bullet">
        /// <item><see cref="ClaimTypes.NameIdentifier"/> - User ID</item>
        /// <item><see cref="ClaimTypes.Name"/> - User email</item>
        /// <item><see cref="ClaimTypes.Email"/> - User email</item>
        /// <item>displayName - User's display name</item>
        /// </list>
        /// The token is signed using HMAC SHA256 with the secret key from the configuration and is valid for 1 hour.
        /// </remarks>
        public async Task<string> GenerateTokenForAccountAction(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User not found.");
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Email),
                new(ClaimTypes.Email, user.Email),
                new("displayName", user.DisplayName),
            };

            var jwtKey = config.Jwt.Key;

            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                throw new InvalidOperationException("JWT key is missing in configuration");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config.Jwt.Issuer,
                audience: config.Jwt.Audience,
                claims: claims,
                expires: DateTimeExtension.UtcNow.AddHours(1),
                signingCredentials: creds);

            return await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
        }

        /// <summary>
        /// Helper Function to generate token for unified structure.
        /// </summary>
        /// <param name="payload">Dictionary. payload.</param>
        /// <param name="expires">DateTime.</param>
        /// <returns>string.</returns>
        /// <exception cref="InvalidOperationException">JWT key is missing in configuration.</exception>
        private string BuildToken(Dictionary<string, object> payload, DateTime expires)
        {
            var jwtKey = config.Jwt.Key;

            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                throw new InvalidOperationException("JWT key is missing in configuration");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config.Jwt.Issuer,
                audience: config.Jwt.Audience,
                claims: null,
                expires: expires,
                signingCredentials: creds);

            foreach (var item in payload)
            {
                token.Payload[item.Key] = item.Value;
            }

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}