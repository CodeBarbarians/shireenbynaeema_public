namespace SharedServices
{
/// <summary>
/// Extension methods for numeric types, providing functionality to round decimal and double values to the nearest long integer and to convert large numbers
/// into a more human-readable short form (e.g., 1.5K for 1500, 2.3M for 2,300,000). These extensions are useful for displaying numbers in a concise format,
/// especially in user interfaces where space is limited. The rounding methods ensure that decimal and double values are rounded away from zero to the nearest
/// long integer, while the short form methods convert large numbers into a more compact representation with appropriate suffixes (K for thousands, M for millions,
/// B for billions). Additionally, there are methods to format numbers as dollar amounts in both short and full forms, enhancing the readability of financial data
/// in the application.
/// </summary>
public static class NumberExtension
{
    /// <summary>
    /// Gets or sets the distance unit to be used in distance formatting methods.
    /// Default is "mi" (miles). Can be changed to "km" (kilometers) or "m" (meters).
    /// </summary>
    public static string DistanceUnit { get; set; } = "KM";

    /// <summary>
    /// Rounds a nullable decimal value to the nearest long integer, rounding away from zero. If the value is null, it returns 0.
    /// </summary>
    /// <param name="value">decimal.</param>
    /// <returns>long.</returns>
    public static long ToRoundInt64(this decimal? value)
    {
        return value == null ? 0 : Convert.ToInt64(Math.Round(value.Value, MidpointRounding.AwayFromZero));
    }

    /// <summary>
    /// Rounds a decimal value to the nearest long integer, rounding away from zero.
    /// </summary>
    /// <param name="value">decimal.</param>
    /// <returns>long.</returns>
    public static long ToRoundInt64(this decimal value)
    {
        return Convert.ToInt64(Math.Round(value, MidpointRounding.AwayFromZero));
    }

    /// <summary>
    /// Rounds a nullable double value to the nearest long integer, rounding away from zero. If the value is null, it returns 0.
    /// </summary>
    /// <param name="value">double.</param>
    /// <returns>long.</returns>
    public static long ToRoundInt64(this double? value)
    {
        return value == null ? 0 : Convert.ToInt64(Math.Round(value.Value, MidpointRounding.AwayFromZero));
    }

    /// <summary>
    /// Rounds a double value to the nearest long integer, rounding away from zero.
    /// </summary>
    /// <param name="value">double.</param>
    /// <returns>long.</returns>
    public static long ToRoundInt64(this double value)
    {
        return Convert.ToInt64(Math.Round(value, MidpointRounding.AwayFromZero));
    }

    /// <summary>
    /// Converts a long integer into a short form string representation. For example, 1500 becomes "1.5K", 2,300,000 becomes "2.3M", and 1,000,000,000 becomes "1B".
    /// </summary>
    /// <param name="number">long.</param>
    /// <returns>shortfomr.</returns>
    public static string ToShortForm(this long number)
    {
        var culture = System.Globalization.CultureInfo.InvariantCulture;
        if (number >= 1_000_000_000)
        {
            return (number / 1_000_000_000D).ToString("0.#", culture) + "B";
        }

        if (number >= 1_000_000)
        {
            return (number / 1_000_000D).ToString("0.#", culture) + "M";
        }

        if (number >= 1_000)
        {
            return (number / 1_000D).ToString("0.#", culture) + "K";
        }

        return number.ToString(culture);
    }

    /// <summary>
    /// Converts a nullable long integer into a short form string representation. For example, 1500 becomes "1.5K", 2,300,000 becomes "2.3M", and 1,000,000,000 becomes "1B".
    /// If the value is null, it returns "0".
    /// </summary>
    /// <param name="number">long?.</param>
    /// <returns>string.</returns>
    public static string ToShortForm(this long? number)
    {
        return number.HasValue ? number.Value.ToShortForm() : "0";
    }

    /// <summary>
    /// Converts an integer into a short form string representation. For example, 1500 becomes "1.5K", 2,300,000 becomes "2.3M", and 1,000,000,000 becomes "1B".
    /// </summary>
    /// <param name="number">int.</param>
    /// <returns>string.</returns>
    public static string ToShortForm(this int number)
    {
        return ((long)number).ToShortForm();
    }

    /// <summary>
    /// Converts a nullable integer into a short form string representation. For example, 1500 becomes "1.5K", 2,300,000 becomes "2.3M", and 1,000,000,000 becomes "1B".
    /// If the value is null, it returns "0".
    /// </summary>
    /// <param name="number">int?.</param>
    /// <returns>string.</returns>
    public static string ToShortForm(this int? number)
    {
        return number.HasValue ? number.Value.ToShortForm() : "0";
    }

    /// <summary>
    /// Converts a double value into a short form string representation. For example, 1500.0 becomes "1.5K", 2,300,000.0 becomes "2.3M", and
    /// 1,000,000,000.0 becomes "1B". The method rounds the double value to the nearest long integer before converting it to the short form.
    /// </summary>
    /// <param name="number">double.</param>
    /// <returns>string.</returns>
    public static string ToShortForm(this double number)
    {
        var culture = System.Globalization.CultureInfo.InvariantCulture;
        if (number >= 1_000_000_000)
        {
            return (number / 1_000_000_000D).ToString("0.#", culture) + "B";
        }

        if (number >= 1_000_000)
        {
            return (number / 1_000_000D).ToString("0.#", culture) + "M";
        }

        if (number >= 1_000)
        {
            return (number / 1_000D).ToString("0.#", culture) + "K";
        }

        return number.ToString("0.#", culture);
    }

    /// <summary>
    /// Converts a nullable double value into a short form string representation. For example, 1500.0 becomes "1.5K", 2,300,000.0 becomes "2.3M", and
    /// 1,000,000,000.0 becomes "1B". If the value is null, it returns "0".
    /// </summary>
    /// <param name="number">double?.</param>
    /// <returns>string.</returns>
    public static string ToShortForm(this double? number)
    {
        return number.HasValue ? number.Value.ToShortForm() : "0";
    }

    /// <summary>
    /// Converts a decimal value into a short form string representation. For example, 1500.0 becomes "1.5K", 2,300,000.0 becomes "2.3M", and
    /// 1,000,000,000.0 becomes "1B". The method rounds the decimal value to the nearest long integer before converting it to the short form.
    /// </summary>
    /// <param name="number">decimal.</param>
    /// <returns>shortform.</returns>
    public static string ToShortForm(this decimal number)
    {
        var culture = System.Globalization.CultureInfo.InvariantCulture;
        if (number >= 1_000_000_000)
        {
            return (number / 1_000_000_000M).ToString("0.#", culture) + "B";
        }

        if (number >= 1_000_000)
        {
            return (number / 1_000_000M).ToString("0.#", culture) + "M";
        }

        if (number >= 1_000)
        {
            return (number / 1_000M).ToString("0.#", culture) + "K";
        }

        return number.ToString("0.#", culture);
    }

    /// <summary>
    /// Converts a nullable decimal value into a short form string representation. For example, 1500.0 becomes "1.5K", 2,300,000.0 becomes "2.3M", and
    /// 1,000,000,000.0 becomes "1B". If the value is null, it returns "0".
    /// </summary>
    /// <param name="number">decimal?.</param>
    /// <returns>string.</returns>
    public static string ToShortForm(this decimal? number)
    {
        return number.HasValue ? number.Value.ToShortForm() : "0";
    }

    // --- Dollar Short Form ---

    /// <summary>
    /// Converts a long integer into a short form string representation with a dollar sign prefix. For example, 1500 becomes "$1.5K",
    /// 2,300,000 becomes "$2.3M", and 1,000,000,000 becomes "$1B".
    /// </summary>
    /// <param name="number">long.</param>
    /// <returns>string.</returns>
    public static string ToDollarShortForm(this long number)
    {
        return "$" + number.ToShortForm();
    }

    /// <summary>
    /// Converts a nullable long integer into a short form string representation with a dollar sign prefix. For example, 1500 becomes "$1.5K",
    /// 2,300,000 becomes "$2.3M", and 1,000,000,000 becomes "$1B". If the value is null, it returns "$0".
    /// </summary>
    /// <param name="number">long?.</param>
    /// <returns>string.</returns>
    public static string ToDollarShortForm(this long? number)
    {
        return number.HasValue ? "$" + number.Value.ToShortForm() : "$0";
    }

    /// <summary>
    /// Converts an integer into a short form string representation with a dollar sign prefix. For example, 1500 becomes "$1.5K",
    /// 2,300,000 becomes "$2.3M", and 1,000,000,000 becomes "$1B".
    /// </summary>
    /// <param name="number">number.</param>
    /// <returns>string.</returns>
    public static string ToDollarShortForm(this int number)
    {
        return "$" + number.ToShortForm();
    }

    /// <summary>
    /// Converts a nullable integer into a short form string representation with a dollar sign prefix. For example, 1500 becomes "$1.5K",
    /// 2,300,000 becomes "$2.3M", and 1,000,000,000 becomes "$1B". If the value is null, it returns "$0".
    /// </summary>
    /// <param name="number">int?.</param>
    /// <returns>string.</returns>
    public static string ToDollarShortForm(this int? number)
    {
        return number.HasValue ? "$" + number.Value.ToShortForm() : "$0";
    }

    /// <summary>
    /// Converts a double value into a short form string representation with a dollar sign prefix. For example, 1500.0 becomes "$1.5K", 2,300,000.0 becomes "$2.3M", and
    /// 1,000,000,000.0 becomes "$1B". The method rounds the double value to the nearest long integer before converting it to the short form.
    /// </summary>
    /// <param name="number">double.</param>
    /// <returns>string.</returns>
    public static string ToDollarShortForm(this double number)
    {
        return "$" + number.ToRoundInt64().ToShortForm();
    }

    /// <summary>
    /// Converts a nullable double value into a short form string representation with a dollar sign prefix. For example, 1500.0 becomes "$1.5K", 2,300,000.0 becomes "$2.3M", and
    /// 1,000,000,000.0 becomes "$1B". If the value is null, it returns "$0".
    /// </summary>
    /// <param name="number">double?.</param>
    /// <returns>string.</returns>
    public static string ToDollarShortForm(this double? number)
    {
        return number.HasValue ? "$" + number.Value.ToRoundInt64().ToShortForm() : "$0";
    }

    /// <summary>
    /// Converts a decimal value into a short form string representation with a dollar sign prefix. For example, 1500.0 becomes "$1.5K", 2,300,000.0 becomes "$2.3M", and
    /// 1,000,000,000.0 becomes "$1B". The method rounds the decimal value to the nearest long integer before converting it to the short form.
    /// </summary>
    /// <param name="number">decimal.</param>
    /// <returns>string.</returns>
    public static string ToDollarShortForm(this decimal number)
    {
        return "$" + number.ToRoundInt64().ToShortForm();
    }

    /// <summary>
    /// Converts a nullable decimal value into a short form string representation with a dollar sign prefix. For example, 1500.0 becomes "$1.5K", 2,300,000.0 becomes "$2.3M", and
    /// 1,000,000,000.0 becomes "$1B". If the value is null, it returns "$0".
    /// </summary>
    /// <param name="number">decimal?.</param>
    /// <returns>string.</returns>
    public static string ToDollarShortForm(this decimal? number)
    {
        return number.HasValue ? "$" + number.Value.ToRoundInt64().ToShortForm() : "$0";
    }

    // --- Only Dollar Form (no short form) ---

    /// <summary>
    /// Converts a long integer into a string representation with a dollar sign prefix and comma separators for thousands. For example, 1500 becomes "$1,500",
    /// 2,300,000 becomes "$2,300,000", and 1,000,000,000 becomes "$1,000,000,000".
    /// </summary>
    /// <param name="number">long.</param>
    /// <returns>string.</returns>
    public static string ToDollarForm(this long number)
    {
        return $"${number:N0}"; // N0 = adds commas, no decimals
    }

    /// <summary>
    /// Converts a nullable long integer into a string representation with a dollar sign prefix and comma separators for thousands. For example, 1500 becomes "$1,500",
    /// 2,300,000 becomes "$2,300,000", and 1,000,000,000 becomes "$1,000,000,000". If the value is null, it returns "$0".
    /// </summary>
    /// <param name="number">long?.</param>
    /// <returns>string.</returns>
    public static string ToDollarForm(this long? number)
    {
        return number.HasValue ? $"${number.Value:N0}" : "$0";
    }

    /// <summary>
    /// Converts an integer into a string representation with a dollar sign prefix and comma separators for thousands. For example, 1500 becomes "$1,500",
    /// 2,300,000 becomes "$2,300,000", and 1,000,000,000 becomes "$1,000,000,000".
    /// </summary>
    /// <param name="number">int.</param>
    /// <returns>string.</returns>
    public static string ToDollarForm(this int number)
    {
        return $"${number:N0}";
    }

    /// <summary>
    /// Converts a nullable integer into a string representation with a dollar sign prefix and comma separators for thousands. For example, 1500 becomes "$1,500",
    /// 2,300,000 becomes "$2,300,000", and 1,000,000,000 becomes "$1,000,000,000". If the value is null, it returns "$0".
    /// </summary>
    /// <param name="number">int?.</param>
    /// <returns>string.</returns>
    public static string ToDollarForm(this int? number)
    {
        return number.HasValue ? $"${number.Value:N0}" : "$0";
    }

    /// <summary>
    /// Converts a double value into a string representation with a dollar sign prefix and comma separators for thousands. For example, 1500.0 becomes "$1,500",
    /// 2,300,000.0 becomes "$2,300,000", and 1,000,000,000.0 becomes "$1,000,000,000".
    /// </summary>
    /// <param name="number">double.</param>
    /// <returns>string.</returns>
    public static string ToDollarForm(this double number)
    {
        return $"${number.ToRoundInt64():N0}"; // N2 = commas + 2 decimals
    }

    /// <summary>
    /// Converts a nullable double value into a string representation with a dollar sign prefix and comma separators for thousands. For example, 1500.0 becomes "$1,500",
    /// 2,300,000.0 becomes "$2,300,000", and 1,000,000,000.0 becomes "$1,000,000,000". If the value is null, it returns "$0".
    /// </summary>
    /// <param name="number">double?.</param>
    /// <returns>string.</returns>
    public static string ToDollarForm(this double? number)
    {
        return number.HasValue ? $"${number.Value.ToRoundInt64():N0}" : "$0";
    }

    /// <summary>
    /// Converts a decimal value into a string representation with a dollar sign prefix and comma separators for thousands. For example, 1500.0 becomes "$1,500",
    /// 2,300,000.0 becomes "$2,300,000", and 1,000,000,000.0 becomes "$1,000,000,000".
    /// </summary>
    /// <param name="number">decimal.</param>
    /// <returns>string.</returns>
    public static string ToDollarForm(this decimal number)
    {
        return $"${number.ToRoundInt64():N0}";
    }

    /// <summary>
    /// Converts a nullable decimal value into a string representation with a dollar sign prefix and comma separators for thousands. For example, 1500.0 becomes "$1,500",
    /// 2,300,000.0 becomes "$2,300,000", and 1,000,000,000.0 becomes "$1,000,000,000". If the value is null, it returns "$0".
    /// </summary>
    /// <param name="number">decimal?.</param>
    /// <returns>string.</returns>
    public static string ToDollarForm(this decimal? number)
    {
        return number.HasValue ? $"${number.Value.ToRoundInt64():N0}" : "$0";
    }

    // --- Percentage Short Form ---

    /// <summary>
    /// Converts a long integer into a short form string representation with a percentage suffix. For example, 1500 becomes "1.5K%",
    /// 2,300,000 becomes "2.3M%", and 1,000,000,000 becomes "1B%".
    /// </summary>
    /// <param name="number">long.</param>
    /// <returns>string.</returns>
    public static string ToPercentageShortForm(this long number)
    {
        return number.ToShortForm() + "%";
    }

    /// <summary>
    /// Converts a nullable long integer into a short form string representation with a percentage suffix. For example, 1500 becomes "1.5K%",
    /// 2,300,000 becomes "2.3M%", and 1,000,000,000 becomes "1B%". If the value is null, it returns "0%".
    /// </summary>
    /// <param name="number">long?.</param>
    /// <returns>string.</returns>
    public static string ToPercentageShortForm(this long? number)
    {
        return number.HasValue ? number.Value.ToShortForm() + "%" : "0%";
    }

    /// <summary>
    /// Converts an integer into a short form string representation with a percentage suffix. For example, 1500 becomes "1.5K%",
    /// 2,300,000 becomes "2.3M%", and 1,000,000,000 becomes "1B%".
    /// </summary>
    /// <param name="number">int.</param>
    /// <returns>string.</returns>
    public static string ToPercentageShortForm(this int number)
    {
        return number.ToShortForm() + "%";
    }

    /// <summary>
    /// Converts a nullable integer into a short form string representation with a percentage suffix. For example, 1500 becomes "1.5K%",
    /// 2,300,000 becomes "2.3M%", and 1,000,000,000 becomes "1B%". If the value is null, it returns "0%".
    /// </summary>
    /// <param name="number">int?.</param>
    /// <returns>string.</returns>
    public static string ToPercentageShortForm(this int? number)
    {
        return number.HasValue ? number.Value.ToShortForm() + "%" : "0%";
    }

    /// <summary>
    /// Converts a double value into a short form string representation with a percentage suffix. For example, 1500.0 becomes "1.5K%", 2,300,000.0 becomes "2.3M%", and
    /// 1,000,000,000.0 becomes "1B%". The method rounds the double value to the nearest long integer before converting it to the short form.
    /// </summary>
    /// <param name="number">double.</param>
    /// <returns>string.</returns>
    public static string ToPercentageShortForm(this double number)
    {
        return number.ToRoundInt64().ToShortForm() + "%";
    }

    /// <summary>
    /// Converts a nullable double value into a short form string representation with a percentage suffix. For example, 1500.0 becomes "1.5K%", 2,300,000.0 becomes "2.3M%", and
    /// 1,000,000,000.0 becomes "1B%". If the value is null, it returns "0%".
    /// </summary>
    /// <param name="number">double?.</param>
    /// <returns>string.</returns>
    public static string ToPercentageShortForm(this double? number)
    {
        return number.HasValue ? number.Value.ToRoundInt64().ToShortForm() + "%" : "0%";
    }

    /// <summary>
    /// Converts a decimal value into a short form string representation with a percentage suffix. For example, 1500.0 becomes "1.5K%", 2,300,000.0 becomes "2.3M%", and
    /// 1,000,000,000.0 becomes "1B%". The method rounds the decimal value to the nearest long integer before converting it to the short form.
    /// </summary>
    /// <param name="number">decimal.</param>
    /// <returns>string.</returns>
    public static string ToPercentageShortForm(this decimal number)
    {
        return number.ToRoundInt64().ToShortForm() + "%";
    }

    /// <summary>
    /// Converts a nullable decimal value into a short form string representation with a percentage suffix. For example, 1500.0 becomes "1.5K%", 2,300,000.0 becomes "2.3M%", and
    /// 1,000,000,000.0 becomes "1B%". If the value is null, it returns "0%".
    /// </summary>
    /// <param name="number">decimal?.</param>
    /// <returns>string.</returns>
    public static string ToPercentageShortForm(this decimal? number)
    {
        return number.HasValue ? number.Value.ToRoundInt64().ToShortForm() + "%" : "0%";
    }

    // --- Only Percentage Form (no short form) ---

    /// <summary>
    /// Converts a long integer into a string representation with a percentage suffix and comma separators for thousands. For example, 1500 becomes "1,500%",
    /// 2,300,000 becomes "2,300,000%", and 1,000,000,000 becomes "1,000,000,000%".
    /// </summary>
    /// <param name="number">long.</param>
    /// <returns>string.</returns>
    public static string ToPercentageForm(this long number)
    {
        return $"{number:N0}%"; // N0 = adds commas, no decimals
    }

    /// <summary>
    /// Converts a nullable long integer into a string representation with a percentage suffix and comma separators for thousands. For example, 1500 becomes "1,500%",
    /// 2,300,000 becomes "2,300,000%", and 1,000,000,000 becomes "1,000,000,000%". If the value is null, it returns "0%".
    /// </summary>
    /// <param name="number">long?.</param>
    /// <returns>string.</returns>
    public static string ToPercentageForm(this long? number)
    {
        return number.HasValue ? $"{number.Value:N0}%" : "0%";
    }

    /// <summary>
    /// Converts an integer into a string representation with a percentage suffix and comma separators for thousands. For example, 1500 becomes "1,500%",
    /// 2,300,000 becomes "2,300,000%", and 1,000,000,000 becomes "1,000,000,000%".
    /// </summary>
    /// <param name="number">int.</param>
    /// <returns>string.</returns>
    public static string ToPercentageForm(this int number)
    {
        return $"{number:N0}%";
    }

    /// <summary>
    /// Converts a nullable integer into a string representation with a percentage suffix and comma separators for thousands. For example, 1500 becomes "1,500%",
    /// 2,300,000 becomes "2,300,000%", and 1,000,000,000 becomes "1,000,000,000%". If the value is null, it returns "0%".
    /// </summary>
    /// <param name="number">int?.</param>
    /// <returns>string.</returns>
    public static string ToPercentageForm(this int? number)
    {
        return number.HasValue ? $"{number.Value:N0}%" : "0%";
    }

    /// <summary>
    /// Converts a double value into a string representation with a percentage suffix and comma separators for thousands. For example, 1500.0 becomes "1,500%",
    /// 2,300,000.0 becomes "2,300,000%", and 1,000,000,000.0 becomes "1,000,000,000%".
    /// </summary>
    /// <param name="number">double.</param>
    /// <returns>string.</returns>
    public static string ToPercentageForm(this double number)
    {
        return $"{number.ToRoundInt64():N0}%";
    }

    /// <summary>
    /// Converts a nullable double value into a string representation with a percentage suffix and comma separators for thousands. For example, 1500.0 becomes "1,500%",
    /// 2,300,000.0 becomes "2,300,000%", and 1,000,000,000.0 becomes "1,000,000,000%". If the value is null, it returns "0%".
    /// </summary>
    /// <param name="number">double?.</param>
    /// <returns>string.</returns>
    public static string ToPercentageForm(this double? number)
    {
        return number.HasValue ? $"{number.Value.ToRoundInt64():N0}%" : "0%";
    }

    /// <summary>
    /// Converts a decimal value into a string representation with a percentage suffix and comma separators for thousands. For example, 1500.0 becomes "1,500%",
    /// 2,300,000.0 becomes "2,300,000%", and 1,000,000,000.0 becomes "1,000,000,000%".
    /// </summary>
    /// <param name="number">decimal.</param>
    /// <returns>string.</returns>
    public static string ToPercentageForm(this decimal number)
    {
        return $"{number.ToRoundInt64():N0}%";
    }

    /// <summary>
    /// Converts a nullable decimal value into a string representation with a percentage suffix and comma separators for thousands. For example, 1500.0 becomes "1,500%",
    /// 2,300,000.0 becomes "2,300,000%", and 1,000,000,000.0 becomes "1,000,000,000%". If the value is null, it returns "0%".
    /// </summary>
    /// <param name="number">decimal?.</param>
    /// <returns>string.</returns>
    public static string ToPercentageForm(this decimal? number)
    {
        return number.HasValue ? $"{number.Value.ToRoundInt64():N0}%" : "0%";
    }

    // --- Distance Short Form ---

    /// <summary>
    /// Converts a long integer into a short form string representation with the configured distance unit suffix. For example, 1500 becomes "1.5K mi",
    /// 2,300,000 becomes "2.3M mi", and 1,000,000,000 becomes "1B mi". The unit can be changed via the DistanceUnit property.
    /// </summary>
    /// <param name="number">long.</param>
    /// <returns>string.</returns>
    public static string ToDistanceShortForm(this long number)
    {
        return number.ToShortForm() + " " + DistanceUnit;
    }

    /// <summary>
    /// Converts a nullable long integer into a short form string representation with the configured distance unit suffix. For example, 1500 becomes "1.5K mi",
    /// 2,300,000 becomes "2.3M mi", and 1,000,000,000 becomes "1B mi". If the value is null, it returns "0 mi". The unit can be changed via the DistanceUnit property.
    /// </summary>
    /// <param name="number">long?.</param>
    /// <returns>string.</returns>
    public static string ToDistanceShortForm(this long? number)
    {
        return number.HasValue ? number.Value.ToShortForm() + " " + DistanceUnit : "0 " + DistanceUnit;
    }

    /// <summary>
    /// Converts an integer into a short form string representation with the configured distance unit suffix. For example, 1500 becomes "1.5K mi",
    /// 2,300,000 becomes "2.3M mi", and 1,000,000,000 becomes "1B mi". The unit can be changed via the DistanceUnit property.
    /// </summary>
    /// <param name="number">int.</param>
    /// <returns>string.</returns>
    public static string ToDistanceShortForm(this int number)
    {
        return number.ToShortForm() + " " + DistanceUnit;
    }

    /// <summary>
    /// Converts a nullable integer into a short form string representation with the configured distance unit suffix. For example, 1500 becomes "1.5K mi",
    /// 2,300,000 becomes "2.3M mi", and 1,000,000,000 becomes "1B mi". If the value is null, it returns "0 mi". The unit can be changed via the DistanceUnit property.
    /// </summary>
    /// <param name="number">int?.</param>
    /// <returns>string.</returns>
    public static string ToDistanceShortForm(this int? number)
    {
        return number.HasValue ? number.Value.ToShortForm() + " " + DistanceUnit : "0 " + DistanceUnit;
    }

    /// <summary>
    /// Converts a double value into a short form string representation with the configured distance unit suffix. For example, 1500.0 becomes "1.5K mi", 2,300,000.0 becomes "2.3M mi", and
    /// 1,000,000,000.0 becomes "1B mi". The method rounds the double value to the nearest long integer before converting it to the short form. The unit can be changed via the DistanceUnit property.
    /// </summary>
    /// <param name="number">double.</param>
    /// <returns>string.</returns>
    public static string ToDistanceShortForm(this double number)
    {
        return number.ToRoundInt64().ToShortForm() + " " + DistanceUnit;
    }

    /// <summary>
    /// Converts a nullable double value into a short form string representation with the configured distance unit suffix. For example, 1500.0 becomes "1.5K mi", 2,300,000.0 becomes "2.3M mi", and
    /// 1,000,000,000.0 becomes "1B mi". If the value is null, it returns "0 mi". The unit can be changed via the DistanceUnit property.
    /// </summary>
    /// <param name="number">double?.</param>
    /// <returns>string.</returns>
    public static string ToDistanceShortForm(this double? number)
    {
        return number.HasValue ? number.Value.ToRoundInt64().ToShortForm() + " " + DistanceUnit : "0 " + DistanceUnit;
    }

    /// <summary>
    /// Converts a decimal value into a short form string representation with the configured distance unit suffix. For example, 1500.0 becomes "1.5K mi", 2,300,000.0 becomes "2.3M mi", and
    /// 1,000,000,000.0 becomes "1B mi". The method rounds the decimal value to the nearest long integer before converting it to the short form. The unit can be changed via the DistanceUnit property.
    /// </summary>
    /// <param name="number">decimal.</param>
    /// <returns>string.</returns>
    public static string ToDistanceShortForm(this decimal number)
    {
        return number.ToRoundInt64().ToShortForm() + " " + DistanceUnit;
    }

    /// <summary>
    /// Converts a nullable decimal value into a short form string representation with the configured distance unit suffix. For example, 1500.0 becomes "1.5K mi", 2,300,000.0 becomes "2.3M mi", and
    /// 1,000,000,000.0 becomes "1B mi". If the value is null, it returns "0 mi". The unit can be changed via the DistanceUnit property.
    /// </summary>
    /// <param name="number">decimal?.</param>
    /// <returns>string.</returns>
    public static string ToDistanceShortForm(this decimal? number)
    {
        return number.HasValue ? number.Value.ToRoundInt64().ToShortForm() + " " + DistanceUnit : "0 " + DistanceUnit;
    }

    // --- Only Distance Form (no short form) ---

    /// <summary>
    /// Converts a long integer into a string representation with the configured distance unit suffix and comma separators for thousands. For example, 1500 becomes "1,500 mi",
    /// 2,300,000 becomes "2,300,000 mi", and 1,000,000,000 becomes "1,000,000,000 mi". The unit can be changed via the DistanceUnit property.
    /// </summary>
    /// <param name="number">long.</param>
    /// <returns>string.</returns>
    public static string ToDistanceForm(this long number)
    {
        return $"{number:N0} {DistanceUnit}"; // N0 = adds commas, no decimals
    }

    /// <summary>
    /// Converts a nullable long integer into a string representation with the configured distance unit suffix and comma separators for thousands. For example, 1500 becomes "1,500 mi",
    /// 2,300,000 becomes "2,300,000 mi", and 1,000,000,000 becomes "1,000,000,000 mi". If the value is null, it returns "0 mi". The unit can be changed via the DistanceUnit property.
    /// </summary>
    /// <param name="number">long?.</param>
    /// <returns>string.</returns>
    public static string ToDistanceForm(this long? number)
    {
        return number.HasValue ? $"{number.Value:N0} {DistanceUnit}" : $"0 {DistanceUnit}";
    }

    /// <summary>
    /// Converts an integer into a string representation with the configured distance unit suffix and comma separators for thousands. For example, 1500 becomes "1,500 mi",
    /// 2,300,000 becomes "2,300,000 mi", and 1,000,000,000 becomes "1,000,000,000 mi". The unit can be changed via the DistanceUnit property.
    /// </summary>
    /// <param name="number">int.</param>
    /// <returns>string.</returns>
    public static string ToDistanceForm(this int number)
    {
        return $"{number:N0} {DistanceUnit}";
    }

    /// <summary>
    /// Converts a nullable integer into a string representation with the configured distance unit suffix and comma separators for thousands. For example, 1500 becomes "1,500 mi",
    /// 2,300,000 becomes "2,300,000 mi", and 1,000,000,000 becomes "1,000,000,000 mi". If the value is null, it returns "0 mi". The unit can be changed via the DistanceUnit property.
    /// </summary>
    /// <param name="number">int?.</param>
    /// <returns>string.</returns>
    public static string ToDistanceForm(this int? number)
    {
        return number.HasValue ? $"{number.Value:N0} {DistanceUnit}" : $"0 {DistanceUnit}";
    }

    /// <summary>
    /// Converts a double value into a string representation with the configured distance unit suffix and comma separators for thousands. For example, 1500.0 becomes "1,500 mi",
    /// 2,300,000.0 becomes "2,300,000 mi", and 1,000,000,000.0 becomes "1,000,000,000 mi". The unit can be changed via the DistanceUnit property.
    /// </summary>
    /// <param name="number">double.</param>
    /// <returns>string.</returns>
    public static string ToDistanceForm(this double number)
    {
        return $"{number.ToRoundInt64():N0} {DistanceUnit}";
    }

    /// <summary>
    /// Converts a nullable double value into a string representation with the configured distance unit suffix and comma separators for thousands. For example, 1500.0 becomes "1,500 mi",
    /// 2,300,000.0 becomes "2,300,000 mi", and 1,000,000,000.0 becomes "1,000,000,000 mi". If the value is null, it returns "0 mi". The unit can be changed via the DistanceUnit property.
    /// </summary>
    /// <param name="number">double?.</param>
    /// <returns>string.</returns>
    public static string ToDistanceForm(this double? number)
    {
        return number.HasValue ? $"{number.Value.ToRoundInt64():N0} {DistanceUnit}" : $"0 {DistanceUnit}";
    }

    /// <summary>
    /// Converts a decimal value into a string representation with the configured distance unit suffix and comma separators for thousands. For example, 1500.0 becomes "1,500 mi",
    /// 2,300,000.0 becomes "2,300,000 mi", and 1,000,000,000.0 becomes "1,000,000,000 mi". The unit can be changed via the DistanceUnit property.
    /// </summary>
    /// <param name="number">decimal.</param>
    /// <returns>string.</returns>
    public static string ToDistanceForm(this decimal number)
    {
        return $"{number.ToRoundInt64():N0} {DistanceUnit}";
    }

    /// <summary>
    /// Converts a nullable decimal value into a string representation with the configured distance unit suffix and comma separators for thousands. For example, 1500.0 becomes "1,500 mi",
    /// 2,300,000.0 becomes "2,300,000 mi", and 1,000,000,000.0 becomes "1,000,000,000 mi". If the value is null, it returns "0 mi". The unit can be changed via the DistanceUnit property.
    /// </summary>
    /// <param name="number">decimal?.</param>
    /// <returns>string.</returns>
    public static string ToDistanceForm(this decimal? number)
    {
        return number.HasValue ? $"{number.Value.ToRoundInt64():N0} {DistanceUnit}" : $"0 {DistanceUnit}";
    }
}
}