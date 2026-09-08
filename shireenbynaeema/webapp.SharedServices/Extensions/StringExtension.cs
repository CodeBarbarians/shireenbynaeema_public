namespace SharedServices
{
    using System.Text;

    /// <summary>
    /// Extension methods for the string class, providing additional functionality for string manipulation and conversion. These methods include converting a string to a MemoryStream, formatting a string with parameters, converting a hexadecimal string to a byte array, and converting a byte array to a hexadecimal string. These extensions can be used throughout the application to simplify common string operations and improve code readability.
    /// The ToMemoryStream method allows you to easily convert a string into a MemoryStream, which can be useful for scenarios where you need to work with streams instead of strings. The FormatWith method provides a convenient way to format a string using string.Format syntax, making it easier to create formatted strings without having to call string.Format directly. The HexToByteArray method enables you to convert a hexadecimal string representation into a byte array, which can be useful for handling binary data represented as hex strings. Finally, the ToHexString method allows you to convert a byte array back into its hexadecimal string representation, which can be helpful for displaying binary data in a human-readable format. Overall, these extension methods enhance the functionality of the string class and provide useful utilities for working with strings in various contexts within the application.
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// Converts a string to a MemoryStream using UTF-8 encoding. This method takes the input string, converts it to a byte array, and then creates a MemoryStream
        /// from that byte array. The resulting MemoryStream can be used in scenarios where you need to work with streams instead of strings, such as when reading or
        /// writing data to files, network streams, or other stream-based APIs. By using this extension method, you can easily convert any string into a MemoryStream\
        /// without having to manually handle the byte array conversion and stream creation process.
        /// </summary>
        /// <param name="str">str.</param>
        /// <returns>MemoryStream.</returns>
        public static MemoryStream ToMemoryStream(this string str)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(str);

            // Convert byte array to MemoryStream
            MemoryStream stream = new(byteArray);

            return stream;
        }

        /// <summary>
        /// Formats a string using the specified format and arguments. This method is an extension method for the string class, allowing you to call it directly on
        /// any string instance. It uses the string.Format method internally to format the string based on the provided format and arguments. The format parameter
        /// is a composite format string that contains placeholders for the arguments, and the args parameter is an array of objects that will be formatted and
        /// inserted into the resulting string. By using this extension method, you can easily create formatted strings without having to call string.Format directly,
        /// improving code readability and simplifying string formatting operations throughout your application.
        /// </summary>
        /// <param name="format">format.</param>
        /// <param name="args">args.</param>
        /// <returns>string.</returns>
        /// <exception cref="ArgumentNullException">ArgumentNullException.</exception>
        public static string FormatWith(this string format, params object[] args)
        {
            ArgumentNullException.ThrowIfNull(format);

            return string.Format(format, args);
        }

        /// <summary>
        /// Converts a hexadecimal string to a byte array. This method takes a string that represents hexadecimal values and converts it into an array of bytes. The input
        /// string must have an even length, as each pair of hexadecimal characters is converted to a single byte. If the input string is not valid hexadecimal, an exception
        /// will be thrown. This extension method is useful for scenarios where you need to work with raw byte data but have hexadecimal representations available.
        /// </summary>
        /// <param name="hex">hex.</param>
        /// <returns>byte[].</returns>
        public static byte[] HexToByteArray(this string hex)
        {
            return Enumerable.Range(0, hex.Length / 2)
                .Select(x => Convert.ToByte(hex.Substring(x * 2, 2), 16))
                .ToArray();
        }

        /// <summary>
        /// Converts a byte array to a hexadecimal string. This method takes an array of bytes and converts it into a string representation of hexadecimal values.
        /// Each byte in the array is represented by two hexadecimal characters in the resulting string. The output string will be in uppercase and will not contain
        /// any separators between the hexadecimal values. This extension method is useful for scenarios where you need to display or store binary data in a
        /// human-readable format, such as when working with cryptographic hashes, binary file contents, or any other raw byte data that needs to be represented
        /// as a string.
        /// </summary>
        /// <param name="bytes">bytes.</param>
        /// <returns>string.</returns>
        public static string ToHexString(this byte[] bytes)
        {
            return Convert.ToHexString(bytes);
        }

        // public static string ToJoinedBySpace(this string format, params object[] args)
        // {

        // }
   }
}