namespace Server
{
    using Hangfire;

    /// <summary>
    /// Extension methods for configuring the middleware pipeline of the web application. This class contains a method that sets up various middleware components,
    /// including authentication, authorization, exception handling, CORS, routing, and Swagger documentation. It also configures the Hangfire dashboard for
    /// background job management. The method is designed to be called during the application startup process to ensure that all necessary middleware is properly
    /// configured and integrated into the request processing pipeline. By centralizing the middleware configuration in this class, we can maintain a clean and
    /// organized startup process while ensuring that all required middleware components are included and correctly set up for the application's needs.
    /// </summary>
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// Configures the middleware pipeline for the web application. This method sets up various middleware components, including authentication, authorization,
        /// exception handling, CORS, routing, and Swagger documentation. It also configures the Hangfire dashboard for background job management.
        /// </summary>
        /// <param name="app">app.</param>
        /// <returns>ConfigureRequestPipeline.</returns>
        public static WebApplication ConfigureRequestPipeline(this WebApplication app)
        {
#if DEBUG
#else
            // Release build → protect only on UAT and PROD
            if (app.Environment.IsEnvironment("UAT") || app.Environment.IsProduction())
            {
                app.UseMiddleware<SwaggerBasicAuthMiddleware>();
            }

#endif

            // Swagger UI
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shireen by Naeema Clothing API");
            });

            app.UseReDoc(options =>
            {
                options.SpecUrl("/swagger/v1/swagger.json");
                options.DocumentTitle = "Shireen by Naeema Clothing API";
                options.RoutePrefix = "docs";
            });

            // ReDoc UI
            app.UseReDoc(options =>
            {
                options.SpecUrl("/swagger/v1/swagger.json");
                options.DocumentTitle = "Shireen by Naeema Clothing API";
                options.RoutePrefix = "docs";
            });

            app.UseExceptionHandler();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCors();
            app.UseRouting();

            app.UseMiddleware<UnauthorizedResponseMiddleware>();
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizationFilter() },
            });
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapFallbackToFile("/index.html");
            return app;
        }
    }
}