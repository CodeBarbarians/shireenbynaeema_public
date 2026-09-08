namespace Infrastructure
{
    using Domain;

    using Microsoft.EntityFrameworkCore;

    using SharedServices;

    using static SharedServices.Constants.Seed;

    /// <summary>
    /// Provides extension methods for seeding initial data into the Entity Framework Core model. This class enables the
    /// setup of essential roles, users, user-role assignments, and synchronization checkpoints to ensure the database
    /// contains required default entities after creation or migration.
    /// </summary>
    /// <remarks>Use these extension methods during model configuration to populate the database with
    /// predefined entities necessary for system operation. Seeding is typically performed in the OnModelCreating method
    /// of your DbContext to establish a consistent starting state for the application.</remarks>
    public static class SeedingExtensions
    {
        /// <summary>
        /// Seeds the database with initial data, including predefined roles, an admin user, facility users,
        /// user-role assignments, and synchronization checkpoints. This ensures the system has essential
        /// default entities for proper operation immediately after database creation or migration.
        /// </summary>
        /// <param name="modelBuilder">
        /// The Entity Framework Core ModelBuilder used to configure the entities and seed data.
        /// </param>
        public static void SeedData(this ModelBuilder modelBuilder)
        {
            // Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = SuperAdminRoleId, Name = SuperAdminRoleName, Scope = RoleScope.Customer },
                new Role { Id = SystemAdminRoleId, Name = SystemAdminRoleName, Scope = RoleScope.Customer },
                new Role { Id = UserRoleId, Name = UserRoleName, Scope = RoleScope.Customer });

            // Admin User
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = AdminUserId,
                Email = AdminEmail,
                FirstName = AdminFirstName,
                LastName = AdminLastName,
                DisplayName = AdminDisplayName,
                PasswordHash = DefaultPasswordHash,
                Status = 1,
                PhoneNo = null,
                InvitedBy = null,
                InvitationDate = null,
                ApprovalDate = null,
                CreatedOn = CreatedOnUnix,
                CreatedBy = SystemUserId,
                UpdatedOn = CreatedOnUnix,
                UpdatedBy = SystemUserId,
                IsSuperAdmin = true,
            });

            // UserRole assignment
            modelBuilder.Entity<UserRole>().HasData(new UserRole
            {
                UserId = AdminUserId,
                RoleId = SuperAdminRoleId,
            });

            // seed initial checkpoint data (removed - no sync needed)
        }
    }
}