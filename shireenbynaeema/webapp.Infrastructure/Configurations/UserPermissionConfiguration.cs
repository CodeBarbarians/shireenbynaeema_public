namespace Infrastructure
{
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Configuration class for the UserPermission entity, defining the database schema and constraints for the UserPermission table. This configuration includes
/// defining a composite primary key consisting of the UserId and PermissionName properties, as well as an index on the UserId property to optimize query performance
/// when filtering by user. Additionally, it establishes a foreign key relationship between the UserPermission entity and the User entity, ensuring referential
/// integrity between the two tables. The UserPermission entity is used to represent the permissions assigned to users within the application, allowing for fine-grained
/// access control based on user roles and permissions. This configuration ensures that the UserPermission table is structured efficiently for storing and retrieving
/// user permission data while maintaining data integrity and optimizing query performance.
/// </summary>
public class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
{
    /// <summary>
    /// Configures the UserPermission entity by defining the composite primary key, creating an index on the UserId property, and establishing a foreign key relationship.
    /// </summary>
    /// <param name="builder">The builder used to configure the entity's schema and relationships.</param>
    public void Configure(EntityTypeBuilder<UserPermission> builder)
    {
        builder.HasKey(up => new { up.UserId, up.PermissionName });

        builder.HasIndex(up => up.UserId)
               .HasDatabaseName("IX_UserPermission_UserId");

        builder.HasOne(up => up.User)
               .WithMany(u => u.UserPermissions)
               .HasForeignKey(up => up.UserId);
    }
}
}