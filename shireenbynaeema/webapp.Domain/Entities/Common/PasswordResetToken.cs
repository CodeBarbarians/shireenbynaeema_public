namespace Domain
{
    using System.ComponentModel.DataAnnotations.Schema;

    using SharedServices;

    /// <summary>
    /// Represents a secure token generated for allowing a user to reset their password.
    /// Token becomes invalid after expiration or once used.
    /// </summary>
    [Table("PasswordResetToken")]
    [EntityDisplayName("Password Reset Token")]
    public class PasswordResetToken
    {
        /// <summary>
        /// Gets or sets primary identifier of the password reset token.
        /// Auto-generated unique GUID.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets identifier of the user requesting password reset.
        /// </summary>
        [ForeignKey(nameof(UserNavigation))]
        [AuditDisplay(typeof(User), nameof(User.DisplayName))]
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the associated user.
        /// </summary>
        public virtual User UserNavigation { get; set; } = default!;

        /// <summary>
        /// Gets or sets secure reset token value sent to the user.
        /// Used to validate password reset requests.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets expiration timestamp in Unix time (seconds).
        /// Token cannot be used after this time.
        /// </summary>
        public long ExpiryDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether indicates whether the token has already been consumed.
        /// Prevents multiple password reset attempts using the same token.
        /// </summary>
        public bool IsUsed { get; set; }
    }
}