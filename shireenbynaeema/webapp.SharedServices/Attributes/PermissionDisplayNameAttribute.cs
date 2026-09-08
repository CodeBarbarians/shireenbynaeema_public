namespace SharedServices
{
    using System.Reflection;

    /// <summary>
    /// Custom attribute to specify a display name for permissions defined in the Permissions class. This attribute can be applied to enum fields representing
    /// permissions to provide a user-friendly name that can be used in the UI or for logging purposes. By using this attribute, developers can easily associate
    /// a human-readable name with each permission, improving the clarity and maintainability of the code when working with permissions throughout the application.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class PermissionDisplayNameAttribute : Attribute
    {
        // Thread-safe lazy dictionary mapping permission key (e.g. "Data.Trade.Add") → display name (e.g. "Add Trade")
        private static readonly Lazy<Dictionary<string, string>> DisplayNameLookup =
            new(BuildDisplayNameLookup, LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionDisplayNameAttribute"/> class with the specified display name for a.
        /// permission.
        /// </summary>
        /// <param name="displayName">The display name to associate with the permission. Cannot be null or empty.</param>
        public PermissionDisplayNameAttribute(string displayName)
        {
            this.DisplayName = displayName;
        }

        /// <summary>
        /// Gets the display name associated with the object.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Resolves the human-readable display name for a given permission key by looking up
        /// the <see cref="PermissionDisplayNameAttribute"/> applied to the corresponding field in <see cref="Permissions"/>.
        /// Falls back to the raw permission key if no matching field or attribute is found.
        /// </summary>
        /// <param name="permissionKey">
        /// The permission identifier (e.g. <c>"Data.Trade.Add"</c>).
        /// </param>
        /// <returns>
        /// The display name (e.g. <c>"Add Trade"</c>), or <paramref name="permissionKey"/> if not resolved.
        /// </returns>
        public static string Resolve(string permissionKey)
        {
            return DisplayNameLookup.Value.TryGetValue(permissionKey, out var name)
                ? name
                : permissionKey;
        }

        /// <summary>
        /// Builds the permission key → display name dictionary by reflecting over all
        /// <see langword="const"/> string fields in the nested type hierarchy of <see cref="Permissions"/>.
        /// </summary>
        private static Dictionary<string, string> BuildDisplayNameLookup()
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var fields = typeof(Permissions)
                .GetNestedTypes(BindingFlags.Public)
                .SelectMany(g => g.GetNestedTypes(BindingFlags.Public))
                .SelectMany(m => m.GetFields(BindingFlags.Public | BindingFlags.Static))
                .Where(f => f.IsLiteral && f.FieldType == typeof(string));

            foreach (var field in fields)
            {
                var key = field.GetRawConstantValue()?.ToString();
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                var displayName = field.GetCustomAttribute<PermissionDisplayNameAttribute>()?.DisplayName ?? key;
                dict.TryAdd(key, displayName);
            }

            return dict;
        }
    }
}