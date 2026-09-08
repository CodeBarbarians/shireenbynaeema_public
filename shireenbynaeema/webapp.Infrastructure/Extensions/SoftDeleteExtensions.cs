namespace Infrastructure
{
using System.Linq.Expressions;

using Domain;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Provides extension methods for configuring soft delete behavior in Entity Framework Core models. These methods
/// enable automatic exclusion of soft-deleted entities from queries and optimize database indexing for soft delete
/// properties.
/// </summary>
/// <remarks>Use these extensions to implement soft delete functionality across all entities that implement the
/// ISoftDeletable interface. Applying the query filter ensures that entities marked as deleted are not returned in
/// query results by default. Configuring indexes on the soft delete property can improve query performance when
/// filtering by deletion status.</remarks>
public static class SoftDeleteExtensions
{
    /// <summary>
    /// Configures global query filters for entities implementing the ISoftDeletable interface to exclude soft-deleted
    /// records from query results.
    /// </summary>
    /// <remarks>This method adds a query filter to each entity type that implements ISoftDeletable, ensuring
    /// that entities with IsDeleted set to <see langword="true"/> are automatically excluded from queries. Use this
    /// method during model configuration to enable soft delete behavior across the DbContext. The filters are applied
    /// globally and affect all queries unless explicitly overridden.</remarks>
    /// <param name="modelBuilder">The ModelBuilder instance used to configure entity models and apply query filters.</param>
    public static void ApplySoftDeleteQueryFilters(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
            var falseConstant = Expression.Constant(false);
            var comparison = Expression.Equal(property, falseConstant);
            var lambda = Expression.Lambda(comparison, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }

    /// <summary>
    /// Configures indexes for the soft delete flag on all entities implementing the ISoftDeletable interface within the
    /// model.
    /// </summary>
    /// <remarks>This method adds a database index on the IsDeleted property for each entity type that
    /// supports soft deletion. The index improves query performance when filtering by the soft delete flag. Call this
    /// method during model configuration to ensure soft delete indexes are created for relevant entities.</remarks>
    /// <param name="modelBuilder">The ModelBuilder instance used to configure entity mappings and indexes for the current model.</param>
    public static void ConfigureSoftDeleteIndexes(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var tableName = entityType.GetTableName() ?? entityType.ClrType.Name;

            modelBuilder.Entity(entityType.ClrType)
                .HasIndex(nameof(ISoftDeletable.IsDeleted))
                .HasDatabaseName($"IX_{tableName}_IsDeleted");
        }
    }
}
}