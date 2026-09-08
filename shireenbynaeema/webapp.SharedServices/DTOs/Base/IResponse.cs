namespace SharedServices
{
using System.Collections;
using System.ComponentModel;
using System.Text.Json.Serialization;

/// <summary>
/// Contract for paginated list responses.
/// </summary>
public interface IListResponse
{
    /// <summary>
    /// Gets the collection of entities for the current page.
    /// </summary>
    IList Entities { get; }

    /// <summary>
    /// Gets the total number of records available across all pages.
    /// </summary>
    int TotalCount { get; }

    /// <summary>
    /// Gets the current page number (1-based).
    /// </summary>
    int PageNumber { get; }

    /// <summary>
    /// Gets the maximum number of records per page.
    /// </summary>
    int PageSize { get; }
}

/// <summary>
/// Contract for single entity retrieval response.
/// </summary>
public interface IRetrieveResponse
{
    /// <summary>
    /// Gets the retrieved entity, or <c>null</c> if no entity was found.
    /// </summary>
    object? Entity { get; }
}

/// <summary>
/// Generic API response contract used by controllers.
/// </summary>
public interface IResponse
{
    /// <summary>
    /// Gets or sets the response payload data. Omitted from JSON when <c>null</c>.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    object? Data { get; set; }

    /// <summary>
    /// Gets or sets an optional status or error code. Omitted from JSON when <c>null</c>.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    object? Code { get; set; }

    /// <summary>
    /// Gets or sets response message (success or failure description).
    /// </summary>
    object? Message { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether indicates whether the request was successful.
    /// </summary>
    bool IsSuccess { get; set; }
}

/// <summary>
/// Represents a standardized error payload returned from services.
/// Extracts basic information from an exception.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="Error"/> class from an exception.
/// </remarks>
/// <param name="ex">The exception that occurred.</param>
public class Error(Exception ex)
{
    /// <summary>
    /// Gets or sets exception type name used as error code.
    /// </summary>
    public string? Code { get; set; } = ex.GetType().Name;

    /// <summary>
    /// Gets or sets human readable error message.
    /// </summary>
    public string? Message { get; set; } = ex.Message;
}

/// <summary>
/// Base service response containing error information.
/// </summary>
public class ServiceResponse
{
    /// <summary>
    /// Gets or sets error details if the operation failed.
    /// </summary>
    public Error? Error { get; set; }
}

/// <summary>
/// Response returned after create/update operations.
/// </summary>
public class SaveResponse : ServiceResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SaveResponse"/> class with the specified save status and optional exception.
    /// information.
    /// </summary>
    /// <param name="isSaved">true if the save operation was successful; otherwise, false. The default is false.</param>
    /// <param name="exception">An optional exception that occurred during the save operation. If not null, the Error property will be set based
    /// on this exception.</param>
    public SaveResponse(bool isSaved = false, Exception? exception = null)
    {
        this.IsSuccess = isSaved;
        if (exception != null)
        {
            this.Error = new Error(exception);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether indicates whether the save operation succeeded.
    /// </summary>
    public bool IsSuccess { get; set; }
}

/// <summary>
/// Represents a paginated response containing a collection of entities and related paging information.
/// </summary>
/// <remarks>Use this class to return lists of items from a service, along with pagination metadata such as the
/// current page and total record count. The paging properties allow clients to implement efficient data navigation and
/// display. The collection of entities may be empty if no records match the query criteria.</remarks>
/// <typeparam name="T">The type of the entities included in the response.</typeparam>
public class ListResponse<T> : ServiceResponse, IListResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ListResponse{T}"/> class with the specified entities, pagination
    /// parameters, and optional exception.
    /// </summary>
    /// <param name="entities">The list of entities for the current page, or <c>null</c> for an empty list.</param>
    /// <param name="pageNumber">The current page number (1-based). The default is 1.</param>
    /// <param name="pageSize">The maximum number of records per page. The default is 10.</param>
    /// <param name="totalCount">The total number of records available across all pages. The default is 0.</param>
    /// <param name="exception">An optional exception that occurred during the operation. If not <c>null</c>, the
    /// <see cref="ServiceResponse.Error"/> property will be populated.</param>
    public ListResponse(List<T>? entities = null, int pageNumber = 1, int pageSize = 10, long totalCount = 0, Exception? exception = null)
    {
        this.Entities = entities ?? [];
        this.TotalCount = Convert.ToInt32(totalCount);
        this.PageNumber = pageNumber;
        this.PageSize = pageSize;

        if (exception != null)
        {
            this.Error = new Error(exception);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ListResponse{T}"/> class with default values.
    /// </summary>
    public ListResponse()
    {
    }

    IList IListResponse.Entities => this.Entities;

    /// <summary>
    /// Gets or sets collection of returned records.
    /// </summary>
    public List<T> Entities { get; set; } = [];

    /// <summary>
    /// Gets or sets total number of records available (ignoring pagination).
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets current page number.
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Gets or sets number of records per page.
    /// </summary>
    public int PageSize { get; set; }
}

/// <summary>
/// Represents the response returned from a retrieve operation, containing the retrieved entity and any associated error
/// information.
/// </summary>
/// <remarks>Use this class to access the result of a retrieve request, including the entity and any error that
/// occurred during the operation. The entity may be null if the operation did not succeed or if no entity was
/// found.</remarks>
/// <typeparam name="T">The type of the entity returned by the retrieve operation.</typeparam>
public class RetrieveResponse<T> : ServiceResponse, IRetrieveResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RetrieveResponse{T}"/> class with the specified entity and
    /// optional exception.
    /// </summary>
    /// <param name="entity">The retrieved entity, or <c>null</c> if no entity was found.</param>
    /// <param name="exception">An optional exception that occurred during the retrieve operation. If not <c>null</c>,
    /// the <see cref="ServiceResponse.Error"/> property will be populated.</param>
    public RetrieveResponse(T? entity, Exception? exception = null)
    {
        this.Entity = entity;
        if (exception != null)
        {
            this.Error = new Error(exception);
        }
    }

    /// <summary>
    /// Gets or sets retrieved entity.
    /// </summary>
    public T? Entity { get; set; }

    object? IRetrieveResponse.Entity => this.Entity;
}

/// <summary>
/// Response returned after delete operations.
/// </summary>
public class DeleteReponse : ServiceResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteReponse"/> class with the specified deletion status and
    /// optional exception.
    /// </summary>
    /// <param name="isDeleted"><c>true</c> if the delete operation was successful; otherwise, <c>false</c>.
    /// Automatically set to <c>false</c> when an exception is provided.</param>
    /// <param name="exception">An optional exception that occurred during the delete operation. If not <c>null</c>,
    /// the <see cref="ServiceResponse.Error"/> property will be populated and <see cref="IsDeleted"/> will be
    /// set to <c>false</c>.</param>
    public DeleteReponse(bool isDeleted, Exception? exception = null)
    {
        if (exception != null)
        {
            this.Error = new Error(exception);
            this.IsDeleted = false;
        }
        else
        {
            this.IsDeleted = isDeleted;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether indicates whether deletion succeeded.
    /// </summary>
    public bool IsDeleted { get; set; }
}

/// <summary>
/// Response returned after file/attachment upload operations.
/// </summary>
public class AttachmentResponse : ServiceResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AttachmentResponse"/> class with the specified upload status
    /// and optional exception.
    /// </summary>
    /// <param name="isSaved"><c>true</c> if the upload was successful; otherwise, <c>false</c>. The default is
    /// <c>false</c>.</param>
    /// <param name="exception">An optional exception that occurred during the upload operation. If not <c>null</c>,
    /// the <see cref="ServiceResponse.Error"/> property will be populated.</param>
    public AttachmentResponse(bool isSaved = false, Exception? exception = null)
    {
        this.IsSuccess = isSaved;
        if (exception != null)
        {
            this.Error = new Error(exception);
        }
    }

    /// <summary>
    /// Gets or sets preview URL (thumbnail or temporary access link).
    /// </summary>
    public string? PreviewUrl { get; set; }

    /// <summary>
    /// Gets or sets final file URL.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets indicates whether upload succeeded.
    /// </summary>
    public bool? IsSuccess { get; set; }
}

/// <summary>
/// Standard API response wrapper used by endpoints.
/// </summary>
public class Response : IResponse
{
    /// <summary>
    /// Gets or sets the response payload data. Omitted from JSON when <c>null</c>.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Data { get; set; }

    /// <summary>
    /// Gets or sets an optional status or error code. Omitted from JSON when <c>null</c>.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Code { get; set; }

    /// <summary>
    /// Gets or sets the response message describing the outcome of the request.
    /// </summary>
    public object? Message { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the request completed successfully.
    /// </summary>
    public bool IsSuccess { get; set; }
}

/// <summary>
/// Base class for listing DTOs containing audit creation metadata.
/// </summary>
public class Base_Listing
{
    /// <summary>
    /// Gets or sets combined formatted creation date and time.
    /// </summary>
    [DisplayName("Created On")]
    public string? CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets creation date portion.
    /// </summary>
    [DisplayName("Created On Date")]
    public string? CreatedOnDate { get; set; }

    /// <summary>
    /// Gets or sets creation time portion.
    /// </summary>
    [DisplayName("Created On Time")]
    public string? CreatedOnTime { get; set; }

    /// <summary>
    /// Gets or sets creation timestamp in Unix epoch seconds.
    /// </summary>
    [DisplayName("Created On (Unix)")]
    public long? CreatedOnUnix { get; set; }
}
}