namespace Application
{
    using SharedServices;

    /// <summary>
    /// Interface for email services, providing a method to send emails with various parameters such as recipient email, subject, plain text content, HTML content,
    /// and attachments.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends an email message asynchronously to the specified recipient, with support for plain text and HTML
        /// content, optional attachments, and customizable sender and recipient details.
        /// </summary>
        /// <remarks>If both <paramref name="plainText"/> and <paramref name="htmlContent"/> are provided,
        /// the email will include both formats for compatibility with various email clients. Attachments must be valid
        /// base64-encoded files. The method does not guarantee delivery; it only initiates the send request and returns
        /// the response from the underlying email service.</remarks>
        /// <param name="toEmail">The email address of the primary recipient. Cannot be null or empty.</param>
        /// <param name="subject">The subject line of the email message. Cannot be null or empty.</param>
        /// <param name="plainText">The plain text content of the email message. Used as the fallback for email clients that do not support
        /// HTML.</param>
        /// <param name="htmlContent">The HTML content of the email message. If null or empty, only the plain text content will be sent.</param>
        /// <param name="fromKey">The key identifying the sender profile to use. Defaults to "PwdReset" if not specified.</param>
        /// <param name="fromNameOverride">An optional display name to override the sender's name. If null, the default sender name associated with
        /// <paramref name="fromKey"/> is used.</param>
        /// <param name="attachments">A list of attachments to include in the email, where each item specifies a file name and its base64-encoded
        /// content. If null, no attachments are sent.</param>
        /// <param name="fromEmailOverride">An optional email address to override the sender's email. If null, the default sender email associated with
        /// <paramref name="fromKey"/> is used.</param>
        /// <param name="ccs">A list of email addresses to include as carbon copy (CC) recipients. If null or empty, no CC recipients are
        /// added.</param>
        /// <param name="replytos">A list of email addresses to set as reply-to recipients. If null or empty, replies will be directed to the
        /// sender.</param>
        /// <returns>A task representing the asynchronous operation. The result contains a response object indicating the outcome
        /// of the email send request.</returns>
        Task<IResponse> SendEmail(string toEmail, string subject, string plainText, string htmlContent, string fromKey = "PwdReset", string? fromNameOverride = null, List<(string FileName, string Base64File)>? attachments = null, string? fromEmailOverride = null, List<string>? ccs = null, List<string>? replytos = null);
    }
}