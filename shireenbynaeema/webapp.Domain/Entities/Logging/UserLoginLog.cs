namespace Domain
{
    using System.ComponentModel.DataAnnotations.Schema;

    using SharedServices;

    /// <summary>
    /// Logs all user login attempts including success/failure, timestamp, and client information.
    /// </summary>
    [Table("UserLoginLog")]
    [EntityDisplayName("User Login Log")]
    public class UserLoginLog : LogFields, IIdentifiable
    {
        /// <summary>Gets or sets primary identifier for the login attempt.</summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Gets or sets identifier of the user attempting to log in.</summary>
        public Guid? UserId { get; set; }

        /// <summary>Gets or sets email of the user attempting to log in.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Gets or sets a value indicating whether indicates whether the login attempt was successful.</summary>
        public bool IsSuccessful { get; set; }

        /// <summary>Gets or sets unix timestamp of when the login attempt occurred.</summary>
        public long AttemptedAt { get; set; }

        /// <summary>Gets or sets iP address from which the login attempt was made.</summary>
        public string IPAddress { get; set; } = string.Empty;

        /// <summary>Gets or sets user agent string from the client making the login attempt.</summary>
        public string UserAgent { get; set; } = string.Empty;

        /// <summary>Gets or sets optional message describing the login attempt outcome.</summary>
        public string Message { get; set; } = string.Empty;
    }
}