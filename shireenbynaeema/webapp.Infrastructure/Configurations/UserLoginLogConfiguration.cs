namespace Infrastructure
{
    using Domain;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// Configuration class for the AuditLog entity, defining the database schema and indexes for the AuditLog table. This configuration ensures that the
    /// AuditLog table is properly set up in the database with the necessary constraints and indexes to optimize query performance for common search scenarios,
    /// such as filtering by timestamp, entity name, user ID, and action type. The indexes defined in this configuration will help improve the efficiency of
    /// queries that retrieve audit log entries based on these criteria, which is essential for maintaining an effective auditing system within the application.
    /// The AuditLog entity is likely used to store records of various actions performed within the application, including details such as the timestamp of the action,
    /// the name of the entity involved, the user ID of the person who performed the action, and the type of action taken. By configuring the database schema with
    /// appropriate indexes, we can ensure that retrieving audit log entries is fast and efficient, even as the volume of log data grows over time. This configuration
    /// plays a crucial role in supporting the application's auditing and monitoring capabilities, allowing administrators and developers to quickly
    /// access relevant audit information when needed.
    /// </summary>
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        /// <summary>
        /// Configures the AuditLog entity by defining indexes on the Timestamp, EntityName, UserId, and Action properties to optimize query performance for
        /// common search scenarios.
        /// </summary>
        /// <param name="builder">The builder used to configure the entity's schema and relationships.</param>
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.HasIndex(e => e.Timestamp)
                   .HasDatabaseName("IX_AuditLog_Timestamp");

            builder.HasIndex(e => new { e.EntityName, e.Timestamp })
                   .HasDatabaseName("IX_AuditLog_EntityName_Timestamp");

            builder.HasIndex(e => new { e.UserId, e.Timestamp })
                   .HasDatabaseName("IX_AuditLog_UserId_Timestamp");

            builder.HasIndex(e => e.Action)
                   .HasDatabaseName("IX_AuditLog_Action");
        }
    }
}