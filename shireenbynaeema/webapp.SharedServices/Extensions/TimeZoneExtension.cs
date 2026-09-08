namespace SharedServices
{
    /// <summary>
    /// Provides extension methods for converting between time zone offsets and string representations, and for
    /// retrieving the current UTC offset for a specified time zone.
    /// </summary>
    public static class TimeZoneExtension
    {
        /// <summary>
        /// Converts a time zone offset in minutes to a string representation in the format "UTC±hh:mm".
        /// </summary>
        /// <param name="offsetInMinutes">The time zone offset in minutes.</param>
        /// <returns>A string representation of the time zone offset.</returns>
        public static string ConvertOffsetToTimeZoneString(this int offsetInMinutes)
        {
            TimeSpan offset = TimeSpan.FromMinutes(offsetInMinutes);
            string sign = offsetInMinutes >= 0 ? "+" : "-";
            return $"UTC{sign}{Math.Abs(offset.Hours):D2}:{Math.Abs(offset.Minutes):D2}";
        }

        /// <summary>
        /// Converts a time zone string in the format 'UTC±hh:mm' to its offset in minutes from Coordinated Universal
        /// Time (UTC).
        /// </summary>
        /// <param name="timeZoneString">A string representing the time zone offset from UTC, in the format 'UTC±hh:mm'. For example, 'UTC+05:30' or
        /// 'UTC-04:00'.</param>
        /// <returns>The offset from UTC, in minutes. Positive values indicate time zones east of UTC; negative values indicate
        /// time zones west of UTC.</returns>
        /// <exception cref="ArgumentException">Thrown if timeZoneString is null, empty, or not in the expected 'UTC±hh:mm' format.</exception>
        public static int ConvertTimeZoneStringToOffset(this string timeZoneString)
        {
            if (string.IsNullOrWhiteSpace(timeZoneString) || !timeZoneString.StartsWith("UTC"))
            {
                throw new ArgumentException("Invalid time zone string format. Expected format: 'UTC±hh:mm'.");
            }

            string offsetPart = timeZoneString.Substring(3); // Remove "UTC"
            if (offsetPart.Length != 6 || (offsetPart[0] != '+' && offsetPart[0] != '-'))
            {
                throw new ArgumentException("Invalid time zone string format. Expected format: 'UTC±hh:mm'.");
            }

            char sign = offsetPart[0];
            string[] parts = offsetPart.Substring(1).Split(':');
            if (parts.Length != 2 || !int.TryParse(parts[0], out int hours) || !int.TryParse(parts[1], out int minutes))
            {
                throw new ArgumentException("Invalid time zone string format. Expected format: 'UTC±hh:mm'.");
            }

            int totalMinutes = (hours * 60) + minutes;
            return sign == '+' ? totalMinutes : -totalMinutes;
        }

        /// <summary>
        /// Gets the current UTC offset for the specified time zone identifier as a formatted string.
        /// </summary>
        /// <remarks>The returned offset is based on the current date and time (UTC) and reflects any
        /// daylight saving adjustments in effect at the time of the call.</remarks>
        /// <param name="timeZoneId">The identifier of the time zone to retrieve the UTC offset for. This should be a valid system time zone ID.</param>
        /// <returns>A string representing the current UTC offset of the specified time zone in the format "hh:mm".</returns>
        /// <exception cref="ArgumentException">Thrown if the specified <paramref name="timeZoneId"/> is not found or is invalid.</exception>
        public static double GetOffSetfromTimeZone(this string timeZoneId)
        {
            try
            {
                TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                TimeSpan offset = tz.GetUtcOffset(DateTime.UtcNow);
                return offset.TotalHours;
            }
            catch (TimeZoneNotFoundException)
            {
                throw new ArgumentException($"The time zone ID '{timeZoneId}' was not found.");
            }
            catch (InvalidTimeZoneException)
            {
                throw new ArgumentException($"The time zone ID '{timeZoneId}' is invalid.");
            }
        }
    }
}