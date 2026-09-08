namespace Infrastructure
{
    using Domain;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// Configuration class for the UserLoginLog entity, defining the database schema and indexes for the UserLoginLog table. This configuration ensures that the
    /// UserLoginLog table is properly set up in the database with the necessary constraints and indexes.
    /// </summary>
    public class UserLoginLogConfiguration : IEntityTypeConfiguration<UserLoginLog>
    {
        /// <summary>
        /// Configures the indexes for the UserLoginLog entity in the model builder.
        /// </summary>
        /// <remarks>This method defines single and composite indexes to optimize query performance for
        /// common access patterns, such as searching by attempted login time, user ID, email, and login success status.
        /// Call this method within the Entity Framework model configuration to ensure the indexes are applied when the
        /// database is created or updated.</remarks>
        /// <param name="builder">The builder used to configure the UserLoginLog entity's schema and indexes.</param>
        public void Configure(EntityTypeBuilder<UserLoginLog> builder)
        {
            // Single column index
            builder.HasIndex(e => e.AttemptedAt)
                   .HasDatabaseName("IX_UserLoginLog_AttemptedAt");

            // Composite index
            builder.HasIndex(e => new { e.UserId, e.AttemptedAt })
                   .HasDatabaseName("IX_UserLoginLog_UserId_AttemptedAt");

            builder.HasIndex(e => e.Email)
                   .HasDatabaseName("IX_UserLoginLog_Email");

            builder.HasIndex(e => new { e.IsSuccessful, e.AttemptedAt })
                   .HasDatabaseName("IX_UserLoginLog_IsSuccessful_AttemptedAt");
        }
    }
}