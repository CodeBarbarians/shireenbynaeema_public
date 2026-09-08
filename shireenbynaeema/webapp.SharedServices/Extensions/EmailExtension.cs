namespace SharedServices
{
    using System.Net.Mail;

    /// <summary>
    /// Extension methods for validating email addresses. This class provides a method to check if a given string is a valid email address format using the System.Net.Mail.MailAddress class.
    /// </summary>
    public static class EmailExtension
    {
        /// <summary>
        /// Validates whether the provided string is in a valid email address format. It attempts to create a MailAddress object with the input string,
        /// and if it succeeds without throwing an exception, it returns true. If an exception is thrown, it catches it and returns false, indicating that
        /// the input string is not a valid email address.
        /// </summary>
        /// <param name="email">email.</param>
        /// <returns>bool.</returns>
        public static bool IsValidEmail(this string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}