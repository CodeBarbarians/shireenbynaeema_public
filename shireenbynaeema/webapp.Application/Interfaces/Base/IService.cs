namespace Application
{
    using System.Linq.Expressions;

    using SharedServices;

    /// <summary>
    /// Generic service interface for handling common operations on entities of type T. This interface defines a set of methods for performing CRUD (Create, Read, Update, Delete)
    /// operations, as well as retrieving entities based on specific criteria. The methods are designed to work with asynchronous programming patterns,
    /// allowing for efficient handling of data operations in a non-blocking manner.
    /// Implementing this interface allows for consistent and reusable service logic across different types of entities in the application.
    /// </summary>
    /// <typeparam name="T">The entity type that this service operates on. Must be a reference type.</typeparam>
    public interface IService<T>
        where T : class
    {
        /// <summary>
        /// Gets the name of the module associated with this service. This property is typically used for logging, error handling,
        /// and providing context in responses. It allows the service to identify itself and its operations, making it easier to trace actions
        /// and issues back to the specific module or area of the application that is responsible for them.
        /// </summary>
        string ModuleName { get; }

        /// <summary>
        /// Gets the display name of the module associated with this service. This property is used for user-friendly representations of the
        /// module in logs, error messages, and responses.
        /// </summary>
        string ModuleDisplayName { get; }

        /// <summary>
        /// Gets the type of the status enumeration associated with this service. This property is used to define the specific status codes
        /// and messages that are relevant to the operations of this service.
        /// </summary>
        Type StatusEnumType { get; }

        /// <summary>
        /// Retrieves a paginated or filtered list of all entities of type T based on the provided request criteria.
        /// </summary>
        /// <param name="request">A <see cref="ListRequest"/> object containing pagination, filtering, and sorting information.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IResponse"/> with the list of entities or an error response.</returns>
        Task<IResponse> GetAllAsync(ListRequest request);

        /// <summary>
        /// Retrieves a single entity of type T by its unique identifier (ID).
        /// </summary>
        /// <param name="id">The unique identifier of the entity to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IResponse"/> with the entity or an error response if not found.</returns>
        Task<IResponse> GetByIdAsync(Guid id);

        /// <summary>
        /// Adds a new entity of type T to the data store. This method takes a SaveRequest object that contains the entity to be added, along with any necessary metadata.
        /// </summary>
        /// <param name="request">A <see cref="SaveRequest{T}"/> object containing the entity and metadata required for creation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IResponse"/> indicating success or failure.</returns>
        Task<IResponse> AddAsync(SaveRequest<T> request);

        /// <summary>
        /// Adds a batch of new entities of type T to the data store. This method takes a list of entities to be added, allowing for batch operations
        /// and improving efficiency when multiple entities need to be created at once.
        /// </summary>
        /// <param name="entity">A collection of entities to be added to the data store.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IResponse"/> indicating the result of the batch operation.</returns>
        Task<IResponse> AddRangeAsync(List<T> entity);

        /// <summary>
        /// Updates an existing entity of type T in the data store. This method takes a SaveRequest object that contains the updated entity,
        /// along with any necessary metadata.
        /// </summary>
        /// <param name="request">A <see cref="SaveRequest{T}"/> object containing the entity with updated values and metadata.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IResponse"/> indicating success or failure.</returns>
        Task<IResponse> UpdateAsync(SaveRequest<T> request);

        /// <summary>
        /// Deletes an entity of type T from the data store based on its unique identifier (ID).
        /// </summary>
        /// <param name="id">The unique identifier of the entity to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IResponse"/> indicating success or failure.</returns>
        Task<IResponse> DeleteAsync(Guid id);

        /// <summary>
        /// Retrieves an entity of type T based on a specified predicate condition. This method allows for retrieving an entity by matching a specific field's value.
        /// </summary>
        /// <param name="predicate">An expression that defines the condition to match against entities of type T.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IResponse"/> with the matching entity or an error response if not found.</returns>
        Task<IResponse> GetByFieldNameAsync(Expression<Func<T, bool>> predicate);
    }
}