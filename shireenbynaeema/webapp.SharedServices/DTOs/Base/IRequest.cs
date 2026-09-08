namespace SharedServices
{
    /// <summary>
    /// Defines the contract for a save request targeting an entity.
    /// </summary>
    /// <remarks>
    /// Implement this interface to standardize save and update operations across different entity types.
    /// The <see cref="EntityId"/> property allows callers to specify an existing entity for updates,
    /// while <see cref="Entity"/> carries the data payload.
    /// </remarks>
    public interface ISaveRequest
    {
        /// <summary>
        /// Gets or sets the unique identifier of the entity to save.
        /// A <see langword="null"/> value indicates a new entity creation.
        /// </summary>
        Guid? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the entity data to persist.
        /// </summary>
        object Entity { get; set; }
    }

    /// <summary>
    /// Represents a request to save an entity, including its identifier and data.
    /// </summary>
    /// <remarks>Use this class to encapsulate the entity and its optional identifier when performing save operations.
    /// The entity data is provided via the <see cref="Entity"/> property, and the identifier can be specified if available.
    /// This class is typically used in data access or service layers to standardize save requests.</remarks>
    /// <typeparam name="TEntity">The type of the entity to be saved.</typeparam>
    public class SaveRequest<TEntity> : ISaveRequest
    {
        /// <summary>
        /// Gets or sets the unique identifier of the entity.
        /// A <see langword="null"/> value indicates a new entity that has not yet been persisted.
        /// </summary>
        public Guid? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the strongly-typed entity data to be saved.
        /// </summary>
        public TEntity Entity { get; set; } = default!;

        /// <inheritdoc />
        object ISaveRequest.Entity
        {
            get => this.Entity!;
            set => this.Entity = (TEntity)value;
        }
    }

    /// <summary>
    /// Represents a synchronization request that specifies whether the sync is an initial full sync
    /// or an incremental sync starting from a given timestamp.
    /// </summary>
    public class SyncRequest
    {
        /// <summary>
        /// Gets or sets a value indicating whether this request represents an initial (full) synchronization.
        /// When <see langword="true"/>, all data should be synchronized regardless of <see cref="FromTimestamp"/>.
        /// </summary>
        public bool IsInitialSync { get; set; }

        /// <summary>
        /// Gets or sets the row-version timestamp from which incremental synchronization should begin.
        /// A <see langword="null"/> value indicates that no lower bound is specified.
        /// </summary>
        public ulong? FromTimestamp { get; set; }
    }

    /// <summary>
    /// Represents a paginated list request with sorting and optional text search capabilities.
    /// </summary>
    /// <remarks>
    /// Default sort order is descending by <c>CreatedOnUnix</c>. Use <see cref="OrderBy"/> and
    /// <see cref="SortBy"/> to override. The computed <see cref="Skip"/> and <see cref="Take"/>
    /// properties can be passed directly to LINQ or SQL queries for pagination.
    /// </remarks>
    public class ListRequest
    {
        private const string DefaultOrderBy = "CreatedOnUnix";
        private const string DefaultSortBy = "desc";

        /// <summary>
        /// Initializes a new instance of the <see cref="ListRequest"/> class with default values.
        /// </summary>
        public ListRequest()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ListRequest"/> class with the specified pagination parameters.
        /// </summary>
        /// <param name="pageNumber">The zero-based page index.</param>
        /// <param name="pageSize">The number of items per page.</param>
        public ListRequest(int pageNumber, int pageSize)
        {
            this.PageNumber = pageNumber;
            this.PageSize = pageSize;
        }

        /// <summary>
        /// Gets or sets the zero-based page index.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Gets or sets the number of items per page. Defaults to <c>10</c>.
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Gets or sets the property name used for ordering results.
        /// Defaults to <c>"CreatedOnUnix"</c> when not specified or whitespace.
        /// </summary>
        public string OrderBy
        {
            get => string.IsNullOrWhiteSpace(field) ? DefaultOrderBy : field;
            set => field = string.IsNullOrWhiteSpace(value) ? DefaultOrderBy : value;
        }

        /// <summary>
        /// Gets or sets the sort direction (<c>"asc"</c> or <c>"desc"</c>).
        /// Defaults to <c>"desc"</c> when not specified or whitespace.
        /// </summary>
        public string SortBy
        {
            get => string.IsNullOrWhiteSpace(field) ? DefaultSortBy : field;
            set => field = string.IsNullOrWhiteSpace(value) ? DefaultSortBy : value;
        }

        /// <summary>
        /// Gets the number of items to skip, computed as <see cref="PageNumber"/> × <see cref="PageSize"/>.
        /// </summary>
        public int Skip => this.PageNumber * this.PageSize;

        /// <summary>
        /// Gets the number of items to take, equivalent to <see cref="PageSize"/>.
        /// </summary>
        public int Take => this.PageSize;

        /// <summary>
        /// Gets or sets an optional free-text search string used to filter results.
        /// </summary>
        public string? SearchText { get; set; }
    }

    /// <summary>
    /// Represents a lookup list request that extends <see cref="ListRequest"/> with pre-selected values
    /// and a create-mode indicator.
    /// </summary>
    /// <remarks>
    /// Use <see cref="SelectedValues"/> to pass previously chosen <see cref="Guid"/> identifiers so
    /// they can be highlighted or prioritized in the result set.
    /// </remarks>
    public class LookupListRequest : ListRequest
    {
        /// <summary>
        /// Gets or sets the collection of previously selected entity identifiers.
        /// </summary>
        public List<Guid>? SelectedValues { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the lookup is being used in a create context.
        /// </summary>
        public bool? IsCreateMode { get; set; }
    }

    /// <summary>
    /// Represents a dashboard-specific lookup list request that extends <see cref="ListRequest"/>
    /// with integer-based selected values and a create-mode indicator.
    /// </summary>
    /// <remarks>
    /// Similar to <see cref="LookupListRequest"/> but uses <see cref="int"/> identifiers instead
    /// of <see cref="Guid"/> values, suitable for dashboard KPI or metric selections.
    /// </remarks>
    public class DashboardLookupListRequest : ListRequest
    {
        /// <summary>
        /// Gets or sets the collection of previously selected integer identifiers.
        /// </summary>
        public List<int>? SelectedValues { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the lookup is being used in a create context.
        /// </summary>
        public bool? IsCreateMode { get; set; }
    }

    /// <summary>
    /// Represents a request to retrieve a single entity by its identifier.
    /// </summary>
    public class RetrieveRequest
    {
        /// <summary>
        /// Gets or sets the identifier of the entity to retrieve.
        /// </summary>
        public object EntityId { get; set; } = new object();
    }

    /// <summary>
    /// Represents a request to send a One-Time Password (OTP) to a user's email address.
    /// </summary>
    public class OtpRequestModel
    {
        /// <summary>
        /// Gets or sets the email address to which the OTP should be delivered.
        /// </summary>
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a request to verify a One-Time Password (OTP), including the target email,
    /// the OTP code, and whether the session should be persisted.
    /// </summary>
    public class OtpVerifyModel
    {
        /// <summary>
        /// Gets or sets the email address associated with the OTP.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the OTP code to verify.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the user's session should be persisted after verification.
        /// </summary>
        public bool RememberMe { get; set; }
    }

    /// <summary>
    /// Represents a request to reset a user's password, including the new password and its confirmation.
    /// </summary>
    public class PasswordResetRequest
    {
        /// <summary>
        /// Gets or sets the email address of the user whose password is being reset.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the new password.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the confirmation of the new password. Must match <see cref="Password"/>.
        /// </summary>
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a request to send a password-reset email to the specified address.
    /// </summary>
    public class PasswordResetEmailRequest
    {
        /// <summary>
        /// Gets or sets the email address to which the password reset instructions should be sent.
        /// </summary>
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a request to verify a token associated with a user's email address.
    /// </summary>
    public class TokenVerificationRequest
    {
        /// <summary>
        /// Gets or sets the email address associated with the token.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the token string to verify.
        /// </summary>
        public string Token { get; set; } = string.Empty;
    }

    /// <summary>
    /// Extends <see cref="TokenVerificationRequest"/> to include a work order identifier,
    /// enabling token verification in the context of a vendor work order.
    /// </summary>
    public class VendorWorkOrderTokenVerificationRequest : TokenVerificationRequest
    {
        /// <summary>
        /// Gets or sets the unique identifier of the work order associated with the vendor token.
        /// </summary>
        public Guid WorkOrderId { get; set; }
    }

    /// <summary>
    /// Represents a request to activate or deactivate one or more users.
    /// </summary>
    public class ActiveInActiveUserRequest
    {
        /// <summary>
        /// Gets or sets the list of user identifiers to activate or deactivate.
        /// </summary>
        public List<Guid> Users { get; set; } = [];

        /// <summary>
        /// Gets or sets the activation flag. A non-zero value typically indicates activation;
        /// zero indicates deactivation.
        /// </summary>
        public int Flag { get; set; }
    }

    /// <summary>
    /// Represents a generic request to activate or deactivate one or more entities by their identifiers.
    /// </summary>
    /// <remarks>
    /// This is a general-purpose counterpart to <see cref="ActiveInActiveUserRequest"/> and can be
    /// used for any entity type that supports activation toggling.
    /// </remarks>
    public class ActiveInActiveRequest
    {
        /// <summary>
        /// Gets or sets the list of entity identifiers to activate or deactivate.
        /// </summary>
        public List<Guid> Ids { get; set; } = [];

        /// <summary>
        /// Gets or sets the activation flag. A non-zero value typically indicates activation;
        /// zero indicates deactivation.
        /// </summary>
        public StatusType Flag { get; set; }
    }

    /// <summary>
    /// Represents a request to invite multiple users in bulk, optionally resending previously issued invitations.
    /// </summary>
    public class BulkUserInviteRequest
    {
        /// <summary>
        /// Gets or sets a value indicating whether this request is resending previously issued invitations.
        /// </summary>
        public bool IsResendInvite { get; set; }

        /// <summary>
        /// Gets or sets the collection of user invite entries to process.
        /// </summary>
        public List<UserInviteEntry> Users { get; set; } = [];
    }

    /// <summary>
    /// Represents the result of a bulk user invite operation, including success and failure counts.
    /// </summary>
    public class BulkUserInviteResponse
    {
        /// <summary>
        /// Gets or sets the total number of users targeted for invitation.
        /// </summary>
        public int Invited { get; set; }

        /// <summary>
        /// Gets or sets the number of invitations that were sent successfully.
        /// </summary>
        public int Success { get; set; }

        /// <summary>
        /// Gets or sets the number of invitations that failed to send.
        /// </summary>
        public int Failed { get; set; }
    }

    /// <summary>
    /// Represents a single entry in a bulk user invite, specifying the target email, optional
    /// organization, and assigned roles.
    /// </summary>
    public class UserInviteEntry
    {
        /// <summary>
        /// Gets or sets the email address of the user to invite.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the optional organization identifier the user should be associated with.
        /// </summary>
        public Guid? OrganizationId { get; set; } = Guid.Empty;

        /// <summary>
        /// Gets or sets the list of role identifiers to assign to the invited user.
        /// </summary>
        public List<Guid> Roles { get; set; } = [];
    }

    /// <summary>
    /// Represents a dashboard response containing a collection of Key Performance Indicator (KPI) entries.
    /// </summary>
    /// <remarks>
    /// Each KPI is represented as a string-keyed dictionary, allowing flexible and dynamic
    /// key-value pairs for different dashboard metrics.
    /// </remarks>
    public class DashboardResponse
    {
        /// <summary>
        /// Gets or sets the list of dashboard KPI entries, where each entry is a dictionary of metric names to values.
        /// </summary>
        public List<Dictionary<string, string>> DashboardKPIs { get; set; } = [];
    }

    /// <summary>
    /// Represents a geographic location specified by latitude and longitude coordinates.
    /// </summary>
    public class LocationRequest
    {
        /// <summary>
        /// Gets or sets the latitude coordinate in decimal degrees. A <see langword="null"/> value indicates the latitude is unspecified.
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude coordinate in decimal degrees. A <see langword="null"/> value indicates the longitude is unspecified.
        /// </summary>
        public double? Longitude { get; set; }
    }

    /// <summary>
    /// Represents a request to link a vendor (identified by email) to a specific work order.
    /// </summary>
    public class VendorLinkRequest
    {
        /// <summary>
        /// Gets or sets the email address of the vendor to link.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the unique identifier of the work order to associate with the vendor.
        /// </summary>
        public Guid WorkOrderId { get; set; }
    }
}