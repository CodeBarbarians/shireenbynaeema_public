namespace Domain
{
    using System.ComponentModel.DataAnnotations.Schema;

    using SharedServices;

    /// <summary>
    /// Tracks invitation emails sent to users along with delivery and status details.
    /// </summary>
    [Table("UserInviteLog")]
    [EntityDisplayName("User Invite Log")]
    public class UserInviteLog : Auditable, IIdentifiable
    {
        /// <summary>Gets or sets primary identifier of the invite log entry.</summary>
        public Guid Id { get; set; }

        /// <summary>Gets or sets identifier of the user being invited.</summary>
        public Guid UserId { get; set; }

        /// <summary>Gets or sets role assigned to the invited user.</summary>
        public Guid RoleId { get; set; }

        /// <summary>Gets or sets user who initiated the invitation.</summary>
        public Guid? InvitedBy { get; set; }

        /// <summary>Gets or sets unix timestamp indicating when the invitation email was sent.</summary>
        public long? SentOn { get; set; }

        /// <summary>Gets or sets indicates whether the invitation email was successfully delivered.</summary>
        public int? IsSuccess { get; set; }

        /// <summary>Gets or sets current processing status of the invitation (pending, accepted, expired, etc.).</summary>
        public int? Status { get; set; }
    }
}