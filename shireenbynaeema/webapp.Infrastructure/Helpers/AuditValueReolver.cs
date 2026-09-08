namespace Infrastructure
{
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Reflection;

    using Domain;
    using Microsoft.EntityFrameworkCore;
    using SharedServices;

    /// <summary>
    /// Helper class to resolve audit log values into human-readable formats. Uses <see cref="AuditDisplayAttribute"/>
    /// metadata to resolve foreign key references and enum display names. Batch-preloads all referenced entities
    /// per audit log entry to avoid N+1 query patterns.
    /// </summary>
    public class AuditValueResolver
    {
        private readonly DatabaseContext db;
        private readonly AuditLookupCache cache;

        /// <summary>
        /// Cached property metadata per entity type name. Built once per entity type and reused across calls.
        /// </summary>
        private readonly Dictionary<string, Dictionary<string, PropertyMeta>> metadataCache = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditValueResolver"/> class using the specified database context and audit lookup
        /// cache.
        /// </summary>
        /// <param name="db">The database context used to access and query audit-related data.</param>
        /// <param name="cache">The cache used to optimize audit value lookups and reduce database queries.</param>
        public AuditValueResolver(DatabaseContext db, AuditLookupCache cache)
        {
            this.db = db;
            this.cache = cache;
        }

        /// <summary>
        /// Pre-loads all foreign key entity display values for the given audit entries in batch queries,
        /// grouped by target entity type. This must be called before <see cref="ResolveAsync"/> to avoid N+1 queries.
        /// </summary>
        /// <param name="entityName">The CLR type name of the audited entity (e.g. "Organization").</param>
        /// <param name="entries">The parsed audit entries whose FK values need resolution.</param>
        /// <returns>A task representing the asynchronous preload operation.</returns>
        public async Task PreloadAsync(string? entityName, List<AuditEntry> entries)
        {
            var metadata = this.GetPropertyMetadata(entityName);
            if (metadata == null || entries.Count == 0)
            {
                return;
            }

            // Collect all FK GUIDs grouped by (TargetEntity, DisplayProperty)
            var fkGroups = new Dictionary<(Type TargetEntity, string DisplayProperty), HashSet<Guid>>();

            foreach (var entry in entries)
            {
                if (!metadata.TryGetValue(entry.PropertyName, out var meta) || meta.Attribute?.TargetEntity == null)
                {
                    continue;
                }

                var key = (meta.Attribute.TargetEntity, meta.Attribute.DisplayProperty!);
                if (!fkGroups.TryGetValue(key, out var ids))
                {
                    ids = [];
                    fkGroups[key] = ids;
                }

                CollectGuid(entry.OldValue, ids);
                CollectGuid(entry.NewValue, ids);
            }

            // Also collect user GUIDs for "By" / "User" properties
            var userIds = new HashSet<Guid>();
            foreach (var entry in entries)
            {
                if (entry.PropertyName.EndsWith("By", StringComparison.InvariantCulture) ||
                    entry.PropertyName.Contains("User"))
                {
                    CollectGuid(entry.OldValue, userIds);
                    CollectGuid(entry.NewValue, userIds);
                }
            }

            // Batch-load users
            if (userIds.Count > 0)
            {
                await this.BatchLoadUsers(userIds);
            }

            // Batch-load each FK entity group
            foreach (var ((targetEntity, displayProperty), ids) in fkGroups)
            {
                if (ids.Count > 0)
                {
                    await this.BatchLoadEntities(targetEntity, displayProperty, ids);
                }
            }
        }

        /// <summary>
        /// Resolves a display-friendly string representation for a property value, using <see cref="AuditDisplayAttribute"/>
        /// metadata when available.
        /// </summary>
        /// <param name="entityName">The CLR type name of the audited entity (e.g. "Organization"). Used to look up property metadata.</param>
        /// <param name="property">The name of the property whose value is being resolved.</param>
        /// <param name="value">The value to resolve for display. Can be null.</param>
        /// <returns>A resolved display string, or "—" if the value is null or empty.</returns>
        public async Task<string?> ResolveAsync(string? entityName, string property, object? value)
        {
            if (value == null)
            {
                return "—";
            }

            var str = value.ToString();
            if (string.IsNullOrWhiteSpace(str))
            {
                return "—";
            }

            // Check AuditDisplayAttribute metadata first
            var metadata = this.GetPropertyMetadata(entityName);
            if (metadata != null && metadata.TryGetValue(property, out var meta))
            {
                // ================= ENUM via AuditDisplay =================
                if (meta.Attribute!.UseEnumDisplay)
                {
                    var enumResult = ResolveEnumDisplayName(meta.PropertyType, str);
                    if (enumResult != null)
                    {
                        return enumResult;
                    }
                }

                // ================= FK ENTITY via AuditDisplay =================
                if (meta.Attribute.TargetEntity != null && Guid.TryParse(str, out var fkId))
                {
                    return this.ResolveCachedEntity(meta.Attribute.TargetEntity.Name, fkId);
                }
            }

            // ================= TIMESTAMP =================
            if (property.EndsWith("On", StringComparison.InvariantCulture) || property.Contains("Date") || (property.Contains("Time") && !property.Contains("TimeZone")))
            {
                if (long.TryParse(str, out var unix))
                {
                    return unix.ToFormattedDateTime("yyyy-MM-dd HH:mm:ss");
                }
            }

            // ================= USER =================
            if (property.EndsWith("By", StringComparison.InvariantCulture) || property.Contains("User"))
            {
                if (Guid.TryParse(str, out var userId))
                {
                    return await this.ResolveUser(userId);
                }
            }

            // ================= FK ENTITY (convention fallback) =================
            if (Guid.TryParse(str, out var entityId))
            {
                return await this.ResolveEntityByConvention(property, entityId);
            }

            // ================= BOOLEAN =================
            if (str == "True")
            {
                return "Yes";
            }

            if (str == "False")
            {
                return "No";
            }

            return str;
        }

        // -------------------------------------------------------
        // ENUM RESOLUTION
        // -------------------------------------------------------

        /// <summary>
        /// Resolves an enum value's display name using the <see cref="DisplayAttribute"/> on the enum member.
        /// Handles both raw integer values and named enum values.
        /// </summary>
        /// <param name="propertyType">The CLR type of the property (may be nullable).</param>
        /// <param name="rawValue">The raw string value from the audit log.</param>
        /// <returns>The display name, or the raw value if the enum cannot be resolved.</returns>
        private static string? ResolveEnumDisplayName(Type propertyType, string rawValue)
        {
            var enumType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            if (!enumType.IsEnum)
            {
                return null;
            }

            // Try parse as integer first (audit logs often store the numeric value)
            object? enumObj = null;
            if (int.TryParse(rawValue, out var intVal))
            {
                if (Enum.IsDefined(enumType, intVal))
                {
                    enumObj = Enum.ToObject(enumType, intVal);
                }
            }
            else if (Enum.TryParse(enumType, rawValue, ignoreCase: true, out var parsed))
            {
                enumObj = parsed;
            }

            if (enumObj is not Enum enumValue)
            {
                return rawValue;
            }

            return enumValue.GetDisplayName();
        }

        /// <summary>
        /// Collects a GUID from the given value into the target set.
        /// </summary>
        /// <param name="value">The value to attempt parsing as a GUID.</param>
        /// <param name="target">The set to add the parsed GUID to.</param>
        private static void CollectGuid(object? value, HashSet<Guid> target)
        {
            if (value != null && Guid.TryParse(value.ToString(), out var id))
            {
                target.Add(id);
            }
        }

        // -------------------------------------------------------
        // PROPERTY METADATA (from AuditDisplayAttribute)
        // -------------------------------------------------------

        /// <summary>
        /// Builds or retrieves cached property metadata for the given entity type.
        /// </summary>
        /// <param name="entityName">The CLR type name of the entity.</param>
        /// <returns>A dictionary mapping property names to their metadata, or null if entity not found.</returns>
        private Dictionary<string, PropertyMeta>? GetPropertyMetadata(string? entityName)
        {
            if (string.IsNullOrWhiteSpace(entityName))
            {
                return null;
            }

            if (this.metadataCache.TryGetValue(entityName, out var cached))
            {
                return cached;
            }

            var entityType = this.db.Model.GetEntityTypes()
                .FirstOrDefault(e => e.ClrType.Name == entityName);

            if (entityType == null)
            {
                return null;
            }

            var map = new Dictionary<string, PropertyMeta>();
            foreach (var prop in entityType.ClrType.GetProperties())
            {
                var attr = prop.GetCustomAttribute<AuditDisplayAttribute>();
                if (attr != null)
                {
                    map[prop.Name] = new PropertyMeta(attr, prop.PropertyType);
                }
            }

            this.metadataCache[entityName] = map;
            return map;
        }

        // -------------------------------------------------------
        // BATCH USER LOADING
        // -------------------------------------------------------

        /// <summary>
        /// Batch-loads user display names into the cache for all specified user IDs.
        /// </summary>
        /// <param name="ids">The set of user IDs to load.</param>
        /// <returns>A task representing the asynchronous batch load operation.</returns>
        private async Task BatchLoadUsers(HashSet<Guid> ids)
        {
            // Filter out already-cached IDs
            var needed = ids.Where(id => !this.cache.Users.ContainsKey(id)).ToList();
            if (needed.Count == 0)
            {
                return;
            }

            var users = await this.db.Users
                .Where(u => needed.Contains(u.Id))
                .Select(u => new { u.Id, Name = u.FirstName + " " + u.LastName })
                .ToListAsync();

            foreach (var u in users)
            {
                this.cache.Users[u.Id] = u.Name;
            }

            // Mark missing users
            foreach (var id in needed)
            {
                this.cache.Users.TryAdd(id, "(Deleted User)");
            }
        }

        // -------------------------------------------------------
        // USER LOOKUP (single, uses cache)
        // -------------------------------------------------------

        /// <summary>
        /// Resolves the display name of a user by their unique identifier, using the cache.
        /// </summary>
        /// <param name="id">The unique identifier of the user to resolve.</param>
        /// <returns>A string containing the user's display name. Returns "(Deleted User)" if the user does not exist.</returns>
        private async Task<string> ResolveUser(Guid id)
        {
            if (this.cache.Users.TryGetValue(id, out var name))
            {
                return name;
            }

            name = await this.db.Users
                .Where(x => x.Id == id)
                .Select(x => x.FirstName + " " + x.LastName)
                .FirstOrDefaultAsync()
                ?? "(Deleted User)";

            this.cache.Users[id] = name;
            return name;
        }

        // -------------------------------------------------------
        // BATCH ENTITY LOADING (via AuditDisplayAttribute)
        // -------------------------------------------------------

        /// <summary>
        /// Batch-loads entity display values for a set of IDs using the exact target entity type and display property
        /// specified by <see cref="AuditDisplayAttribute"/>. Uses a dynamic WHERE IN query to load all needed entities
        /// in a single database round-trip, then extracts display values via reflection.
        /// </summary>
        /// <param name="targetEntity">The CLR type of the target entity.</param>
        /// <param name="displayProperty">The property name to use for display.</param>
        /// <param name="ids">The set of entity IDs to load.</param>
        /// <returns>A task representing the asynchronous batch load operation.</returns>
        private async Task BatchLoadEntities(Type targetEntity, string displayProperty, HashSet<Guid> ids)
        {
            var entityTypeName = targetEntity.Name;

            if (!this.cache.Entities.TryGetValue(entityTypeName, out var map))
            {
                map = [];
                this.cache.Entities[entityTypeName] = map;
            }

            var needed = ids.Where(id => !map.ContainsKey(id)).ToList();
            if (needed.Count == 0)
            {
                return;
            }

            // Find the EF entity type
            var efEntityType = this.db.Model.GetEntityTypes()
                .FirstOrDefault(e => e.ClrType == targetEntity);

            if (efEntityType == null)
            {
                return;
            }

            var clrType = efEntityType.ClrType;
            var displayPropInfo = clrType.GetProperty(displayProperty);
            if (displayPropInfo == null)
            {
                return;
            }

            // Build: db.Set<T>().Where(e => needed.Contains(EF.Property<Guid>(e, "Id")))
            var setMethod = typeof(DbContext)
                .GetMethod(nameof(DbContext.Set), Type.EmptyTypes)!
                .MakeGenericMethod(clrType);

            var dbSet = (IQueryable)setMethod.Invoke(this.db, null)!;
            var param = Expression.Parameter(clrType, "e");

            var idAccess = Expression.Call(
                typeof(EF),
                nameof(EF.Property),
                [typeof(Guid)],
                param,
                Expression.Constant("Id"));

            var containsMethod = typeof(List<Guid>).GetMethod(nameof(List<Guid>.Contains), [typeof(Guid)])!;
            var containsCall = Expression.Call(Expression.Constant(needed), containsMethod, idAccess);
            var whereLambda = Expression.Lambda(containsCall, param);

            var whereCall = Expression.Call(
                typeof(Queryable),
                nameof(Queryable.Where),
                [clrType],
                dbSet.Expression,
                whereLambda);

            var filteredQuery = dbSet.Provider.CreateQuery(whereCall);

            // Materialize all matched entities and extract Id + display property via reflection
            var entities = await EntityFrameworkQueryableExtensions.ToListAsync((dynamic)filteredQuery);
            var idProp = clrType.GetProperty("Id");

            foreach (var entity in entities)
            {
                var entityId = (Guid)idProp!.GetValue(entity)!;
                var displayValue = displayPropInfo.GetValue(entity)?.ToString() ?? string.Empty;
                map[entityId] = displayValue;
            }
        }

        /// <summary>
        /// Looks up an already-cached entity display value. Returns a placeholder if not found.
        /// </summary>
        /// <param name="entityTypeName">The name of the entity type.</param>
        /// <param name="id">The entity ID to look up.</param>
        /// <returns>The cached display value or "(Reference Changed)".</returns>
        private string ResolveCachedEntity(string entityTypeName, Guid id)
        {
            if (this.cache.Entities.TryGetValue(entityTypeName, out var map) && map.TryGetValue(id, out var name))
            {
                return name;
            }

            return "(Reference Changed)";
        }

        // -------------------------------------------------------
        // CONVENTION-BASED ENTITY LOOKUP (fallback)
        // -------------------------------------------------------

        /// <summary>
        /// Resolves an entity display name using convention: strips "Id" from property name to find entity type,
        /// then picks a display property by searching for Name/Title/Code/Desc/Number.
        /// </summary>
        /// <param name="property">The property name (e.g. "OrganizationId").</param>
        /// <param name="id">The entity ID to resolve.</param>
        /// <returns>The display name or "(Reference Changed)".</returns>
        private async Task<string?> ResolveEntityByConvention(string property, Guid id)
        {
            var entityName = property.Replace("Id", string.Empty);

            if (!this.cache.Entities.TryGetValue(entityName, out var map))
            {
                map = [];
                this.cache.Entities[entityName] = map;
            }

            if (map.TryGetValue(id, out var cached))
            {
                return cached;
            }

            var name = await this.LookupEntityNameByConvention(entityName, id);
            if (name != null)
            {
                map[id] = name;
            }

            return name ?? "(Reference Changed)";
        }

        /// <summary>
        /// Retrieves the display name of an entity instance by convention: searches for a property whose name
        /// contains 'Name', 'Title', 'Code', 'Desc', or 'Number'.
        /// </summary>
        /// <param name="entityName">The entity type name derived from the property name.</param>
        /// <param name="id">The unique identifier of the entity instance.</param>
        /// <returns>The display name if found; otherwise, null.</returns>
        private async Task<string?> LookupEntityNameByConvention(string entityName, Guid id)
        {
            var entityType = this.db.Model.GetEntityTypes()
                .FirstOrDefault(e => e.ClrType.Name == entityName);
            entityType ??= this.db.Model.GetEntityTypes()
                .FirstOrDefault(e => e.ClrType.Name.Contains(entityName));

            if (entityType == null)
            {
                return null;
            }

            var clrType = entityType.ClrType;

            var setMethod = typeof(DbContext)
                .GetMethod(nameof(DbContext.Set), Type.EmptyTypes)!
                .MakeGenericMethod(clrType);

            var query = (IQueryable)setMethod.Invoke(this.db, null)!;

            var parameter = Expression.Parameter(clrType, "e");

            var idProperty = Expression.Call(
                typeof(EF),
                nameof(EF.Property),
                [typeof(Guid)],
                parameter,
                Expression.Constant("Id"));

            var condition = Expression.Equal(idProperty, Expression.Constant(id));
            var lambda = Expression.Lambda(condition, parameter);

            var whereCall = Expression.Call(
                typeof(Queryable),
                nameof(Queryable.Where),
                [clrType],
                query.Expression,
                lambda);

            var filteredQuery = query.Provider.CreateQuery(whereCall);

            var entity = await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync((dynamic)filteredQuery);
            if (entity == null)
            {
                return null;
            }

            // Pick display column automatically
            var displayProp = clrType.GetProperties()
                .FirstOrDefault(p =>
                    p.Name.Contains("Name", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Contains("Title", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Contains("Code", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Contains("Desc", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Contains("Number", StringComparison.OrdinalIgnoreCase));

            return displayProp?.GetValue(entity)?.ToString();
        }

        private record PropertyMeta(AuditDisplayAttribute Attribute, Type PropertyType);
    }
}