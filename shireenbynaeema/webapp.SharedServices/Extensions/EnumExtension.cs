namespace SharedServices
{
    using System.ComponentModel.DataAnnotations;
    using System.Reflection;

    /// <summary>
    /// Provides extension methods for working with enumeration (enum) values, including retrieving display names
    /// defined by attributes.
    /// </summary>
    /// <remarks>This class contains static methods that extend the functionality of enum types, allowing
    /// developers to access additional metadata such as display names specified with attributes like <see
    /// cref="DisplayAttribute"/>. These methods are intended to simplify common
    /// enum-related operations in application code.</remarks>
    public static class EnumExtension
    {
        /// <summary>
        /// Retrieves the display name for the specified enumeration value, using the <see cref="DisplayAttribute"/> if
        /// present.
        /// </summary>
        /// <remarks>This method is typically used to obtain a user-friendly name for an enum value, such
        /// as for display in a UI. If the enum member is decorated with a <see cref="DisplayAttribute"/>, its
        /// <c>Name</c> property is returned; otherwise, the enum member's name is returned.</remarks>
        /// <param name="value">The enumeration value for which to obtain the display name.</param>
        /// <returns>A string containing the display name defined by the <see cref="DisplayAttribute"/> for the enumeration
        /// value, or the value's name if no display attribute is present.</returns>
        public static string GetDisplayName(this Enum value)
        {
            var member = value.GetType()
                .GetMember(value.ToString())
                .FirstOrDefault();

            if (member == null)
            {
                return value.ToString();
            }

            var attribute = member.GetCustomAttribute<DisplayAttribute>();

            return attribute?.Name ?? value.ToString();
        }
    }
}