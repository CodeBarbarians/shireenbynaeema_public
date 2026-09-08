namespace Server
{
    using System;
    using System.Text;

    using Infrastructure;

    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Middleware that enforces HTTP Basic Authentication for requests to Swagger UI endpoints.
    /// </summary>
    /// <remarks>Use this middleware to restrict access to the Swagger UI by requiring clients to provide
    /// valid basic authentication credentials. Place this middleware before the Swagger middleware in the application's
    /// request pipeline to ensure that authentication is enforced before serving Swagger UI resources. This middleware
    /// only affects requests with paths starting with "/swagger"; all other requests are passed through without
    /// authentication checks.</remarks>
    public class SwaggerBasicAuthMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IServiceScopeFactory scopeFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerBasicAuthMiddleware"/> class.
        /// with the specified next middleware delegate.
        /// </summary>
        /// <param name="next">
        /// The next middleware delegate in the HTTP request pipeline.
        /// </param>
        /// <param name="scopeFactory">
        /// The database context used for accessing application data, if needed for authentication or logging purposes.
        /// </param>
        public SwaggerBasicAuthMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
        {
            this.next = next;
            this.scopeFactory = scopeFactory;
        }

        /// <summary>
        /// Invokes the SwaggerBasicAuthMiddleware to enforce basic authentication
        /// on requests to the Swagger UI endpoints.
        /// </summary>
        /// <param name="context">
        /// The HttpContext for the current HTTP request, providing access to request and response objects.
        /// </param>
        /// <returns>
        /// A Task representing the asynchronous operation of checking authentication
        /// and either allowing the request to proceed or returning a 401 Unauthorized response.
        /// </returns>
        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/swagger"))
            {
                if (!context.Request.Headers.TryGetValue("Authorization", out var value))
                {
                    await Challenge(context);
                    return;
                }

                var authHeader = value.ToString();
                if (!authHeader.StartsWith("Basic "))
                {
                    await Challenge(context);
                    return;
                }

                var encodedCredentials = authHeader["Basic ".Length..].Trim();
                var decodedBytes = Convert.FromBase64String(encodedCredentials);
                var decodedCredentials = Encoding.UTF8.GetString(decodedBytes).Split(':');

                if (decodedCredentials.Length != 2)
                {
                    await Challenge(context);
                    return;
                }

                var email = decodedCredentials[0];
                var password = decodedCredentials[1];

                // ✅ NEW SCOPE PER REQUEST
                using var scope = this.scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                var user = await dbContext.Users
                    .FirstOrDefaultAsync(x => x.Email == email);

                if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    await Challenge(context);
                    return;
                }
            }

            await this.next(context);
        }

        /// <summary>
        /// Sends a 401 Unauthorized response with a WWW-Authenticate header
        /// to prompt the client for basic authentication credentials.
        /// </summary>
        /// <param name="context">
        /// The HttpContext for the current HTTP request, used to set the response status and headers.
        /// </param>
        /// <returns>
        /// A completed Task representing the synchronous operation of issuing the challenge response.
        /// </returns>
        private static Task Challenge(HttpContext context)
        {
            context.Response.Headers["WWW-Authenticate"] = "Basic";
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        }
    }
}