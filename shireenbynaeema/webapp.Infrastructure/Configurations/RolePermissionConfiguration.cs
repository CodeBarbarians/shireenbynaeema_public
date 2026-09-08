namespace Infrastructure
{
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Configuration class for the RolePermission entity, defining the database schema and relationships for role permissions.
/// This configuration sets up a composite primary key.
/// </summary>
public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    /// <summary>
    /// Configures the RolePermission entity by defining its primary key, indexes, and relationships. The primary key is a composite key consisting of
    /// RoleId and PermissionName to ensure that each role can have multiple permissions, but each permission can only be assigned to a role once.
    /// An index is created on RoleId for efficient querying, and a foreign key relationship is established with the Role entity to maintain referential integrity.
    /// </summary>
    /// <param name="builder">The builder used to configure the entity's schema and relationships.</param>
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.HasKey(rp => new { rp.RoleId, rp.PermissionName });

        builder.HasIndex(rp => rp.RoleId)
               .HasDatabaseName("IX_RolePermission_RoleId");

        builder.HasOne(rp => rp.Role)
               .WithMany(r => r.RolePermissions)
               .HasForeignKey(rp => rp.RoleId);
    }
}
}