namespace Domain
{
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Defines the contract for accessing application configuration settings, including connection strings,
    /// authentication, email, storage, and other operational parameters.
    /// </summary>
    /// <remarks>This interface provides strongly-typed access to various configuration sections required by
    /// the application. Implementations typically retrieve values from app settings, environment variables, or other
    /// configuration sources. Members expose settings for integrations such as SendGrid, Azure Blob Storage, JWT
    /// authentication, and more, enabling centralized management of configuration data.</remarks>
    public interface IAppSettingsConfig
    {
        /// <summary>
        /// Gets the connection strings configuration, providing access to database connection strings used by the application.
        /// </summary>
        ConnectionStringsConfig ConnectionStrings { get; }

        /// <summary>
        /// Gets the SendGrid configuration, which includes settings for email integration such as API keys and email addresses used for sending notifications and other communications through SendGrid.
        /// </summary>
        SendGridConfig SendGrid { get; }

        /// <summary>
        /// Gets the password reset configuration, which includes settings related to password reset functionality such as the URL for password reset and token expiry time. This configuration is essential for managing user authentication and account recovery processes within the application.
        /// </summary>
        PasswordResetConfig PasswordReset { get; }

        /// <summary>
        /// Gets the configuration settings for user invitations.
        /// </summary>
        UserInviteConfig UserInvite { get; }

        /// <summary>
        /// Gets the configuration settings for vendor work orders.
        /// </summary>
        VendorWorkOrderConfig VendorWorkOrder { get; }

        /// <summary>
        /// Gets the configuration settings for Azure Blob Storage integration.
        /// </summary>
        AzureBlobStorageConfig AzureBlobStorage { get; }

        /// <summary>
        /// Gets the configuration settings for Google reCAPTCHA integration.
        /// </summary>
        /// <remarks>Use this property to access reCAPTCHA options required for validating user
        /// interactions and protecting forms from automated abuse. The configuration typically includes site keys,
        /// secret keys, and other relevant settings.</remarks>
        RecaptchaConfig Recaptcha { get; }

        /// <summary>
        /// Gets the configuration settings for JSON Web Token (JWT) authentication.
        /// </summary>
        /// <remarks>Use this property to access JWT-related options, such as token expiration, signing
        /// credentials, and validation parameters. The returned configuration is typically used when generating or
        /// validating JWTs within authentication workflows.</remarks>
        JwtConfig Jwt { get; }

        /// <summary>
        /// Gets the configuration settings for Cross-Origin Resource Sharing (CORS) for the application.
        /// </summary>
        CorsConfig Cors { get; }

        /// <summary>
        /// Gets the criteria used to filter work orders by the user who created them.
        /// </summary>
        ICheckWorkOrderCreatedByUserId WorkOrderCreatedByUserId { get; }

        /// <summary>
        /// Gets the identifier of the cutoff work order associated with this instance.
        /// </summary>
        int CutoffWOID { get; }

        /// <summary>
        /// Gets the URL of the banner image associated with the entity.
        /// </summary>
        string BannerImageURL { get; }

        /// <summary>
        /// Gets the configuration settings for front-end URLs used in the application, such as links to work order details, user lists, and estimate views. This configuration allows for centralized management of important URLs that are referenced throughout the application, ensuring consistency and ease of maintenance when URL changes are necessary.
        /// </summary>
        FrontEndUrls FrontEndUrls { get; }

        /// <summary>
        /// Gets the license key used for Stimulsoft reporting components.
        /// </summary>
        public string StiLicense { get; }

        /// <summary>
        /// Gets the encryption key used to secure sensitive data.
        /// </summary>
        public string EncryptionKey { get; }

        /// <summary>
        /// Gets or sets the API key used to authenticate requests to Azure Maps services.
        /// </summary>
        /// <remarks>The API key must be valid and authorized for the Azure Maps account associated with
        /// your application. Storing API keys securely is recommended to prevent unauthorized access.</remarks>
        string AzureMapsApiKey { get; set; }
    }

    /// <summary>
    /// Provides strongly-typed access to application configuration settings loaded from an IConfiguration source.
    /// </summary>
    /// <remarks>This class aggregates configuration sections such as connection strings, email settings,
    /// authentication, and other application-specific options. Use this type to access configuration values throughout
    /// the application in a consistent and type-safe manner. All properties are initialized from the corresponding
    /// configuration sections or values, with sensible defaults applied if a section is missing.</remarks>
    public class AppSettingsConfig : IAppSettingsConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppSettingsConfig"/> class using the specified configuration source.
        /// </summary>
        /// <remarks>Each property is populated from the corresponding section or value in the provided
        /// configuration. If a section or value is missing, the property is initialized with a default value. This
        /// constructor enables centralized access to application settings throughout the application.</remarks>
        /// <param name="config">The configuration source from which application settings are loaded. Cannot be null.</param>
        public AppSettingsConfig(IConfiguration config)
        {
            this.ConnectionStrings = config.GetSection("ConnectionStrings").Get<ConnectionStringsConfig>() ?? new ConnectionStringsConfig();
            this.SendGrid = config.GetSection("SendGrid").Get<SendGridConfig>() ?? new SendGridConfig();
            this.PasswordReset = config.GetSection("PasswordReset").Get<PasswordResetConfig>() ?? new PasswordResetConfig();
            this.UserInvite = config.GetSection("UserInvite").Get<UserInviteConfig>() ?? new UserInviteConfig();
            this.VendorWorkOrder = config.GetSection("VendorWorkOrder").Get<VendorWorkOrderConfig>() ?? new VendorWorkOrderConfig();
            this.AzureBlobStorage = config.GetSection("AzureBlobStorage").Get<AzureBlobStorageConfig>() ?? new AzureBlobStorageConfig();
            this.Recaptcha = config.GetSection("Recaptcha").Get<RecaptchaConfig>() ?? new RecaptchaConfig();
            this.Jwt = config.GetSection("Jwt").Get<JwtConfig>() ?? new JwtConfig();
            this.Cors = config.GetSection("Cors").Get<CorsConfig>() ?? new CorsConfig();
            this.WorkOrderCreatedByUserId = config.GetSection("ICheckWorkOrderCreatedByUserId").Get<ICheckWorkOrderCreatedByUserId>() ?? new ICheckWorkOrderCreatedByUserId();
            this.BannerImageURL = config.GetValue<string>("BannerImageURL") ?? string.Empty;
            this.FrontEndUrls = config.GetSection("FrontEndUrls").Get<FrontEndUrls>() ?? new FrontEndUrls();
            this.StiLicense = config.GetValue<string>("StiLicense") ?? string.Empty;
            this.CutoffWOID = config.GetValue<int?>("WOCutoffID") ?? 0;
            this.EncryptionKey = config.GetValue<string>("EncryptionKey") ?? string.Empty;
            this.AzureMapsApiKey = config.GetValue<string>("AzureMapsApiKey") ?? string.Empty;
        }

        /// <inheritdoc/>
        public ConnectionStringsConfig ConnectionStrings { get; }

        /// <inheritdoc/>
        public SendGridConfig SendGrid { get; }

        /// <inheritdoc/>
        public PasswordResetConfig PasswordReset { get; }

        /// <inheritdoc/>
        public UserInviteConfig UserInvite { get; }

        /// <inheritdoc/>
        public VendorWorkOrderConfig VendorWorkOrder { get; }

        /// <inheritdoc/>
        public AzureBlobStorageConfig AzureBlobStorage { get; }

        /// <inheritdoc/>
        public RecaptchaConfig Recaptcha { get; }

        /// <inheritdoc/>
        public JwtConfig Jwt { get; }

        /// <inheritdoc/>
        public CorsConfig Cors { get; }

        /// <inheritdoc/>
        public ICheckWorkOrderCreatedByUserId WorkOrderCreatedByUserId { get; }

        /// <inheritdoc/>
        public string BannerImageURL { get; }

        /// <inheritdoc/>
        public int CutoffWOID { get; }

        /// <inheritdoc/>
        public FrontEndUrls FrontEndUrls { get; }

        /// <inheritdoc/>
        public string StiLicense { get; }

        /// <inheritdoc/>
        public string EncryptionKey { get; }

        /// <inheritdoc/>
        public string AzureMapsApiKey { get; set; }
    }

    /// <summary>
    /// Represents a configuration container for database connection strings.
    /// </summary>
    /// <remarks>This class is typically used to store and access connection string values for different
    /// environments or services within an application. Properties correspond to named connection strings, such as the
    /// default connection or a development-specific connection.</remarks>
    public class ConnectionStringsConfig
    {
        /// <summary>
        /// Gets or sets the default value associated with the property.
        /// </summary>
        public string? Default { get; set; }

        /// <summary>
        /// Gets or sets the device identifier used for development checks.
        /// </summary>
        public string? ICheckDev { get; set; }
    }

    /// <summary>
    /// Represents configuration settings required for sending emails using the SendGrid service.
    /// </summary>
    /// <remarks>This class encapsulates the API key and email address information needed to authenticate and
    /// specify sender and recipient details when integrating with SendGrid. All properties must be set with valid
    /// values before using this configuration to send emails.</remarks>
    public class SendGridConfig
    {
        /// <summary>
        /// Gets or sets the API key used for authenticating requests to external services.
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the configuration for the sender's email address used in outgoing messages.
        /// </summary>
        public FromEmailConfig? FromEmail { get; set; }

        /// <summary>
        /// Gets or sets the configuration settings for sending email notifications to recipients.
        /// </summary>
        public ToEmailConfig? ToEmail { get; set; }
    }

    /// <summary>
    /// Represents configuration settings for sender email addresses used in various application notifications.
    /// </summary>
    /// <remarks>This class provides properties for specifying the sender email address for password reset
    /// emails, vendor dispatch communications, and client notifications. Each property can be set to customize the
    /// 'From' address used in outgoing emails for the corresponding notification type.</remarks>
    public class FromEmailConfig
    {
        /// <summary>
        /// Gets or sets the password reset token or code associated with the user.
        /// </summary>
        public string? PwdReset { get; set; }

        /// <summary>
        /// Gets or sets the vendor dispatch identifier or reference associated with the current entity.
        /// </summary>
        public string? VendorDispatch { get; set; }

        /// <summary>
        /// Gets or sets the client notifications identifier or reference associated with the current entity.
        /// </summary>
        public string? ClientNotifications { get; set; }
    }

    /// <summary>
    /// Represents the configuration for email recipients used for invoice notifications.
    /// </summary>
    public class ToEmailConfig
    {
        /// <summary>
        /// Gets or sets the invoice data associated with the entity.
        /// </summary>
        public string? Invoices { get; set; }
    }

    /// <summary>
    /// Represents configuration settings related to password reset functionality, including the URL for password reset and token expiry time. This configuration is essential for managing user authentication and account recovery processes within the application, allowing for secure and efficient handling of password reset requests. The properties in this class can be set to customize the behavior of the password reset feature according to the application's requirements.
    /// </summary>
    public class PasswordResetConfig
    {
        /// <summary>
        /// Gets or sets the URL associated with the resource.
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Gets or sets the token expiry duration, in seconds.
        /// </summary>
        public int? TokenExpiry { get; set; }
    }

    /// <summary>
    /// Represents configuration settings for user invitation functionality, including invitation URL, system
    /// administrator contact, and token expiration parameters.
    /// </summary>
    /// <remarks>Use this class to specify options related to sending and managing user invitations within the
    /// system. The properties allow customization of the invitation link, administrative contact email, and the
    /// duration for which invitation tokens remain valid.</remarks>
    public class UserInviteConfig
    {
        /// <summary>
        /// Gets or sets the URL associated with the resource.
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Gets or sets the email address of the system administrator.
        /// </summary>
        public string? SystemAdminEmail { get; set; }

        /// <summary>
        /// Gets or sets the token expiry duration, in seconds.
        /// </summary>
        public int? TokenExpiry { get; set; }
    }

    /// <summary>
    /// Represents configuration settings for a vendor work order integration, including endpoint URL and token
    /// expiration parameters.
    /// </summary>
    public class VendorWorkOrderConfig
    {
        /// <summary>
        /// Gets or sets the URL associated with the resource.
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Gets or sets the token expiry duration, in seconds, for authenticating requests to the vendor work order endpoint. This configuration is crucial for ensuring secure communication with the vendor's API, as it defines how long the authentication token remains valid before requiring renewal. Properly setting this value helps maintain security while allowing sufficient time for legitimate requests to be processed without interruption.
        /// </summary>
        public int? TokenExpiry { get; set; }
    }

    /// <summary>
    /// Represents configuration settings required to access and organize data in an Azure Blob Storage container.
    /// </summary>
    /// <remarks>Use this class to specify connection details and folder structure when interacting with Azure
    /// Blob Storage. The properties define the connection string, container name, and logical folder paths for
    /// organizing blobs within the container.</remarks>
    public class AzureBlobStorageConfig
    {
        /// <summary>
        /// Gets or sets the connection string used to establish a connection to the database.
        /// </summary>
        public string? ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the name of the container used for storage operations.
        /// </summary>
        public string? ContainerName { get; set; }

        /// <summary>
        /// Gets or sets the path to the root folder used by the application.
        /// </summary>
        public string? RootFolder { get; set; }

        /// <summary>
        /// Gets or sets the path to the folder where files are stored.
        /// </summary>
        public string? FilesFolder { get; set; }
    }

    /// <summary>
    /// Represents the configuration settings required to integrate Google reCAPTCHA into an application.
    /// </summary>
    /// <remarks>Use this class to provide the site and secret keys necessary for reCAPTCHA validation. Both
    /// keys must be obtained from the Google reCAPTCHA administration console. This configuration is typically used
    /// when verifying user responses to reCAPTCHA challenges.</remarks>
    public class RecaptchaConfig
    {
        /// <summary>
        /// Gets or sets the site key used for authentication or identification purposes.
        /// </summary>
        public string? SiteKey { get; set; }

        /// <summary>
        /// Gets or sets the secret key used for authentication or identification purposes. This key is typically used in server-side validation of reCAPTCHA responses to verify that the user interaction is legitimate and not automated. It is important to keep this key secure and not expose it in client-side code, as it can be used to bypass reCAPTCHA validation if compromised.
        /// </summary>
        public string? SecretKey { get; set; }
    }

    /// <summary>
    /// Represents configuration settings used for JSON Web Token (JWT) authentication, including the signing key,
    /// issuer, and audience values.
    /// </summary>
    /// <remarks>Use this class to provide the necessary parameters for generating and validating JWTs in
    /// authentication scenarios. The values should be set according to the requirements of your authentication provider
    /// and security policies.</remarks>
    public class JwtConfig
    {
        /// <summary>
        /// Gets or sets the key used for signing JWTs, which is essential for ensuring the integrity and authenticity of the tokens. This key should be kept secure and should be sufficiently complex to prevent unauthorized access or token forgery. The signing key is typically used in conjunction with a signing algorithm to generate a signature for the JWT, allowing recipients to verify that the token has not been tampered with and that it was issued by a trusted authority.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// Gets or sets the issuer of the JWT, which is a string that identifies the principal that issued the token. The issuer value is typically used in token validation to ensure that the token was issued by a trusted authority. When validating a JWT, the recipient can check the issuer claim against an expected value to confirm that the token is legitimate and was issued by a known source. Properly setting the issuer value is important for maintaining the security of your authentication system and preventing unauthorized access.
        /// </summary>
        public string? Issuer { get; set; }

        /// <summary>
        /// Gets or sets the intended recipient or audience for the token.
        /// </summary>
        public string? Audience { get; set; }
    }

    /// <summary>
    /// Represents the configuration settings for Cross-Origin Resource Sharing (CORS) policies.
    /// </summary>
    /// <remarks>Use this class to specify which origins are permitted to access resources in a web
    /// application. Typically used in server-side frameworks to control CORS behavior for HTTP requests.</remarks>
    public class CorsConfig
    {
        /// <summary>
        /// Gets or sets the array of allowed origins that are permitted to access resources in the application. This configuration is essential for enabling CORS in web applications, allowing you to specify which domains are allowed to make cross-origin requests to your server. Properly configuring allowed origins helps prevent unauthorized access while enabling legitimate interactions from trusted sources. Each origin should be specified as a string, and you can include multiple origins as needed to accommodate different environments or client applications.
        /// </summary>
        public string[]? AllowedOrigins { get; set; }
    }

    /// <summary>
    /// Represents environment-specific user identifiers for work order creation checks.
    /// </summary>
    /// <remarks>This class holds user IDs associated with the creation of work orders in different
    /// environments, such as UAT (User Acceptance Testing) and Production. Use these properties to determine which user
    /// created a work order in each environment.</remarks>
    public class ICheckWorkOrderCreatedByUserId
    {
        /// <summary>
        /// Gets or sets the user acceptance test (UAT) identifier associated with the entity.
        /// </summary>
        public int? UAT { get; set; }

        /// <summary>
        /// Gets or sets the production identifier associated with the entity.
        /// </summary>
        public int? Prod { get; set; }
    }

    /// <summary>
    /// Represents configuration settings for front-end URLs used in the application, such as links to work order details, user lists, and estimate views. This configuration allows for centralized management of important URLs that are referenced throughout the application, ensuring consistency and ease of maintenance when URL changes are necessary.
    /// </summary>
    public class FrontEndUrls
    {
        /// <summary>
        /// Gets or sets the detailed description or notes associated with the work order.
        /// </summary>
        public string? WorkOrderDetail { get; set; }

        /// <summary>
        /// Gets or sets the URL or identifier associated with the user list view in the application. This configuration is used to specify the location of the user list page, allowing for easy navigation and access to user-related information within the application. By centralizing this URL in the configuration, it becomes easier to update and maintain the link to the user list view as needed without having to search through code for hardcoded URLs.
        /// </summary>
        public string? UserList { get; set; }

        /// <summary>
        /// Gets or sets the URL or identifier associated with the estimate view in the application. This configuration is used to specify the location of the estimate view page, allowing for easy navigation and access to estimate-related information within the application. By centralizing this URL in the configuration, it becomes easier to update and maintain the link to the estimate view as needed without having to search through code for hardcoded URLs.
        /// </summary>
        public string? EstimateView { get; set; }
    }
}