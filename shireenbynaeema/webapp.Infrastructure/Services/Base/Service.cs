namespace Infrastructure
{
    using System.Linq.Expressions;

    using Application;
    using Domain;
    using SharedServices;

    /// <summary>
    /// Provides extension methods for converting entities and lists of entities into standardized request and response
    /// objects for repository operations.
    /// </summary>
    /// <remarks>The methods in this class simplify the process of wrapping entities and collections in
    /// request and response types commonly used for saving, updating, listing, and retrieving data. These extensions
    /// help ensure consistent API patterns and reduce boilerplate code when interacting with repositories. All methods
    /// are static and intended for use with entity types and repository request/response models.</remarks>
    public static class RequestExtensions
    {
        /// <summary>
        /// Converts an entity of type <typeparamref name="T"/> into a <see cref="SaveRequest{T}"/> object.
        /// This allows the entity to be used directly for saving or updating in the repository.
        /// </summary>
        /// <typeparam name="T">The type of the entity being converted.</typeparam>
        /// <param name="entity">The entity instance to wrap in a <see cref="SaveRequest{T}"/>.</param>
        /// <returns>A <see cref="SaveRequest{T}"/> containing the provided entity.</returns>
        public static SaveRequest<T> ToSaveRequest<T>(this T entity)
        {
            return new SaveRequest<T> { Entity = entity };
        }

        /// <summary>
        /// Converts an entity of type <typeparamref name="T"/> along with its unique identifier (GUID)
        /// into a <see cref="SaveRequest{T}"/> object, suitable for updating the entity in the repository.
        /// </summary>
        /// <typeparam name="T">The type of the entity being converted.</typeparam>
        /// <param name="entity">The entity instance to wrap in a <see cref="SaveRequest{T}"/>.</param>
        /// <param name="id">The unique identifier (GUID) of the entity to be updated.</param>
        /// <returns>A <see cref="SaveRequest{T}"/> containing the entity and its ID for update operations.</returns>
        public static SaveRequest<T> ToUpdateRequest<T>(this T entity, Guid? id)
        {
            return new SaveRequest<T> { EntityId = id, Entity = entity };
        }

        /// <summary>
        /// Converts a list of entities of type <typeparamref name="T"/> into a <see cref="ListResponse{T}"/>,
        /// including pagination details from the provided <see cref="ListRequest"/> and the total count of items.
        /// </summary>
        /// <typeparam name="T">The type of the entities in the list.</typeparam>
        /// <param name="result">The list of entities to include in the response.</param>
        /// <param name="request">The <see cref="ListRequest"/> containing pagination parameters such as page number and page size.</param>
        /// <param name="totalCount">The total number of entities available for pagination.</param>
        /// <returns>A <see cref="ListResponse{T}"/> containing the entities, pagination info, and total count.</returns>
        public static ListResponse<T> ToListResponse<T>(this List<T> result, ListRequest request, int totalCount)
        {
            return new ListResponse<T>(pageNumber: request.PageNumber, pageSize: request.PageSize)
            {
                Entities = result,
                TotalCount = totalCount,
            };
        }

        /// <summary>
        /// Converts a list of entities of type <typeparamref name="T"/> into a <see cref="ListResponse{T}"/>,
        /// including the total count of items for pagination purposes.
        /// </summary>
        /// <typeparam name="T">The type of the entities in the list.</typeparam>
        /// <param name="result">The list of entities to include in the response.</param>
        /// <param name="totalCount">The total number of entities available.</param>
        /// <returns>A <see cref="ListResponse{T}"/> containing the entities and total count.</returns>
        public static ListResponse<T> ToListResponse<T>(this List<T> result, int totalCount)
        {
            return new ListResponse<T>()
            {
                Entities = result,
                TotalCount = totalCount,
            };
        }

        /// <summary>
        /// Converts an entity of type <typeparamref name="T"/> into a <see cref="RetrieveResponse{T}"/>,
        /// which standardizes the response format for retrieval operations.
        /// </summary>
        /// <typeparam name="T">The type of the entity being wrapped.</typeparam>
        /// <param name="response">The entity to wrap in a <see cref="RetrieveResponse{T}"/>.</param>
        /// <returns>A <see cref="RetrieveResponse{T}"/> containing the provided entity.</returns>
        public static RetrieveResponse<T> ToRetrieveResponse<T>(this T response)
        {
            return new RetrieveResponse<T>(response);
        }
    }

    /// <summary>
    /// Provides a generic service implementation for managing entities of type T, including standard CRUD operations
    /// and response formatting.
    /// </summary>
    /// <remarks>This service class offers asynchronous methods for adding, updating, deleting, and retrieving
    /// entities, with responses formatted using module-specific messages. It is designed to work with repositories that
    /// support the IIdentifiable interface and provides consistent response handling for client applications. Thread
    /// safety depends on the underlying repository and response implementations.</remarks>
    /// <typeparam name="T">The entity type managed by the service. Must be a class that implements the IIdentifiable interface.</typeparam>
    /// <param name="repository">The repository used for data access operations on entities of type T.</param>
    /// <param name="response">The response object used to standardize operation results and messages.</param>
    public class Service<T>(IRepository<T> repository, IResponse response) : IService<T>
        where T : class, IIdentifiable
    {
        /// <summary>
        /// Gets the name of the module type represented by the generic parameter T.
        /// </summary>
        public string ModuleName => typeof(T).Name;

        /// <summary>
        /// Gets the display name of the module type represented by <typeparamref name="T"/>.
        /// </summary>
        public string ModuleDisplayName => typeof(T).GetEntityDisplayName();

        /// <summary>
        /// Gets the <see cref="Type"/> object representing the status enumeration type used by this class.
        /// </summary>
        public Type StatusEnumType => typeof(StatusType);

        /// <summary>
        /// Adds a new entity of type <typeparamref name="T"/> to the underlying repository and returns a standardized response.
        /// </summary>
        /// <param name="request">A <see cref="SaveRequest{T}"/> containing the entity to be added.</param>
        /// <returns>
        /// An <see cref="IResponse"/> indicating the success or failure of the add operation, including a message formatted with the module name.
        /// </returns>
        public virtual async Task<IResponse> AddAsync(SaveRequest<T> request)
        {
            var resp = await repository.AddAsync(request.Entity).ConfigureAwait(false);
            response.Message = resp.IsSuccess == Constants.ResponseSuccess ? Constants.SaveSuccess.FormatWith(this.ModuleDisplayName) : Constants.SaveFailed.FormatWith(this.ModuleDisplayName);
            response.IsSuccess = resp.IsSuccess;
            response.Data = request.Entity.Id;
            return response;
        }

        /// <summary>
        /// Adds multiple entities of type <typeparamref name="T"/> to the repository in a single operation and returns a standardized response.
        /// </summary>
        /// <param name="entity">A list of entities to be added.</param>
        /// <returns>
        /// An <see cref="IResponse"/> indicating the success or failure of the bulk add operation, including a message formatted with the module name.
        /// </returns>
        public virtual async Task<IResponse> AddRangeAsync(List<T> entity)
        {
            var resp = await repository.AddRangeAsync(entity).ConfigureAwait(false);
            response.Message = resp.IsSuccess == Constants.ResponseSuccess ? Constants.SaveSuccess.FormatWith(this.ModuleDisplayName) : Constants.SaveFailed.FormatWith(this.ModuleDisplayName);
            response.IsSuccess = resp.IsSuccess;
            return response;
        }

        /// <summary>
        /// Deletes an entity of type <typeparamref name="T"/> from the repository using its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier (GUID) of the entity to be deleted.</param>
        /// <returns>
        /// An <see cref="IResponse"/> indicating whether the deletion was successful, including a message formatted with the module name.
        /// </returns>
        public virtual async Task<IResponse> DeleteAsync(Guid id)
        {
            var resp = await repository.DeleteAsync(id).ConfigureAwait(false);
            response.Message = resp.IsDeleted == Constants.ResponseSuccess ? Constants.DeleteSuccess.FormatWith(this.ModuleDisplayName) : Constants.DeleteFailed.FormatWith(this.ModuleDisplayName);
            response.IsSuccess = resp.IsDeleted;
            return response;
        }

        /// <summary>
        /// Retrieves a paginated list of entities of type <typeparamref name="T"/> from the repository using the specified <see cref="ListRequest"/> parameters.
        /// </summary>
        /// <param name="request">The request object containing pagination, filtering, and sorting information.</param>
        /// <returns>
        /// An <see cref="IResponse"/> containing the list of entities, a success flag, and a message indicating the result of the operation.
        /// </returns>
        public virtual async Task<IResponse> GetAllAsync(ListRequest request)
        {
            var resp = await repository.GetAllAsync(request).ConfigureAwait(false);
            response.Message = resp != null ? Constants.ListSuccess.FormatWith(this.ModuleDisplayName) : Constants.ListFailed.FormatWith(this.ModuleDisplayName);
            response.IsSuccess = resp != null ? true : false;
            response.Data = resp;
            return response;
        }

        /// <summary>
        /// Retrieves a single entity of type <typeparamref name="T"/> from the repository that matches the specified predicate expression.
        /// </summary>
        /// <param name="predicate">A LINQ expression used to filter the entity based on a specific field or condition.</param>
        /// <returns>
        /// An <see cref="IResponse"/> containing the matched entity (if any), a success flag, and a message indicating the result of the retrieval operation.
        /// </returns>
        public virtual async Task<IResponse> GetByFieldNameAsync(Expression<Func<T, bool>> predicate)
        {
            var resp = await repository.GetByFieldNameAsync(predicate).ConfigureAwait(false);
            response.Message = resp != null ? Constants.RetrieveSuccess.FormatWith(this.ModuleDisplayName) : Constants.RetrieveFailed.FormatWith(this.ModuleDisplayName);
            response.IsSuccess = resp != null ? true : false;
            response.Data = resp;
            return response;
        }

        /// <summary>
        /// Retrieves a single entity of type <typeparamref name="T"/> from the repository based on its unique identifier (GUID).
        /// </summary>
        /// <param name="id">The unique identifier of the entity to retrieve.</param>
        /// <returns>
        /// An <see cref="IResponse"/> containing the entity if found, a success flag, and a message indicating whether the retrieval was successful.
        /// </returns>
        public virtual async Task<IResponse> GetByIdAsync(Guid id)
        {
            var resp = await repository.GetByIdAsync(id).ConfigureAwait(false);
            response.Data = resp;
            response.Message = resp != null ? Constants.RetrieveSuccess.FormatWith(this.ModuleDisplayName) : Constants.RetrieveFailed.FormatWith(this.ModuleDisplayName);
            response.IsSuccess = resp != null ? true : false;
            return response;
        }

        /// <summary>
        /// Updates an existing entity of type <typeparamref name="T"/> in the repository using the provided <see cref="SaveRequest{T}"/>.
        /// Sets the entity's ID from the request and calls the repository's update method.
        /// Returns a standardized <see cref="IResponse"/> indicating the success or failure of the operation.
        /// </summary>
        /// <param name="request">The <see cref="SaveRequest{T}"/> containing the entity to update and its ID.</param>
        /// <returns>
        /// An <see cref="IResponse"/> with a success flag and a message describing whether the update was successful.
        /// </returns>
        public virtual async Task<IResponse> UpdateAsync(SaveRequest<T> request)
        {
            request.Entity.Id = (Guid)request.EntityId!;
            var resp = await repository.UpdateAsync(request.Entity).ConfigureAwait(false);
            response.Message = resp.IsSuccess == Constants.ResponseSuccess ? Constants.UpdateSuccess.FormatWith(this.ModuleDisplayName) : Constants.UpdateFailed.FormatWith(this.ModuleDisplayName);
            response.IsSuccess = resp.IsSuccess;
            return response;
        }
    }
}