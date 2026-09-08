namespace Domain
{
    /// <summary>
    /// Stores history of emails sent by the system for audit and troubleshooting purposes.
    /// </summary>
    public class EmailLog : Auditable, IIdentifiable
    {
        /// <summary>Gets or sets primary identifier of the email log record.</summary>
        public Guid Id { get; set; }

        /// <summary>Gets or sets recipient email address.</summary>
        public string ToEmail { get; set; } = string.Empty;

        /// <summary>Gets or sets sender email address.</summary>
        public string FromEmail { get; set; } = string.Empty;

        /// <summary>Gets or sets subject line of the email.</summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>Gets or sets comma-separated list of CC recipient email addresses.</summary>
        public string? CCs { get; set; } = string.Empty; // Comma separated list of CC emails
    }
}