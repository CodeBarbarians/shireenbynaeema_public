namespace Server
{
    using Domain;

    /// <summary>
    /// Extension method for registering application configurations in the dependency injection container.
    /// </summary>
    public static class ConfigurationRegistration
    {
        /// <summary>
        /// Registers the AppSettingsConfig class as a singleton service for the IAppSettingsConfig interface.
        /// This method reads configuration values from the provided IConfiguration instance and creates a concrete
        /// instance of AppSettingsConfig. It also configures the options pattern, allowing other services to inject
        /// IOptions&lt;AppSettingsConfig&gt; if needed.
        /// </summary>
        /// <param name="services">The dependency injection service collection.</param>
        /// <param name="configuration">The application configuration from which settings will be read.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
        public static IServiceCollection AddAppConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            // Create the AppSettingsConfig instance by reading from IConfiguration directly
            var appSettings = new AppSettingsConfig(configuration);

            // Register the concrete instance as singleton for IAppSettingsConfig
            services.AddSingleton<IAppSettingsConfig>(appSettings);

            // Optional: still bind for IOptions<AppSettingsConfig> if other services need it
            services.Configure<AppSettingsConfig>(configuration);

            return services;
        }
    }
}