namespace Infrastructure
{
    using System.Linq.Expressions;

    using Application;
    using Domain;
    using Microsoft.EntityFrameworkCore;
    using SharedServices;

    /// <summary>
    /// Provides a generic repository implementation for performing asynchronous CRUD operations on entities of type T
    /// within a database context.
    /// </summary>
    /// <remarks>This repository supports adding, updating, deleting, and retrieving entities using
    /// asynchronous methods. It is designed to work with a DatabaseContext and requires that T implements IIdentifiable
    /// to ensure entities have unique identifiers. The repository handles common data access patterns and can be
    /// extended for custom behaviors. Thread safety depends on the underlying DatabaseContext; concurrent operations
    /// should be managed appropriately.</remarks>
    /// <typeparam name="T">The entity type managed by the repository. Must be a class that implements the IIdentifiable interface.</typeparam>
    public class Repository<T> : IRepository<T>
        where T : class, IIdentifiable
    {
        private readonly DatabaseContext dbcontext;

        /// <summary>
        /// Initializes a new instance of the <see cref="Repository{T}"/> class using the specified database context.
        /// </summary>
        /// <param name="dbcontext">The database context to be used for data operations. Cannot be null.</param>
        public Repository(DatabaseContext dbcontext)
        {
            this.dbcontext = dbcontext;
        }

        /// <summary>
        /// Adds a new entity of type T to the database asynchronously.
        /// This method adds the entity to the DbContext and persists the changes.
        /// </summary>
        /// <param name="entity">The entity object to be added to the database.</param>
        /// <returns>A <see cref="SaveResponse"/> indicating whether the add operation was successful.</returns>
        public async Task<SaveResponse> AddAsync(T entity)
        {
            var flag = false;

            await this.dbcontext.Set<T>().AddAsync(entity);
            await this.dbcontext.SaveChangesAsync();
            flag = true;
            return new SaveResponse(flag);
        }

        /// <summary>
        /// Adds multiple entities of type T to the database asynchronously.
        /// This method adds all entities in the provided list to the DbContext and saves the changes.
        /// </summary>
        /// <param name="entity">A list of entities to be added to the database.</param>
        /// <returns>A <see cref="SaveResponse"/> indicating whether the add range operation was successful.</returns>
        public async Task<SaveResponse> AddRangeAsync(List<T> entity)
        {
            var flag = false;
            await this.dbcontext.Set<T>().AddRangeAsync(entity);
            await this.dbcontext.SaveChangesAsync();
            flag = Constants.ResponseSuccess;
            return new SaveResponse(flag);
        }

        /// <summary>
        /// Deletes an entity of type T from the database asynchronously using its unique identifier (GUID).
        /// The method attempts to remove the entity and returns a <see cref="DeleteReponse"/> indicating success or failure.
        /// </summary>
        /// <param name="entity">The GUID of the entity to be deleted.</param>
        /// <returns>A <see cref="DeleteReponse"/> representing the result of the deletion operation, including any exception if it occurs.</returns>
        public async Task<DeleteReponse> DeleteAsync(Guid entity)
        {
            try
            {
                await this.dbcontext.Set<T>().Where(x => x.Id == entity).ExecuteDeleteAsync();
                await this.dbcontext.SaveChangesAsync();
                return new DeleteReponse(true);
            }
            catch (Exception ex)
            {
                return new DeleteReponse(Constants.ResponseFailure, ex);
            }
        }

        /// <summary>
        /// Retrieves a paginated list of entities of type T from the database asynchronously.
        /// The method applies the pagination parameters (Skip and Take) from the provided <see cref="ListRequest"/>
        /// and returns a <see cref="ListResponse{T}"/> containing the requested page of entities along with the total count.
        /// </summary>
        /// <param name="request">The pagination and filtering parameters encapsulated in a <see cref="ListRequest"/> object.</param>
        /// <returns>A <see cref="ListResponse{T}"/> containing the entities for the current page and the total number of entities.</returns>
        public async Task<ListResponse<T>> GetAllAsync(ListRequest request)
        {
            var query = this.dbcontext.Set<T>()
                .AsNoTracking()
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync();

            return new ListResponse<T>(pageNumber: request.PageNumber, pageSize: request.PageSize)
            {
                Entities = items,
                TotalCount = totalCount,
            };
        }

        /// <summary>
        /// Retrieves a single entity of type T from the database asynchronously that matches the specified predicate expression.
        /// </summary>
        /// <param name="predicate">A LINQ expression defining the condition to filter the entity.</param>
        /// <returns>The entity of type T that matches the predicate.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the database context is not initialized.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no entity matching the predicate is found.</exception>
        public async Task<T> GetByFieldNameAsync(Expression<Func<T, bool>> predicate)
        {
            if (this.dbcontext == null)
            {
                throw new InvalidOperationException("Database context is not initialized.");
            }

            var entity = await this.dbcontext.Set<T>().FirstOrDefaultAsync(predicate);
            if (entity == null)
            {
                throw new KeyNotFoundException($"Entity of type {typeof(T).Name} matching the given criteria was not found.");
            }

            return entity;
        }

        /// <summary>
        /// Retrieves a single entity of type T from the database asynchronously using its unique identifier (GUID).
        /// </summary>
        /// <param name="id">The unique identifier (GUID) of the entity to retrieve.</param>
        /// <returns>A RetrieveResponse containing the entity of type T if found.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if no entity with the specified Id is found.</exception>
        public async Task<RetrieveResponse<T>> GetByIdAsync(Guid id)
        {
            var entity = await this.dbcontext.Set<T>().FindAsync(id);
            if (entity == null)
            {
                throw new KeyNotFoundException($"Entity of type {typeof(T).Name} with Id {id} was not found.");
            }

            return new RetrieveResponse<T>(entity);
        }

        /// <summary>
        /// Updates an existing entity of type T in the database asynchronously.
        /// Ensures the entity exists, applies the updated values, and saves changes.
        /// </summary>
        /// <param name="entityToUpdate">The entity object containing updated values and a valid Id.</param>
        /// <returns>A SaveResponse indicating whether the update was successful.</returns>
        /// <exception cref="ArgumentException">Thrown if the entity to update does not exist in the database.</exception>
        public async Task<SaveResponse> UpdateAsync(T entityToUpdate)
        {
            var flag = false;

            // Retrieves the entity using the ID to ensure it exists in the database
            var entity = await this.dbcontext.Set<T>().FindAsync(entityToUpdate.Id);
            if (entity == null)
            {
                throw new ArgumentException("Entity not found.");
            }

            // Maps the updated values to the tracked entity
            this.dbcontext.Entry(entity).CurrentValues.SetValues(entityToUpdate);

            // Saves changes asynchronously
            await this.dbcontext.SaveChangesAsync();

            flag = true;

            return new SaveResponse(flag);
        }
    }
}