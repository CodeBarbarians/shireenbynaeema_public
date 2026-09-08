namespace SharedServices
{
    using System.ComponentModel;
    using System.Reflection;

    /// <summary>
    /// Centralized definition of all permissions in the application, organized by module and feature.
    /// </summary>
    /// <remarks>
    /// Permissions are structured as nested static classes following a <c>Module.Feature.Action</c> naming convention.
    /// Use <see cref="All"/> to retrieve a flat list of every permission string, or call
    /// <see cref="GetAllGrouped"/> / <see cref="GetGroupedForUser(IEnumerable{string})"/> for a hierarchical
    /// representation suitable for UI rendering. Classes decorated with <see cref="ModuleItselfAttribute"/>
    /// are flattened so the single inner type is treated as both the group and the module.
    /// </remarks>
    public static class Permissions
    {
        /// <summary>
        /// A flat, distinct list of every permission identifier defined across all nested module classes.
        /// </summary>
        /// <remarks>
        /// Built at static-initialization time via reflection over all <see langword="const"/> string fields
        /// in the nested type hierarchy.
        /// </remarks>
        public static readonly List<string> All =
            [.. typeof(Permissions)
            .GetNestedTypes()
            .SelectMany(g => g.GetNestedTypes())
            .SelectMany(m => m.GetFields())
            .Where(f => f.IsLiteral)
            .Select(f => f.GetRawConstantValue()!.ToString()!)
            .Distinct()];

        /// <summary>
        /// Returns every permission organized into <see cref="ModuleGroup"/> instances with their child
        /// <see cref="ModuleItem"/> and <see cref="PermissionItem"/> entries.
        /// </summary>
        /// <returns>A list of <see cref="ModuleGroup"/> representing the full permission tree.</returns>
        public static List<ModuleGroup> GetAllGrouped()
        {
            return Build();
        }

        /// <summary>
        /// Returns the permission tree filtered to include only the permissions present in
        /// <paramref name="userPermissions"/>. Groups and modules with no matching permissions are omitted.
        /// </summary>
        /// <param name="userPermissions">The set of permission identifiers assigned to the current user.</param>
        /// <returns>A filtered list of <see cref="ModuleGroup"/> containing only the user's permissions.</returns>
        public static List<ModuleGroup> GetGroupedForUser(IEnumerable<string> userPermissions)
        {
            return Build([.. userPermissions]);
        }

        /// <summary>
        /// Retrieves the display name for a type by reading its <see cref="DisplayNameAttribute"/>,
        /// falling back to <see cref="MemberInfo.Name"/> when the attribute is absent.
        /// </summary>
        /// <param name="t">The type whose display name is requested.</param>
        /// <returns>The display name string.</returns>
        private static string GetDisplayName(Type t)
        {
            return t.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? t.Name;
        }

        /// <summary>
        /// Retrieves the display name for a field by reading its <see cref="PermissionDisplayNameAttribute"/>,
        /// falling back to <see cref="MemberInfo.Name"/> when the attribute is absent.
        /// </summary>
        /// <param name="f">The field whose display name is requested.</param>
        /// <returns>The display name string.</returns>
        private static string GetDisplayName(FieldInfo f)
        {
            return f.GetCustomAttribute<PermissionDisplayNameAttribute>()?.DisplayName ?? f.Name;
        }

        /// <summary>
        /// Builds the hierarchical permission structure by reflecting over the nested types of <see cref="Permissions"/>.
        /// </summary>
        /// <param name="filter">
        /// An optional set of permission identifiers. When supplied, only permissions whose identifiers
        /// are contained in the set are included; when <see langword="null"/>, all permissions are returned.
        /// </param>
        /// <returns>A list of <see cref="ModuleGroup"/> instances representing the assembled permission tree.</returns>
        private static List<ModuleGroup> Build(HashSet<string>? filter = null)
        {
            var result = new List<ModuleGroup>();

            foreach (var groupType in typeof(Permissions).GetNestedTypes())
            {
                if (groupType == typeof(ModuleGroup) || groupType == typeof(ModuleItem) || groupType == typeof(PermissionItem))
                {
                    continue;
                }

                var isModuleItself = groupType.GetCustomAttribute<ModuleItselfAttribute>() != null;

                var group = new ModuleGroup
                {
                    Name = GetDisplayName(groupType),
                    IsModuleItself = isModuleItself,
                };

                if (isModuleItself)
                {
                    // Treat inner type as the module itself (flatten)
                    var moduleType = groupType.GetNestedTypes().FirstOrDefault();
                    if (moduleType == null)
                    {
                        continue;
                    }

                    var module = new ModuleItem
                    {
                        Name = GetDisplayName(moduleType),
                        Url = GetNavigation(moduleType),
                    };

                    foreach (var field in moduleType.GetFields(BindingFlags.Public | BindingFlags.Static))
                    {
                        if (!field.IsLiteral)
                        {
                            continue;
                        }

                        var id = field.GetRawConstantValue()!.ToString()!;
                        if (filter?.Contains(id) == false)
                        {
                            continue;
                        }

                        module.Permissions.Add(new PermissionItem
                        {
                            Id = id,
                            Name = GetDisplayName(field),
                        });
                    }

                    if (module.Permissions.Count != 0)
                    {
                        group.Modules.Add(module);
                    }
                }
                else
                {
                    // Default behavior (existing logic)
                    foreach (var moduleType in groupType.GetNestedTypes())
                    {
                        var module = new ModuleItem
                        {
                            Name = GetDisplayName(moduleType),
                            Url = GetNavigation(moduleType),
                        };

                        foreach (var field in moduleType.GetFields(BindingFlags.Public | BindingFlags.Static))
                        {
                            if (!field.IsLiteral)
                            {
                                continue;
                            }

                            var id = field.GetRawConstantValue()!.ToString()!;
                            if (filter?.Contains(id) == false)
                            {
                                continue;
                            }

                            module.Permissions.Add(new PermissionItem
                            {
                                Id = id,
                                Name = GetDisplayName(field),
                            });
                        }

                        if (module.Permissions.Count != 0)
                        {
                            group.Modules.Add(module);
                        }
                    }
                }

                if (group.Modules.Count != 0)
                {
                    result.Add(group);
                }
            }

            return result;
        }

        /// <summary>
        /// Retrieves the navigation URL for a type by reading its <see cref="NavigationAttribute"/>.
        /// </summary>
        /// <param name="t">The type whose navigation URL is requested.</param>
        /// <returns>The URL string, or <see langword="null"/> if the attribute is not present.</returns>
        private static string? GetNavigation(Type t)
        {
            return t.GetCustomAttribute<NavigationAttribute>()?.Url;
        }

        [DisplayName("Store")]
        public static class Store
        {
            [Navigation("/collections/all")]
            [DisplayName("Products")]
            public static class Products
            {
                [PermissionDisplayName("View Products")]
                public const string View = "Store.Products.View";
            }

            [DisplayName("Cart")]
            public static class Cart
            {
                [PermissionDisplayName("Manage Cart")]
                public const string Manage = "Store.Cart.Manage";
            }

            [DisplayName("Orders")]
            public static class Orders
            {
                [PermissionDisplayName("Place Orders")]
                public const string Place = "Store.Orders.Place";
            }

            [DisplayName("Account")]
            public static class Account
            {
                [PermissionDisplayName("Manage Account")]
                public const string Manage = "Store.Account.Manage";
            }
        }

        [DisplayName("Admin")]
        public static class Admin
        {
            [Navigation("/admin")]
            [DisplayName("Dashboard")]
            public static class Dashboard
            {
                [PermissionDisplayName("View Dashboard")]
                public const string View = "Admin.Dashboard.View";
            }

            [Navigation("/admin/products")]
            [DisplayName("Products")]
            public static class Products
            {
                [PermissionDisplayName("View Products")]
                public const string View = "Admin.Products.View";

                [PermissionDisplayName("Add Products")]
                public const string Add = "Admin.Products.Add";

                [PermissionDisplayName("Update Products")]
                public const string Update = "Admin.Products.Update";

                [PermissionDisplayName("Delete Products")]
                public const string Delete = "Admin.Products.Delete";
            }

            [Navigation("/admin/categories")]
            [DisplayName("Categories")]
            public static class Categories
            {
                [PermissionDisplayName("View Categories")]
                public const string View = "Admin.Categories.View";

                [PermissionDisplayName("Add Categories")]
                public const string Add = "Admin.Categories.Add";

                [PermissionDisplayName("Update Categories")]
                public const string Update = "Admin.Categories.Update";

                [PermissionDisplayName("Delete Categories")]
                public const string Delete = "Admin.Categories.Delete";
            }

            [Navigation("/admin/orders")]
            [DisplayName("Orders")]
            public static class Orders
            {
                [PermissionDisplayName("View Orders")]
                public const string View = "Admin.Orders.View";

                [PermissionDisplayName("Update Orders")]
                public const string Update = "Admin.Orders.Update";

                [PermissionDisplayName("Cancel Orders")]
                public const string Cancel = "Admin.Orders.Cancel";
            }

            [Navigation("/admin/customers")]
            [DisplayName("Customers")]
            public static class Customers
            {
                [PermissionDisplayName("View Customers")]
                public const string View = "Admin.Customers.View";

                [PermissionDisplayName("Update Customers")]
                public const string Update = "Admin.Customers.Update";
            }

            [Navigation("/admin/invoices")]
            [DisplayName("Invoices")]
            public static class Invoices
            {
                [PermissionDisplayName("View Invoices")]
                public const string View = "Admin.Invoices.View";

                [PermissionDisplayName("Generate Invoices")]
                public const string Generate = "Admin.Invoices.Generate";
            }

            [Navigation("/admin/deliveries")]
            [DisplayName("Deliveries")]
            public static class Deliveries
            {
                [PermissionDisplayName("View Deliveries")]
                public const string View = "Admin.Deliveries.View";

                [PermissionDisplayName("Update Deliveries")]
                public const string Update = "Admin.Deliveries.Update";
            }

            [Navigation("/admin/returns")]
            [DisplayName("Returns")]
            public static class Returns
            {
                [PermissionDisplayName("View Returns")]
                public const string View = "Admin.Returns.View";

                [PermissionDisplayName("Process Returns")]
                public const string Process = "Admin.Returns.Process";
            }

            [Navigation("/admin/users")]
            [DisplayName("Users")]
            public static class Users
            {
                [PermissionDisplayName("View Users")]
                public const string View = "Admin.Users.View";

                [PermissionDisplayName("Add Users")]
                public const string Add = "Admin.Users.Add";

                [PermissionDisplayName("Update Users")]
                public const string Update = "Admin.Users.Update";

                [PermissionDisplayName("Delete Users")]
                public const string Delete = "Admin.Users.Delete";

                [PermissionDisplayName("Manage User Roles")]
                public const string ManageRoles = "Admin.Users.ManageRoles";

                [PermissionDisplayName("Invite Users")]
                public const string Invite = "Admin.Users.Invite";

                [PermissionDisplayName("Manage User Permissions")]
                public const string ManagePermissions = "Admin.Users.ManagePermissions";
            }

            [Navigation("/admin/roles")]
            [DisplayName("Roles")]
            public static class Roles
            {
                [PermissionDisplayName("View Roles")]
                public const string View = "Admin.Roles.View";

                [PermissionDisplayName("Add Roles")]
                public const string Add = "Admin.Roles.Add";

                [PermissionDisplayName("Update Roles")]
                public const string Update = "Admin.Roles.Update";

                [PermissionDisplayName("Delete Roles")]
                public const string Delete = "Admin.Roles.Delete";
            }

            [DisplayName("Settings")]
            public static class Settings
            {
                [PermissionDisplayName("View Settings")]
                public const string View = "Admin.Settings.View";

                [PermissionDisplayName("Update Settings")]
                public const string Update = "Admin.Settings.Update";
            }
        }

        /// <summary>
        /// Represents a top-level permission group (e.g., "Operations", "Admin") that contains one or more
        /// <see cref="ModuleItem"/> entries.
        /// </summary>
        public class ModuleGroup
        {
            /// <summary>Gets or sets the display name of the group.</summary>
            public string Name { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets a value indicating whether this group is treated as a single flattened module
            /// (marked with <see cref="ModuleItselfAttribute"/>).
            /// </summary>
            public bool IsModuleItself { get; set; }

            /// <summary>Gets or sets the list of modules belonging to this group.</summary>
            public List<ModuleItem> Modules { get; set; } = [];
        }

        /// <summary>
        /// Represents a feature module within a <see cref="ModuleGroup"/> (e.g., "Work Orders", "Estimates")
        /// and its associated navigation URL and permission entries.
        /// </summary>
        public class ModuleItem
        {
            /// <summary>Gets or sets the display name of the module.</summary>
            public string Name { get; set; } = string.Empty;

            /// <summary>Gets or sets the front-end navigation URL for this module, or <see langword="null"/> if none is defined.</summary>
            public string? Url { get; set; }

            /// <summary>Gets or sets the list of individual permissions within this module.</summary>
            public List<PermissionItem> Permissions { get; set; } = [];
        }

        /// <summary>
        /// Represents a single permission entry with a unique identifier and a human-readable name.
        /// </summary>
        public class PermissionItem
        {
            /// <summary>Gets or sets the unique permission identifier (e.g., <c>"Admin.Users.View"</c>).</summary>
            public string Id { get; set; } = string.Empty;

            /// <summary>Gets or sets the human-readable display name for this permission.</summary>
            public string Name { get; set; } = string.Empty;
        }

        /// <summary>
        /// Groups a set of <see cref="PermissionItem"/> entries under a module name for a specific user.
        /// </summary>
        public class UserGroupedPermissions
        {
            /// <summary>Gets or sets the module name these permissions belong to.</summary>
            public string Module { get; set; } = string.Empty;

            /// <summary>Gets or sets the list of permissions the user holds within this module.</summary>
            public List<PermissionItem> Permissions { get; set; } = [];
        }
    }
}