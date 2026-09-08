namespace Domain
{
    using System.ComponentModel.DataAnnotations.Schema;

    using SharedServices;

    /// <summary>
    /// Represents an invitation token used to allow a user to accept an invitation
    /// and complete account activation or registration.
    /// </summary>
    [Table("UserInviteToken")]
    [EntityDisplayName("User Invite Token")]
    public class UserInviteToken
    {
        /// <summary>
        /// Gets or sets primary identifier of the invite token.
        /// Auto-generated unique GUID.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets identifier of the invited user.
        /// </summary>
        [ForeignKey(nameof(UserNavigation))]
        [AuditDisplay(typeof(User), nameof(User.DisplayName))]
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the associated user.
        /// </summary>
        public virtual User UserNavigation { get; set; } = default!;

        /// <summary>
        /// Gets or sets secure invitation token value sent to the user.
        /// Used to validate invitation acceptance.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets expiration timestamp in Unix time (seconds).
        /// Token cannot be used after this time.
        /// </summary>
        public long ExpiryDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether indicates whether the invitation token has already been used.
        /// Prevents multiple activations using the same token.
        /// </summary>
        public bool IsUsed { get; set; }
    }
}