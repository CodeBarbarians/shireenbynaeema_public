namespace Domain
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using SharedServices;

    /// <summary>
    /// Stores application exceptions for diagnostics and monitoring.
    /// </summary>
    [Table("ExceptionLog")]
    [EntityDisplayName("Exception Log")]
    public class ExceptionLog : IIdentifiable
    {
        /// <summary>Gets or sets primary identifier of the exception record.</summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>Gets or sets date and time when the exception was logged.</summary>
        [Required]
        public DateTime Logged { get; set; }

        /// <summary>Gets or sets severity level of the exception (e.g., Error, Warning, Info).</summary>
        [Required]
        [MaxLength(50)]
        public string Level { get; set; } = string.Empty;

        /// <summary>Gets or sets exception message describing the error.</summary>
        [Required]
        [MaxLength(4000)]
        public string Message { get; set; } = string.Empty;

        /// <summary>Gets or sets name of the logger or component that generated the exception.</summary>
        [MaxLength(250)]
        public string Logger { get; set; } = string.Empty;

        /// <summary>Gets or sets full stack trace of the exception.</summary>
        [MaxLength(4000)]
        public string StackTrace { get; set; } = string.Empty;
    }
}