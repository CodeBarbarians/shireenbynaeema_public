namespace SharedServices
{
    /// <summary>
    /// Provides extension methods for safely handling null or whitespace strings and converting objects to safe string representations. These extensions help ensure
    /// that when working with strings or object representations, you can avoid issues related to null values or empty strings by providing default values defined in
    /// the Constants class. The OrEmpty and OrRetrieveEmpty methods return a default value when the input string is null or whitespace, while the ToSafeString method
    /// converts an object to its string representation, returning a default value if the object is null or its string representation is empty. These extensions are
    /// useful for improving code readability and reducing the likelihood of null reference exceptions when dealing with strings and object conversions in the
    /// application.
    /// </summary>
    public static class SafeExtensions
    {
        /// <summary>
        /// Returns the input string if it is not null or whitespace; otherwise, returns a default value defined in Constants.ListingEmpty. This extension method is useful for
        /// providing a safe default value when working with potentially null or empty strings.
        /// </summary>
        /// <param name="input">input.</param>
        /// <returns>string.</returns>
        public static string OrEmpty(this string? input)
        {
            return string.IsNullOrWhiteSpace(input)
                ? Constants.ListingEmpty
                : input;
        }

        /// <summary>
        /// Converts an object to its string representation, returning a default value defined in Constants.ListingEmpty if the object is null or if its string
        /// representation is null or whitespace. This extension method is useful for safely converting objects to strings while providing a fallback value when
        /// the input is not valid.
        /// </summary>
        /// <typeparam name="T">T.</typeparam>
        /// <param name="input">input.</param>
        /// <returns>string.</returns>
        public static string ToSafeString<T>(this T? input)
        {
            if (input == null)
            {
                return Constants.ListingEmpty;
            }

            var str = input.ToString();
            return string.IsNullOrWhiteSpace(str)
                ? Constants.ListingEmpty
                : str!;
        }

        /// <summary>
        /// Returns the input string if it is not null or whitespace; otherwise, returns a default value defined in Constants.RetrieveEmpty. This extension method is useful for
        /// providing a safe default value when working with potentially null or empty strings.
        /// </summary>
        /// <param name="input">input.</param>
        /// <returns>string.</returns>
        public static string OrRetrieveEmpty(this string? input)
        {
            return string.IsNullOrWhiteSpace(input)
                ? Constants.RetrieveEmpty
                : input;
        }

        /// <summary>
        /// Converts an object to its string representation, returning a default value defined in Constants.RetrieveEmpty if the object is null or if its string
        /// representation is null or whitespace. This extension method is useful for safely converting objects to strings while providing a fallback value when
        /// the input is not valid.
        /// </summary>
        /// <typeparam name="T">T.</typeparam>
        /// <param name="input">input.</param>
        /// <param name="emptyValue">emptyValue.</param>
        /// <returns>string.</returns>
        public static string ToSafeString<T>(this T? input, string? emptyValue = null)
        {
            if (input == null)
            {
                return emptyValue ?? Constants.RetrieveEmpty;
            }

            var str = input.ToString();
            return string.IsNullOrWhiteSpace(str)
                ? (emptyValue ?? Constants.RetrieveEmpty)
                : str!;
        }
    }
}