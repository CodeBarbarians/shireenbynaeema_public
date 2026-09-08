namespace SharedServices
{
using System.Linq.Expressions;
using System.Reflection;

/// <summary>
/// Provides extension methods for dynamically ordering queryable and enumerable data sources based on property names
/// and sort directions specified at runtime.
/// </summary>
/// <remarks>The methods in this class enable flexible ordering of collections by allowing property names and sort
/// directions to be supplied dynamically, such as from user input or API requests. These extensions are particularly
/// useful for implementing server-side sorting in data-driven applications. Ordering is performed using reflection and
/// expression trees, and supports both ascending and descending order. When used with LINQ providers like Entity
/// Framework, ordering is translated to the underlying data source.</remarks>
public static class OrderByExtension
{
    /// <summary>
    /// Applies dynamic ordering to the queryable source based on the specified ordering criteria in the request.
    /// </summary>
    /// <remarks>If the OrderBy property in the request is null, empty, or consists only of whitespace, the
    /// original source is returned without modification. The ordering is applied using the property name and sort
    /// direction provided in the request. The method supports both ascending and descending order based on the SortBy
    /// value.</remarks>
    /// <typeparam name="T">The type of the elements in the source sequence. Must inherit from Base_Listing.</typeparam>
    /// <param name="source">The source queryable collection to which ordering will be applied.</param>
    /// <param name="request">The request containing ordering information, including the property name to order by and the sort direction.</param>
    /// <returns>An IQueryable&lt;T&gt; with ordering applied according to the specified property and sort direction. If no ordering is
    /// specified, returns the original source.</returns>
    /// <exception cref="InvalidOperationException">Thrown if T does not inherit from Base_Listing.</exception>
    /// <exception cref="ArgumentException">Thrown if the property specified in request.OrderBy does not exist on type T.</exception>
    public static IQueryable<T> ApplyOrdering<T>(this IQueryable<T> source, ListRequest request)
    {
        if (!typeof(T).IsSubclassOf(typeof(Base_Listing)))
        {
            string name = typeof(T).Name;
            throw new InvalidOperationException($"{name} does not contain Base Listing fields");
        }

        if (string.IsNullOrWhiteSpace(request.OrderBy))
        {
            return source;
        }

        var property = typeof(T).GetProperty(request.OrderBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        if (property == null)
        {
            throw new ArgumentException($"Property '{request.OrderBy}' not found on type '{typeof(T).Name}'");
        }

        var parameter = Expression.Parameter(typeof(T), "x");
        var propertyAccess = Expression.PropertyOrField(parameter, property.Name);
        var orderByExpression = Expression.Lambda(propertyAccess, parameter);

        string methodName = request.SortBy?.ToLowerInvariant() == "desc" ? "OrderByDescending" : "OrderBy";

        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            [typeof(T), property.PropertyType],
            source.Expression,
            Expression.Quote(orderByExpression));

        return source.Provider.CreateQuery<T>(resultExpression);
    }

    /// <summary>
    /// Sorts the elements of an <see cref="IQueryable{TEntity}"/> sequence according to a specified property name and
    /// sort direction.
    /// </summary>
    /// <remarks>This method enables dynamic ordering of queryable data sources based on property names
    /// provided at runtime. The ordering is performed on the server side when used with LINQ providers such as Entity
    /// Framework.</remarks>
    /// <typeparam name="TEntity">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The sequence of elements to order.</param>
    /// <param name="orderByProperty">The name of the property to use for sorting. Must correspond to a public property of <typeparamref
    /// name="TEntity"/>.</param>
    /// <param name="desc">A value indicating whether to sort in descending order. Pass <see langword="true"/> to sort descending;
    /// otherwise, <see langword="false"/> for ascending order.</param>
    /// <returns>An <see cref="IQueryable{TEntity}"/> whose elements are sorted according to the specified property and
    /// direction.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="orderByProperty"/> is null, empty, or does not correspond to a property on
    /// <typeparamref name="TEntity"/>.</exception>
    public static IQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> source, string orderByProperty, bool desc)
    {
        if (string.IsNullOrWhiteSpace(orderByProperty))
        {
            throw new ArgumentException("Property name cannot be null or empty.", nameof(orderByProperty));
        }

        var type = typeof(TEntity);

        var property = type.GetProperty(orderByProperty)
            ?? throw new ArgumentException(
                $"Property '{orderByProperty}' not found on type '{type.Name}'.");

        var parameter = Expression.Parameter(type, "p");
        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
        var orderByExpression = Expression.Lambda(propertyAccess, parameter);

        var command = desc ? "OrderByDescending" : "OrderBy";

        var resultExpression = Expression.Call(
            typeof(Queryable),
            command,
            new[] { type, property.PropertyType },
            source.Expression,
            Expression.Quote(orderByExpression));

        return source.Provider.CreateQuery<TEntity>(resultExpression);
    }

    /// <summary>
    /// Applies ordering to the sequence based on the specified property and sort direction from the request.
    /// </summary>
    /// <remarks>If the OrderBy property in the request is null, empty, or consists only of white-space
    /// characters, the original sequence is returned without ordering. The ordering is case-insensitive with respect to
    /// the property name.</remarks>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <param name="source">The sequence of elements to order.</param>
    /// <param name="request">The request containing ordering information, including the property name to order by and the sort direction. The
    /// OrderBy property specifies the property name, and SortBy determines the direction ('ascending' or 'descending').</param>
    /// <returns>An ordered sequence of elements based on the specified property and sort direction. If no ordering is specified,
    /// the original sequence is returned.</returns>
    /// <exception cref="Exception">Thrown if the type parameter T is a subclass of Base_Listing.</exception>
    /// <exception cref="InvalidDataException">Thrown if the property specified by request.OrderBy does not exist on the type T.</exception>
    public static IEnumerable<T> ApplyOrdering<T>(this IEnumerable<T> source, ListRequest request)
    {
        if (typeof(T).IsSubclassOf(typeof(Base_Listing)))
        {
            throw new InvalidDataException(message: $"{typeof(T).Name} does not contain Base Listing fields");
        }

        if (string.IsNullOrWhiteSpace(request.OrderBy))
        {
            return source;
        }

        var property = typeof(T).GetProperty(request.OrderBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        if (property == null)
        {
            throw new ArgumentException($"Property '{request.OrderBy}' not found on type '{typeof(T).Name}'");
        }

        Func<T, object> keySelector = x => property.GetValue(x, null)!;

        return request.SortBy?.ToLowerInvariant() == "ascending"
            ? source.OrderBy(keySelector)
            : source.OrderByDescending(keySelector);
    }
}
}