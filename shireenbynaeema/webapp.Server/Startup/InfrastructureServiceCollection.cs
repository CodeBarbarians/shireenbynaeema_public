namespace Server
{
    using System.Text;

    using Domain;
    using Hangfire;
    using Hangfire.PostgreSql;
    using Infrastructure;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.OpenApi.Models;

    /// <summary>
    /// Extension method for setting up application infrastructure services, including database contexts, authentication, CORS, Swagger, and other related services.
    /// This class centralizes the configuration of these services to keep the Startup.cs file clean and organized. By calling the AddApplicationDependencies <see />
    /// in Startup.cs, all necessary infrastructure services will be registered with the dependency injection container, ensuring that they are available throughout
    /// the application. This includes setting up Entity Framework Core with SQL Server, configuring JWT authentication, enabling CORS with specified allowed origins,
    /// and integrating Swagger for API documentation. Additionally, it configures global exception handling and problem details for consistent error responses across
    /// the API.
    /// </summary>
    public static class InfrastructureServiceCollection
    {
        /// <summary>
        /// Adds application infrastructure services to the dependency injection container, including database contexts, authentication, CORS, Swagger, and other
        /// related services.
        /// </summary>
        /// <param name="services">services.</param>
        /// <returns>AddApplicationDependencies.</returns>
        public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
        {
            services.AddDataProtection();
            services.AddHttpContextAccessor();

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            // Resolve AppSettingsConfig singleton from DI
            var sp = services.BuildServiceProvider();
            var appSettings = sp.GetRequiredService<IAppSettingsConfig>();

            // DbContexts
            services.AddDbContext<DatabaseContext>((sp2, options) =>
            {
                options.UseNpgsql(appSettings.ConnectionStrings!.Default!, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly("webapp.Server");
                    npgsqlOptions.CommandTimeout(300);
                })
                .AddInterceptors(sp2.GetRequiredService<AuditInterceptor>());
            });

            // Hangfire
            services.AddHangfire(config =>
            {
                config.UsePostgreSqlStorage(options =>
                {
                    options.UseNpgsqlConnection(appSettings.ConnectionStrings!.Default!);
                })
                .WithJobExpirationTimeout(TimeSpan.FromDays(5));
            });
#if DEBUG
            services.AddHangfireServer();
#else
        services.AddHangfireServer();
#endif

            // Controllers
            services.AddControllers(options =>
            {
                options.Filters.Add<ValidateModelFilter>();
            }).AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            });

            // Swagger
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization using Bearer",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Scheme = "Bearer",
                    Type = SecuritySchemeType.ApiKey,
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme },
                        In = ParameterLocation.Header,
                    },
                    new List<string>()
                },
                });
            });

            // JWT Authentication
            var jwtConfig = appSettings.Jwt!;
            var key = Encoding.UTF8.GetBytes(jwtConfig.Key!);
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero, // 👈 default is 5 minutes, so reduce to 0 to make expiry exact
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtConfig.Issuer,
                        ValidAudience = jwtConfig.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                    };
                });

            // CORS
            var allowedOrigins = appSettings.Cors?.AllowedOrigins ?? Array.Empty<string>();
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins(allowedOrigins)
                           .AllowCredentials()
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                });
            });

            // Stimulsoft License
            Stimulsoft.Base.StiLicense.Key = appSettings.StiLicense;
            services.AddAuthorization();

            // for Role Status Check

            /*services.AddAuthorization(options =>
            {
                // This runs for EVERY [Authorize] automatically
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(new ActiveRoleRequirement())
                    .Build();
            });*/

            // Exception handling & problem details
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            return services;
        }
    }
}