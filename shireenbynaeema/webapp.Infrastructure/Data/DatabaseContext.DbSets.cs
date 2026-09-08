namespace Infrastructure
{
    using Domain;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Partial class for DatabaseContext that defines the DbSet properties for each entity in the application. These DbSet properties represent the tables in
    /// the database and allow for querying and saving instances of the corresponding entities. By defining these DbSet properties, we can easily perform CRUD
    /// operations on the database using Entity Framework Core's LINQ queries and change tracking features. This partial class is a crucial part of the DatabaseContext,
    /// as it provides the necessary properties to interact with the various entities in the application, such as users, roles, work orders, notifications, and more.
    /// Each DbSet property corresponds to a specific entity type, enabling us to manage and manipulate data effectively within our application.
    /// </summary>
    public partial class DatabaseContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseContext"/> class with the specified set of customer priorities.
        /// </summary>
        public DatabaseContext()
        {
        }

        /// <summary>
        /// Gets or sets dbSet properties for each entity in the application, representing the tables in the database and allowing for querying and saving instances
        /// of the corresponding entities.
        /// </summary>
        public DbSet<AuditLog> AuditLogs { get; set; }

        /// <summary>
        /// Gets or sets dbSet property for the ExceptionLog entity, representing the ExceptionLogs table in the database and allowing for querying and saving instances of
        /// the ExceptionLog entity.
        /// </summary>
        public DbSet<ExceptionLog> ExceptionLogs { get; set; }

        /// <summary>
        /// Gets or sets dbSet property for the EmailLog entity, representing the EmailLogs table in the database and allowing for querying and saving instances of the
        /// EmailLog entity.
        /// </summary>
        public DbSet<EmailLog> EmailLogs { get; set; }

        /// <summary>
        /// Gets or sets dbSet property for the Role entity, representing the Roles table in the database and allowing for querying and saving instances of the Role entity. This
        /// DbSet is essential for managing user roles and permissions within the application, enabling us to define and assign roles to users for access control
        /// purposes.
        /// </summary>
        public DbSet<Role> Roles { get; set; }

        /// <summary>
        /// Gets or sets dbSet property for the User entity, representing the Users table in the database and allowing for querying and saving instances of the User entity. This
        /// DbSet is essential for managing user accounts and authentication within the application, enabling us to create, read, update, and delete user records as needed.
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Gets or sets dbSet property for the UserRole entity, representing the UserRoles table in the database and allowing for querying and saving instances of the UserRole entity. This
        /// DbSet is essential for managing the many-to-many relationship between users and roles within the application, enabling us to assign multiple roles to users and vice versa.
        /// </summary>
        public DbSet<UserRole> UserRoles { get; set; }

        /// <summary>
        /// Gets or sets dbSet property for the RolePermission entity, representing the RolePermissions table in the database and allowing for querying and saving instances of the RolePermission entity. This
        /// DbSet is essential for managing the many-to-many relationship between roles and permissions within the application, enabling us to assign multiple permissions to roles and vice versa.
        /// </summary>
        public DbSet<RolePermission> RolePermissions { get; set; }

        /// <summary>
        /// Gets or sets dbSet property for the UserPermission entity, representing the UserPermissions table in the database and allowing for querying and saving instances of the UserPermission entity. This
        /// DbSet is essential for managing the many-to-many relationship between users and permissions within the application, enabling us to assign multiple permissions to users and vice versa.
        /// </summary>
        public DbSet<UserPermission> UserPermissions { get; set; }

        /// <summary>
        /// Gets or sets dbSet property for the PasswordResetToken entity, representing the PasswordResetTokens table in the database and allowing for querying and saving instances of the PasswordResetToken entity. This
        /// DbSet is essential for managing password reset tokens within the application, enabling us to create, read, update, and delete password reset tokens as needed.
        /// </summary>
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        /// <summary>
        /// Gets or sets dbSet property for the UserInviteToken entity, representing the UserInviteTokens table in the database and allowing for querying and saving instances of the UserInviteToken entity. This
        /// DbSet is essential for managing user invite tokens within the application, enabling us to create, read, update, and delete user invite tokens as needed.
        /// </summary>
        public DbSet<UserInviteToken> UserInviteTokens { get; set; }

        /// <summary>
        /// Gets or sets dbSet property for the OtpCode entity, representing the OtpCodes table in the database and allowing for querying and saving instances of the OtpCode entity. This
        /// DbSet is essential for managing OTP codes within the application, enabling us to create, read, update, and delete OTP codes as needed.
        /// </summary>
        public DbSet<OtpCode> OtpCodes { get; set; }

        /// <summary>
        /// Gets or sets dbSet property for the UserInviteLog entity, representing the UserInviteLogs table in the database and allowing for querying and saving instances of the UserInviteLog entity. This
        /// DbSet is essential for managing user invite logs within the application, enabling us to create, read, update, and delete user invite log records as needed.
        /// </summary>
        public DbSet<UserInviteLog> UserInviteLogs { get; set; }

        /// <summary>
        /// Gets or sets dbSet property for the UserLoginLog entity, representing the UserLoginLogs table in the database and allowing for querying and saving instances of the UserLoginLog entity. This
        /// DbSet is essential for managing user login logs within the application, enabling us to create, read, update, and delete user login log records as needed.
        /// </summary>
        public DbSet<UserLoginLog> UserLoginLogs { get; set; }

        /// <summary>
        /// Gets or sets dbSet property for the Notification entity, representing the Notifications table in the database and allowing for querying and saving instances of the Notification entity. This
        /// DbSet is essential for managing notifications within the application, enabling us to create, read, update, and delete notification records as needed.
        /// </summary>
        public DbSet<Notification> Notifications { get; set; }

        /// <summary>
        /// Gets or sets dbSet property for the NotificationSetting entity, representing the NotificationSettings table in the database and allowing for querying and saving instances of the
        /// NotificationSetting entity.
        /// </summary>
        public DbSet<NotificationSetting> NotificationSettings { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<ProductVariant> ProductVariants { get; set; }

        public DbSet<ProductImage> ProductImages { get; set; }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<Address> Addresses { get; set; }

        public DbSet<Cart> Carts { get; set; }

        public DbSet<CartItem> CartItems { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<Invoice> Invoices { get; set; }

        public DbSet<Delivery> Deliveries { get; set; }

        public DbSet<Return> Returns { get; set; }

        public DbSet<SizeChart> SizeCharts { get; set; }

        public DbSet<Rating> Ratings { get; set; }

        public DbSet<CmsPage> CmsPages { get; set; }

        public DbSet<Setting> Settings { get; set; }

        public DbSet<DeliveryItem> DeliveryItems { get; set; }

        public DbSet<ReturnItem> ReturnItems { get; set; }

        public DbSet<Coupon> Coupons { get; set; }

        public DbSet<OrderCoupon> OrderCoupons { get; set; }

        public DbSet<BlogPost> BlogPosts { get; set; }

        public DbSet<CategoryImage> CategoryImages { get; set; }
    }
}