namespace SharedServices
{
    /// <summary>
    /// Extension methods for TimeSpan to convert durations into a more human-readable format. This class provides a method to format a TimeSpan instance into a
    /// string that represents the duration in terms of minutes, hours, and days, depending on the length of the duration. The formatting logic is designed to
    /// provide a clear and concise representation of the duration, making it easier for users to understand at a glance. For example, durations under an hour
    /// will be displayed in minutes, durations under a day will be displayed in hours and minutes, and durations over a day will be displayed in days and hours.
    /// This extension method can be particularly useful in scenarios where you want to display time intervals in a user-friendly way, such as in logs, reports,
    /// or user interfaces that show elapsed time or time remaining.
    /// </summary>
    public static class TimeSpanExtensions
    {
        /// <summary>
        /// Converts a TimeSpan duration into a human-readable string format. The method formats the duration based on its length.
        /// </summary>
        /// <param name="duration">duration.</param>
        /// <returns>string.</returns>
        public static string ToReadableFormat(this TimeSpan duration)
        {
            if (duration.TotalHours < 1)
            {
                // Under 1 hour → show in minutes
                return $"{(int)duration.TotalMinutes} min";
            }
            else if (duration.TotalDays < 1)
            {
                // Under 24 hours → show in hours and minutes
                var hours = (int)duration.TotalHours;
                var minutes = duration.Minutes;
                return minutes > 0
                    ? $"{hours} hr{(hours > 1 ? "s" : string.Empty)} {minutes} min"
                    : $"{hours} hr{(hours > 1 ? "s" : string.Empty)}";
            }
            else
            {
                // Over 24 hours → show in days and hours
                var days = (int)duration.TotalDays;
                var hours = duration.Hours;
                return hours > 0
                    ? $"{days} day{(days > 1 ? "s" : string.Empty)} {hours} hr{(hours > 1 ? "s" : string.Empty)}"
                    : $"{days} day{(days > 1 ? "s" : string.Empty)}";
            }
        }
    }
}