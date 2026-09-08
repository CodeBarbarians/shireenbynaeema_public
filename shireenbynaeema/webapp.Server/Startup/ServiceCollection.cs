namespace Server
{
    using Application;

    using Domain;

    using Infrastructure;

    using Microsoft.AspNetCore.Authorization;

    using Services;

    using SharedServices;

    /// <summary>
    /// Extension method for registering application services in the dependency injection container. This class provides a centralized location for
    /// adding all the necessary services that the application depends on, including core services, shared services, and any custom services related
    /// to the application's functionality. By using this extension method, we can keep the Startup.cs file clean and organized, while ensuring that all
    /// required services are properly registered and available for dependency injection throughout the application. This promotes better maintainability
    /// and separation of concerns by encapsulating service registration logic in a dedicated class.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds application services to the dependency injection container. This method registers all the necessary services that the application depends on,
        /// including core services, shared services, and any custom services related to the application's functionality.
        /// </summary>
        /// <param name="services">services.</param>
        /// <returns>AddApplicationServices.</returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IUserPermissionService, UserPermissionService>();
            services.AddScoped<IRoleService, RoleService>();

            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddScoped<IAuditScope, AuditScope>();
            services.AddScoped<AuditCompareBuilder>();
            services.AddScoped<AuditLookupCache>();
            services.AddScoped<AuditValueResolver>();
            services.AddScoped<AuditInterpreter>();

            services.AddScoped<IExceptionLogService, ExceptionLogService>();
            services.AddScoped<IUserLoginLogService, UserLoginLogService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserExtensionService, UserExtensionService>();
            services.AddScoped<IPasswordResetTokenService, PasswordResetTokenService>();
            services.AddScoped<IUserInviteTokenService, UserInviteTokenService>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddScoped<INotificationService, NotificationService>();

            services.AddScoped<INotificationSettingService, NotificationSettingService>();
            services.AddScoped<IProfileDetailsService, ProfileDetailsService>();
            services.AddScoped<IActivityLogService, ActivityLogService>();

            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<IDeliveryService, DeliveryService>();
            services.AddScoped<IReturnService, ReturnService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IRatingService, RatingService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<ICouponService, CouponService>();
            services.AddScoped<IBlogPostService, BlogPostService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<ICmsPageService, CmsPageService>();
            services.AddScoped<ISettingService, SettingService>();
            services.AddScoped<IPaymentService, PaymentService>();

            services.AddScoped<IBlobFileService, BlobFileService>();
            services.AddScoped<IEmailService, EmailService>();

            services.AddHttpClient<IRecaptchaService, RecaptchaService>();
            services.AddHttpClient<IGeocodingService, GeocodingService>();

            services.AddScoped<ICryptoService, CryptoService>();

            services.AddScoped<ICustomBackgroundJobClient, CustomBackgroundJobClient>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(IService<>), typeof(Service<>));
            services.AddTransient<IResponse, Response>(config => { return new Response() { Data = null, Code = null, Message = null, IsSuccess = false, }; });

            services.AddScoped<AuditInterceptor>();
            services.AddScoped<ICurrentUserContext, CurrentUserContext>();

            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, PermissionHandler>();
            return services;
        }
    }
}