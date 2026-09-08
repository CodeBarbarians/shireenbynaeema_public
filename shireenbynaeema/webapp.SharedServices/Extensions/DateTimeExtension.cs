namespace SharedServices
{
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    /// <summary>
    /// Provides extension methods for DateTime and Unix timestamp conversions,
    /// formatting, and time zone handling.
    /// </summary>
    public static class DateTimeExtension
    {
        private const string DefaultTimeZone = "Eastern Standard Time";

        private static readonly TimeZoneInfo EstZone =
          TimeZoneInfo.FindSystemTimeZoneById(DefaultTimeZone);

        /// <summary>
        /// Gets the current UTC DateTime.
        /// </summary>
        public static DateTime UtcNow =>
            DateTimeOffset.UtcNow.UtcDateTime;

        /// <summary>
        /// Gets the current UTC time as a Unix timestamp.
        /// </summary>
        public static long UtcNowUnixTimestamp =>
            DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        /// <summary>
        /// Converts a nullable DateTime to a Unix timestamp.
        /// </summary>
        /// <param name="dateTime">dateTime.</param>
        /// <returns>long.</returns>
        public static long? ToTimeStamp(this DateTime? dateTime)
        {
            if (!dateTime.HasValue)
            {
                return null;
            }

            return new DateTimeOffset(dateTime.Value).ToUnixTimeSeconds();
        }

        /// <summary>
        /// Converts a DateTime to a Unix timestamp.
        /// </summary>
        /// <param name="dateTime">datetime.</param>
        /// <returns>long.</returns>
        public static long ToTimeStamp(this DateTime dateTime)
        {
            return new DateTimeOffset(dateTime).ToUnixTimeSeconds();
        }

        extension(long timeStamp)
        {
            /// <summary>
            /// Converts a Unix timestamp to a local DateTime.
            /// </summary>
            /// <returns>DateTime.</returns>
            public DateTime ToDateTime()
            {
                return DateTimeOffset
                    .FromUnixTimeSeconds(timeStamp)
                    .LocalDateTime;
            }

            /// <summary>
            /// Formats a Unix timestamp to a string.
            /// </summary>
            /// <returns>string.</returns>
            public string ToFormattedDateTime(
                [StringSyntax(StringSyntaxAttribute.DateTimeFormat)] string? format = null)
            {
                ArgumentOutOfRangeException.ThrowIfNegative(timeStamp);

                // Check if the timestamp is in milliseconds and convert to seconds if necessary
                if (timeStamp > DateTimeOffset.MaxValue.ToUnixTimeSeconds())
                {
                    timeStamp /= 1000;
                }

                var dateTime = DateTimeOffset
                    .FromUnixTimeSeconds(timeStamp)
                    .LocalDateTime;

                return dateTime.ToString(format ?? "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Converts a nullable Unix timestamp to a nullable local DateTime.
        /// </summary>
        /// <param name="timeStamp">timeStamp.</param>
        /// <returns>DateTime.</returns>
        public static DateTime? ToDateTime(this long? timeStamp)
        {
            if (!timeStamp.HasValue)
            {
                return null;
            }

            return DateTimeOffset
                .FromUnixTimeSeconds(timeStamp.Value)
                .LocalDateTime;
        }

        /// <summary>
        /// Formats a nullable Unix timestamp to a string.
        /// </summary>
        /// <param name="timeStamp">timeStamp.</param>
        /// <param name="format">format.</param>
        /// <returns>string.</returns>
        public static string? ToFormattedDateTime(
            this long? timeStamp,
            [StringSyntax(StringSyntaxAttribute.DateTimeFormat)] string? format = null)
        {
            if (timeStamp is null)
            {
                throw new ArgumentNullException(nameof(timeStamp));
            }

            if (timeStamp < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(timeStamp), "Value cannot be negative.");
            }

            // Check if the timestamp is in milliseconds and convert to seconds if necessary
            if (timeStamp > DateTimeOffset.MaxValue.ToUnixTimeSeconds())
            {
                timeStamp /= 1000;
            }

            if (!timeStamp.HasValue)
            {
                return null;
            }

            return timeStamp.Value.ToFormattedDateTime(format);
        }

        /// <summary>
        /// Formats a DateTime to string.
        /// </summary>
        /// <param name="dateTime">dateTime.</param>
        /// <param name="format">format.</param>
        /// <returns>string.</returns>
        public static string ToFormattedDateTime(
            this DateTime dateTime,
            [StringSyntax(StringSyntaxAttribute.DateTimeFormat)] string? format = null)
        {
            var local = dateTime.Kind == DateTimeKind.Utc
                ? dateTime.ToLocalTime()
                : dateTime;

            return local.ToString(format ?? "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Formats a nullable DateTime to string.
        /// </summary>
        /// <param name="dateTime">datetime.</param>
        /// <param name="format">format.</param>
        /// <returns>string.</returns>
        public static string? ToFormattedDateTime(
            this DateTime? dateTime,
            [StringSyntax(StringSyntaxAttribute.DateTimeFormat)] string? format = null)
        {
            if (dateTime is null)
            {
                throw new ArgumentNullException(nameof(dateTime));
            }

            if (!dateTime.HasValue)
            {
                return null;
            }

            return dateTime.Value.ToFormattedDateTime(format);
        }

        /// <summary>
        /// Formats DateTime using dashboard format.
        /// </summary>
        /// <param name="dateTime">datetime.</param>
        /// <returns>string.</returns>
        public static string ToDashboardFormattedDateTime(this DateTime dateTime)
        {
            var local = dateTime.Kind == DateTimeKind.Utc
                ? dateTime.ToLocalTime()
                : dateTime;

            return local.ToString(Constants.DateTime.DashboardDateFormat, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Formats nullable DateTime using dashboard format.
        /// </summary>
        /// <param name="dateTime">datetime.</param>
        /// <returns>string.</returns>
        public static string? ToDashboardFormattedDateTime(this DateTime? dateTime)
        {
            if (!dateTime.HasValue)
            {
                return null;
            }

            return dateTime.Value.ToDashboardFormattedDateTime();
        }

        /// <summary>
        /// Converts a UTC DateTime to a specified time zone.
        /// </summary>
        /// <param name="utcDateTime">utcDateTime.</param>
        /// <param name="timeZoneId">timeZoneId.</param>
        /// <returns>DateTime.</returns>
        public static DateTime ConvertToTimeZone(this DateTime utcDateTime, string timeZoneId = DefaultTimeZone)
        {
            if (utcDateTime.Kind != DateTimeKind.Utc)
            {
                utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            }

            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZone);
        }

        /// <summary>
        /// Converts nullable UTC DateTime to a specified time zone.
        /// </summary>
        /// <param name="utcDateTime">utcDateTime.</param>
        /// <param name="timeZoneId">timeZoneId.</param>
        /// <returns>DateTime.</returns>
        public static DateTime? ConvertToTimeZone(this DateTime? utcDateTime, string timeZoneId = DefaultTimeZone)
        {
            if (!utcDateTime.HasValue)
            {
                return null;
            }

            return utcDateTime.Value.ConvertToTimeZone(timeZoneId);
        }

        /// <summary>
        /// Converts Unix timestamp to DateTime in a specified time zone.
        /// </summary>
        /// <param name="unixTimestamp">unixTimestamp.</param>
        /// <param name="timeZoneId">timeZoneId.</param>
        /// <returns>DateTime.</returns>
        public static DateTime ConvertUnixTimeToTimeZone(this long unixTimestamp, string timeZoneId = DefaultTimeZone)
        {
            var utcDateTime = DateTimeOffset
                .FromUnixTimeSeconds(unixTimestamp)
                .UtcDateTime;

            return utcDateTime.ConvertToTimeZone(timeZoneId);
        }

        /// <summary>
        /// Converts nullable Unix timestamp to DateTime in a specified time zone.
        /// </summary>
        /// <param name="unixTimestamp">unixTimestamp.</param>
        /// <param name="timeZoneId">timeZoneId.</param>
        /// <returns>DateTime.</returns>
        public static DateTime? ConvertUnixTimeToTimeZone(this long? unixTimestamp, string timeZoneId = DefaultTimeZone)
        {
            if (!unixTimestamp.HasValue)
            {
                return null;
            }

            return ConvertUnixTimeToTimeZone(unixTimestamp.Value, timeZoneId);
        }

        /// <summary>
        /// Converts Eastern Time (EST/EDT) DateTime to Unix timestamp.
        /// </summary>
        /// <param name="estDateTime">estDateTime.</param>
        /// <returns>long.</returns>
        public static long ToUnixTimestampFromEst(this DateTime estDateTime)
        {
            var local = DateTime.SpecifyKind(estDateTime, DateTimeKind.Unspecified);

            if (EstZone.IsInvalidTime(local))
            {
                local = local.AddHours(1);
            }

            if (EstZone.IsAmbiguousTime(local))
            {
                local = local.AddMinutes(1);
            }

            var utc = TimeZoneInfo.ConvertTimeToUtc(local, EstZone);

            return new DateTimeOffset(utc).ToUnixTimeSeconds();
        }

        /// <summary>
        /// Converts nullable Eastern Time DateTime to Unix timestamp.
        /// </summary>
        /// <param name="estDateTime">estDateTime.</param>
        /// <returns>long.</returns>
        public static long? ToUnixTimestampFromEst(this DateTime? estDateTime)
        {
            if (!estDateTime.HasValue)
            {
                return null;
            }

            return estDateTime.Value.ToUnixTimestampFromEst();
        }
    }
}