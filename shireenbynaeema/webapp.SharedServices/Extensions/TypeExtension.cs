namespace SharedServices
{
    using System.Reflection;

    /// <summary>
    /// Extension methods for the Type class to retrieve display names for entities.
    /// </summary>
    public static class TypeExtension
    {
        /// <summary>
        /// Retrieves the display name of an entity type based on the EntityDisplayNameAttribute. If the attribute is not present, it returns the name of the type itself.
        /// </summary>
        /// <param name="type">type.</param>
        /// <returns>string.</returns>
        public static string GetEntityDisplayName(this Type type)
        {
            var attr = type.GetCustomAttribute<EntityDisplayNameAttribute>();

            if (attr == null)
            {
                return type.Name;
            }

            return attr.DisplayName;
        }

        /// <summary>
        /// Retrieves the display name of an entity instance by calling the GetEntityDisplayName method on its type. This allows you to get a user-friendly name
        /// for the entity based on the EntityDisplayNameAttribute, or the type name if the attribute is not present.
        /// </summary>
        /// <typeparam name="T">T.</typeparam>
        /// <param name="instance">instance.</param>
        /// <returns>string.</returns>
        public static string GetEntityDisplayName<T>(this T instance)
        {
            return typeof(T).GetEntityDisplayName();
        }
    }
}