namespace SharedServices
{
    /// <summary>
    /// Provides extension methods for converting between <see cref="decimal"/> and <see cref="double"/> types,
    /// including support for their nullable counterparts.
    /// </summary>
    public static class ConversionExtensions
    {
        /// <summary>
        /// Converts a <see cref="decimal"/> value to a <see cref="double"/>.
        /// </summary>
        /// <param name="value">The decimal value to convert.</param>
        /// <returns>The double representation of the specified decimal value.</returns>
        // Non-nullable decimal → double
        public static double ToDouble(this decimal value)
        {
            return (double)value;
        }

        /// <summary>
        /// Converts a nullable <see cref="decimal"/> value to a nullable <see cref="double"/>.
        /// </summary>
        /// <param name="value">The nullable decimal value to convert.</param>
        /// <returns>The nullable double representation of the specified value, or <c>null</c> if the input is <c>null</c>.</returns>
        // Nullable decimal → Nullable double
        public static double? ToDouble(this decimal? value)
        {
            return value.HasValue ? (double?)value.Value : null;
        }

        /// <summary>
        /// Converts a <see cref="double"/> value to a <see cref="decimal"/>.
        /// </summary>
        /// <param name="value">The double value to convert.</param>
        /// <returns>The decimal representation of the specified double value.</returns>
        // Non-nullable double → decimal
        public static decimal ToDecimal(this double value)
        {
            return (decimal)value;
        }

        /// <summary>
        /// Converts a nullable <see cref="double"/> value to a nullable <see cref="decimal"/>.
        /// </summary>
        /// <param name="value">The nullable double value to convert.</param>
        /// <returns>The nullable decimal representation of the specified value, or <c>null</c> if the input is <c>null</c>.</returns>
        // Nullable double → Nullable decimal
        public static decimal? ToDecimal(this double? value)
        {
            return value.HasValue ? (decimal?)value.Value : null;
        }
    }
}