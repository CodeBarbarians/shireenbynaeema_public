namespace Infrastructure
{
    using Domain;
    using Microsoft.EntityFrameworkCore;
    using SharedServices;

    /// <summary>
    /// DatabaseContext is the primary class responsible for interacting with the database using Entity Framework Core. It inherits from DbContext and serves
    /// as the main entry point for querying and saving data to the database. This class is configured to apply entity configurations, soft delete query filters,
    /// and seed initial data into the database. It also includes logic to prevent modifications to the seeded admin user, ensuring that critical seed data remains
    /// intact. The DatabaseContext class is essential for managing the application's data access layer and ensuring that the database schema is properly defined
    /// and maintained according to the application's requirements.
    /// </summary>
    public partial class DatabaseContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseContext"/> class.
        /// Constructor for the DatabaseContext class, which takes DbContextOptions as a parameter and passes it to the base DbContext constructor. This allows for
        /// dependency injection and configuration of the database context.
        /// </summary>
        /// <param name="options">DbContextOptions.</param>
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Overrides the SaveChanges method to include logic that prevents modifications to the seeded admin user. This ensures that the critical seed data
        /// for the admin user remains intact and cannot be accidentally modified or deleted through the application. The method checks for any added, modified,
        /// or deleted entries related to the admin user and throws an exception if such changes are detected, thereby enforcing the integrity of the seeded
        /// admin user data.
        /// </summary>
        /// <returns>int.</returns>
        public override int SaveChanges()
        {
            this.PreventAdminUserChanges();
            return base.SaveChanges();
        }

        /// <summary>
        /// Asynchronous version of the SaveChanges method, which includes the same logic to prevent modifications to the seeded admin user. This method ensures that any
        /// changes to the admin user are properly tracked and handled.
        /// </summary>
        /// <param name="cancellationToken">CancellationToken.</param>
        /// <returns>int.</returns>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            this.PreventAdminUserChanges();
            return await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Configures the database context options. This method can be used to set up the database provider, connection string, and other options related to
        /// the database context.
        /// </summary>
        /// <param name="optionsBuilder">DbContextOptionsBuilder.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        /// <summary>
        /// Configures the model by applying entity configurations, soft delete query filters, and seeding initial data. This method is called by the Entity Framework
        /// during the model creation process.
        /// </summary>
        /// <param name="modelBuilder">ModelBuilder.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all IEntityTypeConfiguration from this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DatabaseContext).Assembly);

            // Apply soft delete query filters (auto-excludes IsDeleted = true)
            modelBuilder.ApplySoftDeleteQueryFilters();

            // Configure indexes on IsDeleted for all soft-deletable entities
            modelBuilder.ConfigureSoftDeleteIndexes();

            // Add Sequences
            // modelBuilder.AddSequences();

            // Seed Data
            modelBuilder.SeedData();
        }

        /// <summary>
        /// Private method that checks for any changes to the seeded admin user in the ChangeTracker. It looks for added entries with the admin email,
        /// as well as modified or deleted entries with the admin user ID. If any such changes are detected, it throws an InvalidOperationException to
        /// prevent modifications to the seeded admin user data. This method is called before saving changes to the database to ensure that the integrity
        /// of the seeded admin user is maintained.
        /// </summary>
        /// <exception cref="InvalidOperationException">Seeded admin user cannot be modified or deleted.</exception>
        private void PreventAdminUserChanges()
        {
            var adminEmail = Constants.Seed.AdminEmail;
            var adminUserId = Constants.Seed.AdminUserId;

            var addedAdmins = this.ChangeTracker.Entries<User>()
                .Where(e => e.State == EntityState.Added && e.Entity.Email == adminEmail);

            if (addedAdmins.Any())
            {
                throw new InvalidOperationException("Admin user is already seeded and cannot be added again.");
            }

            var updatedOrDeletedAdmins = this.ChangeTracker.Entries<User>()
                .Where(e =>
                    (e.State == EntityState.Modified || e.State == EntityState.Deleted)
                    && e.Entity.Id == adminUserId);

            if (updatedOrDeletedAdmins.Any())
            {
                throw new InvalidOperationException("Seeded admin user cannot be modified or deleted.");
            }
        }
    }
}