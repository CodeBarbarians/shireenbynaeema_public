namespace Infrastructure
{
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Configuration class for the UserRole entity, defining the database schema and constraints for the UserRole table. This configuration includes
/// defining a composite primary key consisting of the UserId and RoleId properties, as well as establishing foreign key relationships between the UserRole
/// entity and both the User and Role entities. The UserRole entity is used to represent the many-to-many relationship between users and roles within the
/// application, allowing for flexible assignment of roles to users. This configuration ensures that the UserRole table is structured efficiently for storing
/// and retrieving user-role association data while maintaining data integrity through proper foreign key constraints.
/// </summary>
public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    /// <summary>
    /// Configures the UserRole entity by defining its composite primary key and establishing relationships with the User and Role entities. The composite key is
    /// comprised of the UserId and RoleId properties.
    /// </summary>
    /// <param name="builder">The builder used to configure the entity's schema and relationships.</param>
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        // Composite key
        builder.HasKey(ur => new { ur.UserId, ur.RoleId });

        // Relationships
        builder.HasOne(ur => ur.User)
               .WithMany(u => u.UserRoles)
               .HasForeignKey(ur => ur.UserId);

        builder.HasOne(ur => ur.Role)
               .WithMany(r => r.UserRoles)
               .HasForeignKey(ur => ur.RoleId);
    }
}
}