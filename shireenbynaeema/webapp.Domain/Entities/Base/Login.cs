namespace Application
{
    using System.Text.Json.Serialization;

    using SharedServices;

    /// <summary>
    /// Request model used for registering a new user account.
    /// </summary>
    public class UserRegisterRequest
    {
        /// <summary>Gets or sets user first name.</summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>Gets or sets user last name.</summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>Gets or sets user contact phone number.</summary>
        public string PhoneNo { get; set; } = string.Empty;

        /// <summary>Gets or sets associated organization identifier.</summary>
        public Guid? OrganizationId { get; set; }

        /// <summary>Gets or sets user email address.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Gets or sets user password.</summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>Gets or sets assigned role identifiers.</summary>
        public ICollection<Guid> Roles { get; set; } = [];
    }

    /// <summary>
    /// Request model used for user authentication.
    /// </summary>
    public class LoginRequest
    {
        /// <summary>Gets or sets user email address.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Gets or sets user password.</summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>Gets or sets google reCAPTCHA token.</summary>
        public string RecaptchaToken { get; set; } = string.Empty;

        /// <summary>Gets or sets a value indicating whether indicates persistent login session.</summary>
        public bool RememberMe { get; set; }
    }

    /// <summary>
    /// Response returned after login attempt.
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoginResponse"/> class.
        /// </summary>
        public LoginResponse()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="LoginResponse"/> class.Creates a login response with token or error.</summary>
        /// <param name="token">taken.</param>
        /// <param name="error">error.</param>
        public LoginResponse(string? token, Exception? error = null)
        {
            this.Token = token;
            if (error != null)
            {
                this.Error = new Error(error);
            }
        }

        /// <summary>Gets or sets jWT authentication token.</summary>
        public string? Token { get; set; }

        /// <summary>Gets or sets indicates persistent login session.</summary>
        public bool? RememberMe { get; set; }

        /// <summary>Gets or sets authenticated user identifier.</summary>
        public Guid? UserId { get; set; }

        /// <summary>Gets or sets error details if authentication failed.</summary>
        public Error? Error { get; set; }

        /// <summary>Gets or sets indicates whether OTP was sent.</summary>
        public bool? IsOTPSent { get; set; }
    }

    /// <summary>
    /// Response model returned by Google reCAPTCHA verification.
    /// </summary>
    public class RecaptchaResponse
    {
        /// <summary>Gets or sets a value indicating whether indicates if verification succeeded.</summary>
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        /// <summary>Gets or sets risk analysis score.</summary>
        [JsonPropertyName("score")]
        public float Score { get; set; }

        /// <summary>Gets or sets action name from reCAPTCHA request.</summary>
        [JsonPropertyName("action")]
        public string Action { get; set; } = string.Empty;

        /// <summary>Gets or sets challenge timestamp.</summary>
        [JsonPropertyName("challenge_ts")]
        public DateTime ChallengeTime { get; set; }

        /// <summary>Gets or sets hostname of the site where verification occurred.</summary>
        [JsonPropertyName("hostname")]
        public string Hostname { get; set; } = string.Empty;

        /// <summary>Gets or sets error codes returned by reCAPTCHA.</summary>
        [JsonPropertyName("error-codes")]
        public List<string> ErrorCodes { get; set; } = [];
    }
}