namespace Domain
{
    using System.ComponentModel.DataAnnotations.Schema;

    using SharedServices;

    /// <summary>
    /// Stores One Time Password (OTP) records used for authentication verification.
    /// Each record represents a generated OTP for a specific user with expiry and usage tracking.
    /// </summary>
    [Table("OtpCode")]
    [EntityDisplayName("OtpCode")]
    public class OtpCode
    {
        /// <summary>
        /// Gets or sets primary identifier of the OTP record.
        /// Auto-generated unique GUID.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets identifier of the user for whom the OTP was generated.
        /// </summary>
        [ForeignKey(nameof(UserNavigation))]
        [AuditDisplay(typeof(User), nameof(User.DisplayName))]
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the associated user.
        /// </summary>
        public virtual User UserNavigation { get; set; } = default!;

        /// <summary>
        /// Gets or sets the generated One Time Password value.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets expiry timestamp of the OTP in Unix time (seconds).
        /// After this time OTP becomes invalid.
        /// </summary>
        public long Expiry { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether indicates whether the OTP has already been used.
        /// Prevents OTP reuse after successful verification.
        /// </summary>
        public bool IsUsed { get; set; }

        /// <summary>
        /// Gets or sets creation timestamp of the OTP in Unix time (seconds).
        /// Used to determine latest OTP and expiration logic.
        /// </summary>
        public long CreatedAt { get; set; }
    }
}