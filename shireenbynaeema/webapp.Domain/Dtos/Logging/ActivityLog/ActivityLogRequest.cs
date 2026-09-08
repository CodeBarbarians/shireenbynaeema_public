namespace Domain
{
    using SharedServices;

    /// <summary>
    /// Request model used to filter activity logs.
    /// Supports filtering by user, date range, action type, module, status, and search text.
    /// </summary>
    public class ActivityLogFilterRequest : ListRequest
    {
        /// <summary>
        /// Gets or sets filters logs for a specific user.
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Gets or sets the starting day of the range, or null if no lower bound is specified.
        /// </summary>
        public int? FromDay { get; set; }

        /// <summary>
        /// Gets or sets the ending day of the range, or null if no upper bound is specified.
        /// </summary>
        public int? ToDay { get; set; }

        /// <summary>
        /// Gets or sets the starting month of the range, or null if no lower bound is specified.
        /// </summary>
        public int? FromMonth { get; set; }

        /// <summary>
        /// Gets or sets the starting year of the range, or null if no lower bound is specified.
        /// </summary>
        public int? FromYear { get; set; }

        /// <summary>
        /// Gets or sets the ending month of the range, or null if no upper bound is specified.
        /// </summary>
        public int? ToMonth { get; set; }

        /// <summary>
        /// Gets or sets the ending year of the range, or null if no upper bound is specified.
        /// </summary>
        public int? ToYear { get; set; }

        /// <summary>
        /// Gets or sets filters logs by action types (e.g., Login, RoleUpdated, PasswordChanged).
        /// </summary>
        public List<string>? Actions { get; set; }

        /// <summary>
        /// Gets or sets filters logs by modules (e.g., Authentication, RoleManagement, Security).
        /// </summary>
        public List<string>? Modules { get; set; }

        /// <summary>
        /// Gets or sets filters logs by status (e.g., Success, Failed, Update, Revert).
        /// </summary>
        public List<string>? Statuses { get; set; }
    }
}