namespace Server
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    using SharedServices;

    /// <summary>
    /// Middleware that intercepts HTTP responses with 401 (Unauthorized) or 403 (Forbidden) status codes and returns a
    /// standardized JSON response with an appropriate error message and code.
    /// </summary>
    /// <remarks>This middleware should be registered early in the ASP.NET Core request pipeline to ensure
    /// that unauthorized or forbidden responses are handled consistently. It inspects the request for a JWT bearer
    /// token and customizes the error message and code based on token presence and validity. If the response has
    /// already started, the middleware does not modify the response. This middleware is typically used in APIs to
    /// provide clients with clear, machine-readable error information for authentication and authorization
    /// failures.</remarks>
    public class UnauthorizedResponseMiddleware
    {
        private readonly RequestDelegate next;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnauthorizedResponseMiddleware"/> class.
        /// Constructor for UnauthorizedResponseMiddleware.
        /// </summary>
        /// <param name="next">
        /// The next middleware delegate in the HTTP request pipeline.
        /// </param>
        public UnauthorizedResponseMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        /// <summary>
        /// InvokeAsync method to handle unauthorized (401) and forbidden (403) HTTP responses.
        /// It checks if the request contains a JWT token, validates its expiration,
        /// and returns a custom JSON response with an appropriate message and code.
        /// </summary>
        /// <param name="context">
        /// The HttpContext for the current HTTP request, providing access to request and response objects.
        /// </param>
        /// <returns>
        /// A Task representing the asynchronous operation of processing the HTTP request and potentially returning a custom JSON response.
        /// </returns>
        public async Task InvokeAsync(HttpContext context)
        {
            await this.next(context);

            if (context.Response.StatusCode == StatusCodes.Status401Unauthorized ||
                context.Response.StatusCode == StatusCodes.Status403Forbidden)
            {
                // ✅ Check if response has already started (headers sent)
                if (context.Response.HasStarted)
                {
                    return; // Cannot modify response headers or body
                }

                string? code = null;
                string message = context.Response.StatusCode == StatusCodes.Status401Unauthorized
                    ? "Unauthorized"
                    : "Forbidden";

                // 🧠 Try to check if token is expired
                var authHeader = context.GetBearerToken();
                context.Response.Clear();

                if (!string.IsNullOrEmpty(authHeader))
                {
                    var token = authHeader;
                    var jwtHandler = new JwtSecurityTokenHandler();

                    try
                    {
                        if (jwtHandler.CanReadToken(token))
                        {
                            var jwtToken = jwtHandler.ReadJwtToken(token);
                            if (jwtToken.ValidTo < DateTime.UtcNow)
                            {
                                code = "TOKEN_EXPIRED";
                                message = "Session has expired.";
                                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            }
                            else
                            {
                                code = "INSUFFICIENT_PERMISSIONS";

                                var userRoles = jwtToken.Claims
                                    .Where(c => c.Type is "role" or System.Security.Claims.ClaimTypes.Role)
                                    .Select(c => c.Value)
                                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                                var endpoint = context.GetEndpoint();
                                var authorizeAttrs = endpoint?.Metadata
                                    .GetOrderedMetadata<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>();

                                var missingParts = new List<string>();

                                var requiredRoles = authorizeAttrs?
                                    .Where(a => !string.IsNullOrEmpty(a.Roles))
                                    .SelectMany(a => a.Roles!.Split(',', StringSplitOptions.TrimEntries))
                                    .Distinct(StringComparer.OrdinalIgnoreCase)
                                    .ToList();

                                if (requiredRoles is { Count: > 0 })
                                {
                                    var missing = requiredRoles.Where(r => !userRoles.Contains(r)).ToList();
                                    if (missing.Count > 0)
                                    {
                                        missingParts.Add($"missing role(s): {string.Join(", ", missing)}");
                                    }
                                }

                                var requiredPolicies = authorizeAttrs?
                                    .Where(a => !string.IsNullOrEmpty(a.Policy))
                                    .Select(a => a.Policy!)
                                    .Distinct(StringComparer.OrdinalIgnoreCase)
                                    .ToList();

                                if (requiredPolicies is { Count: > 0 })
                                {
                                    // 🔑 Parse "Permission:Data.Trade.Add,Data.Trade.View" → extract permission keys
                                    //    then resolve each key to its PermissionDisplayNameAttribute display name
                                    var displayNames = requiredPolicies
                                        .SelectMany(p => p.StartsWith(
                                            PermissionAttribute.POLICYPREFIX + ":",
                                            StringComparison.OrdinalIgnoreCase)
                                            ? p[(PermissionAttribute.POLICYPREFIX.Length + 1)..]
                                                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                            : [p])
                                        .Select(PermissionDisplayNameAttribute.Resolve)
                                        .Distinct(StringComparer.OrdinalIgnoreCase)
                                        .ToList();

                                    missingParts.Add($"required permission(s): {string.Join(", ", displayNames)}");
                                }

                                message = missingParts.Count > 0
                                    ? $"You do not have permission to access this resource ({string.Join("; ", missingParts)})."
                                    : "You do not have permission to access this resource.";

                                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            }
                        }
                        else
                        {
                            code = "INVALID_TOKEN";
                            message = "Invalid or malformed token.";
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        }
                    }
                    catch
                    {
                        code = "INVALID_TOKEN";
                        message = "Invalid Session.";
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    }
                }
                else
                {
                    code = "NO_TOKEN";
                    message = "Authorization token is missing.";
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                }

                // ⚙️ Reset response and return custom body
                try
                {
                    context.Response.ContentType = "application/json";

                    var response = new Response
                    {
                        IsSuccess = false,
                        Message = message,
                        Code = code,
                        Data = null,
                    };

                    var jsonOptions = new JsonSerializerOptions
                    {
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    };

                    await context.Response.WriteAsJsonAsync(response, jsonOptions);
                }
                catch (InvalidOperationException)
                {
                    // Response has already been started, cannot modify it
                    // The GlobalExceptionHandler will handle this
                }
            }
        }
    }
}