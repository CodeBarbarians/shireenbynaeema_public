namespace SharedServices
{
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Extension methods for HttpContext, providing additional functionality for working with HTTP requests and responses. This class includes methods that can be used to
    /// simplify common tasks such as retrieving headers, query parameters, and user information from the HttpContext.
    /// </summary>
    public static class HttpContextExtension
    {
        /// <summary>
        /// Retrieves the bearer token from the Authorization header of the HTTP request. This method checks if the Authorization header is present and starts with "Bearer ",
        /// and if so, extracts and returns the token value.
        /// </summary>
        /// <param name="client">HttpContent.</param>
        /// <returns>Token.</returns>
        public static string? GetBearerToken(this HttpContext client)
        {
            var authorizationHeader = client.Request.Headers["Authorization"].ToString();
            return !string.IsNullOrEmpty(authorizationHeader) &&
                   authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? authorizationHeader["Bearer ".Length..].Trim()
                : null;
        }
    }
}