namespace Infrastructure
{
    using Domain;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// Configures the entity mapping for the <see cref="Role"/> type within the Entity Framework model.
    /// </summary>
    /// <remarks>This configuration defines indexes for the <see cref="Role.Name"/> property, including a
    /// unique index filtered to non-deleted roles. Use this class when applying custom entity configurations in a
    /// DbContext to ensure consistent database schema and query performance.</remarks>
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        /// <summary>
        /// Configures the entity mapping for the Role type, including indexes and constraints.
        /// </summary>
        /// <remarks>This method sets up unique and filtered indexes on the Name property to optimize
        /// lookups and enforce constraints. Call this method within the Entity Framework model configuration to ensure
        /// proper database schema generation.</remarks>
        /// <param name="builder">The builder used to configure the Role entity's properties and relationships.</param>
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasIndex(x => x.Name)
                   .IsUnique()
                   .HasDatabaseName("IX_Role_Name")
                   .HasFilter("\"IsDeleted\" = false");
        }
    }
}