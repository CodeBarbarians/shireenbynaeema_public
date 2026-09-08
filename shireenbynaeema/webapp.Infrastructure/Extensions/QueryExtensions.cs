namespace Infrastructure
{
using Domain;
using Microsoft.EntityFrameworkCore;
using SharedServices;

/// <summary>
/// Provides extension methods for querying and managing entities that support soft deletion.
/// </summary>
/// <remarks>These methods enable working with entities implementing the ISoftDeletable interface, allowing for
/// inclusion, filtering, restoration, and soft deletion of records. Use these extensions to control visibility and
/// state of soft-deleted entities in queries and data operations. Thread safety and transactional consistency depend on
/// the underlying data context and should be considered when using these methods.</remarks>
public static class QueryExtensions
{
    /// <summary>
    /// Returns a query that includes entities marked as deleted by soft-delete filters.
    /// </summary>
    /// <remarks>
    /// Use this method when you need to access entities that have been soft-deleted and would
    /// otherwise be excluded by global query filters. This is typically used for administrative or auditing purposes.
    /// </remarks>
    /// <typeparam name="T">
    /// The type of the entities in the query. Must implement the ISoftDeletable interface.
    /// </typeparam>
    /// <param name="query">
    /// The source query to modify to include soft-deleted entities. Cannot be null.
    /// </param>
    /// <returns>
    /// An IQueryable&lt;T&gt; that includes both active and soft-deleted entities.
    /// </returns>
    public static IQueryable<T> IncludeDeleted<T>(this IQueryable<T> query)
        where T : class, ISoftDeletable
    {
        return query.IgnoreQueryFilters();
    }

    /// <summary>
    /// Returns a queryable collection containing only entities that have been marked as deleted using soft deletion.
    /// </summary>
    /// <remarks>This method removes any global query filters and selects entities that are considered deleted
    /// according to the ISoftDeletable contract. Use this method to access soft-deleted records that are typically
    /// excluded from standard queries.</remarks>
    /// <typeparam name="T">The entity type that implements the ISoftDeletable interface.</typeparam>
    /// <param name="query">The source queryable collection to filter for deleted entities.</param>
    /// <returns>An IQueryable containing only entities where the IsDeleted property is set to <see langword="true"/>.</returns>
    public static IQueryable<T> OnlyDeleted<T>(this IQueryable<T> query)
        where T : class, ISoftDeletable
    {
        return query.IgnoreQueryFilters().Where(e => e.IsDeleted);
    }

    /// <summary>
    /// Marks the specified entity as soft-deleted by setting its deletion flags and metadata.
    /// </summary>
    /// <remarks>Soft deletion updates the entity's IsDeleted property to <see langword="true"/>, and records
    /// the deletion timestamp and user. The entity remains in the data store and can be restored or audited later. This
    /// method does not physically remove the entity.</remarks>
    /// <typeparam name="T">The type of the entity to be soft-deleted. Must implement the ISoftDeletable interface.</typeparam>
    /// <param name="entity">The entity instance to mark as soft-deleted. Cannot be null.</param>
    public static void SoftDelete<T>(this T entity)
        where T : class, ISoftDeletable
    {
        entity.IsDeleted = true;
        entity.DeletedOn = DateTimeExtension.UtcNowUnixTimestamp;
        entity.DeletedBy = CurrentUser.UserId;
    }
}
}