namespace SharedServices
{
    using System.Linq.Expressions;

    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Provides extension methods for working with Entity Framework Core DbSet instances.
    /// </summary>
    /// <remarks>This static class contains helper methods that extend the functionality of Entity Framework
    /// Core, enabling more convenient or expressive data access patterns when working with DbSet objects.</remarks>
    public static class EFCoreExtensions
    {
        /// <summary>
        /// Asynchronously determines whether any entity in the set matches the specified predicate and returns the
        /// matching entity if found.
        /// </summary>
        /// <remarks>If multiple entities match the predicate, only the first one is returned. The
        /// operation is performed asynchronously and does not block the calling thread.</remarks>
        /// <typeparam name="T">The type of the entities in the <see cref="DbSet{TEntity}"/>.</typeparam>
        /// <param name="set">The <see cref="DbSet{TEntity}"/> to search for a matching entity.</param>
        /// <param name="predicate">An expression that defines the conditions of the entity to search for. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see
        /// cref="CheckExistsResult{T}"/> indicating whether a matching entity exists and, if so, the entity itself.</returns>
        public static async Task<CheckExistsResult<T>> CheckExists<T>(
            this DbSet<T> set,
            Expression<Func<T, bool>> predicate)
            where T : class
        {
            var entity = await set.FirstOrDefaultAsync(predicate).ConfigureAwait(false);
            return new CheckExistsResult<T>
            {
                Entity = entity,
                Exists = entity != null,
            };
        }
    }

    /// <summary>
    /// Result class for the CheckExists extension method, containing the entity (if found) and a boolean indicating whether the entity exists in the database
    /// This class is used to encapsulate the result of the CheckExists method, allowing callers to easily determine if an entity matching the specified
    /// predicate exists and to access the entity itself if it does. The Entity property will be null if no matching entity is found, while the Exists
    /// property will indicate whether a match was found or not.
    /// </summary>
    /// <typeparam name="T">T.</typeparam>
    public class CheckExistsResult<T>
        where T : class
    {
        /// <summary>
        /// Gets or sets the entity associated with the current operation.
        /// </summary>
        public T? Entity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the resource exists.
        /// </summary>
        public bool Exists { get; set; }
    }
}