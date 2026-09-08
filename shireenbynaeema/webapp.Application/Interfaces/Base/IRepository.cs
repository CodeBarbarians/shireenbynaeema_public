namespace Application
{
    using System.Linq.Expressions;

    using Domain;
    using SharedServices;

    /// <summary>
    /// Provides generic repository methods for performing CRUD operations on entities of type T.
    /// </summary>
    /// <typeparam name="T">The entity type that implements <see cref="IIdentifiable"/> interface.</typeparam>
    /// <remarks>This interface defines the contract for data access operations including retrieval, creation, updating, and deletion of entities.
    /// All operations are asynchronous and support filtering, sorting, and pagination where applicable.</remarks>
    public interface IRepository<T>
        where T : class, IIdentifiable
    {
        /// <summary>
        /// Retrieves all items that match the specified list request criteria asynchronously.
        /// </summary>
        /// <remarks>Use this method to obtain a paged or filtered collection of items based on the
        /// parameters provided in the <paramref name="request"/> object. The operation is performed asynchronously and
        /// does not block the calling thread.</remarks>
        /// <param name="request">The criteria used to filter, sort, and page the items to be retrieved. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="ListResponse{T}"/>
        /// with the items matching the request criteria. The response may be empty if no items are found.</returns>
        Task<ListResponse<T>> GetAllAsync(ListRequest request);

        /// <summary>
        /// Retrieves a single entity of type T by its unique identifier asynchronously.
        /// </summary>
        /// <remarks>This method performs a lookup on the repository using the provided identifier. If no entity is found with the specified id,
        /// the response will contain appropriate information indicating the entity was not found.</remarks>
        /// <param name="id">The unique identifier (Guid) of the entity to retrieve. Cannot be an empty guid.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="RetrieveResponse{T}"/>
        /// with the entity if found, or an appropriate response indicating the entity was not found.</returns>
        Task<RetrieveResponse<T>> GetByIdAsync(Guid id);

        /// <summary>
        /// Adds a new entity of type T to the repository asynchronously.
        /// </summary>
        /// <remarks>This method inserts a new entity into the underlying data store. The entity should be properly initialized
        /// before calling this method. The operation is performed asynchronously.</remarks>
        /// <param name="entity">The entity instance to be added to the repository. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="SaveResponse"/>
        /// indicating the success or failure of the operation.</returns>
        Task<SaveResponse> AddAsync(T entity);

        /// <summary>
        /// Adds a list of entities of type T to the repository asynchronously.
        /// </summary>
        /// <remarks>This method batch inserts multiple entities into the underlying data store in a single operation.
        /// All entities should be properly initialized before calling this method. The operation is performed asynchronously.</remarks>
        /// <param name="entity">The list of entity instances to be added to the repository. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="SaveResponse"/>
        /// indicating the success or failure of the batch operation.</returns>
        Task<SaveResponse> AddRangeAsync(List<T> entity);

        /// <summary>
        /// Updates an existing entity of type T in the repository asynchronously.
        /// </summary>
        /// <remarks>This method modifies an existing entity in the underlying data store. The entity should already exist in the repository
        /// and contain the updated information. The operation is performed asynchronously.</remarks>
        /// <param name="entity">The entity instance containing the updated information. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="SaveResponse"/>
        /// indicating the success or failure of the update operation.</returns>
        Task<SaveResponse> UpdateAsync(T entity);

        /// <summary>
        /// Deletes an entity of type T from the repository based on its unique identifier asynchronously.
        /// </summary>
        /// <remarks>This method removes an entity from the underlying data store using the provided identifier.
        /// If no entity exists with the specified id, the response will contain appropriate information. The operation is performed asynchronously.</remarks>
        /// <param name="entity">The unique identifier (Guid) of the entity to be deleted. Cannot be an empty guid.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="DeleteReponse"/>
        /// indicating the success or failure of the deletion operation.</returns>
        Task<DeleteReponse> DeleteAsync(Guid entity);

        /// <summary>
        /// Retrieves a single entity of type T based on a specified predicate asynchronously.
        /// </summary>
        /// <remarks>This method queries the repository using a custom filter expression to find an entity matching the specified criteria.
        /// The predicate is an expression that defines the condition for selecting the entity. The operation is performed asynchronously.</remarks>
        /// <param name="predicate">A Lambda expression that defines the criteria for selecting the entity. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the entity matching the predicate criteria,
        /// or null if no matching entity is found.</returns>
        Task<T> GetByFieldNameAsync(Expression<Func<T, bool>> predicate);
    }
}