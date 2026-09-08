namespace Infrastructure
{
    using System.Linq.Expressions;

    using Domain;

    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Provides extension methods for performing duplicate checks on entities within a DbSet using specified property
    /// selectors.
    /// </summary>
    /// <remarks>These extensions are intended to help identify whether an entity with matching property
    /// values already exists in the database, excluding the entity itself when updating. The methods are useful for
    /// enforcing uniqueness constraints at the application level, especially when multiple properties are involved.
    /// Thread safety depends on the underlying DbSet implementation.</remarks>
    public static class DbSetDuplicateExtensions
    {
        /// <summary>
        /// Asynchronously checks whether the specified entity has duplicate values for the given properties within the
        /// database set.
        /// </summary>
        /// <remarks>When updating an existing entity, the check excludes the entity itself based on its
        /// <see cref="IIdentifiable.Id"/> property. Only properties with non-null values are checked for
        /// duplicates.</remarks>
        /// <typeparam name="TEntity">The type of the entity being checked. Must implement <see cref="IIdentifiable"/>.</typeparam>
        /// <param name="dbSet">The database set containing entities to check for duplicates.</param>
        /// <param name="entity">The entity instance to check for duplicate property values. Cannot be <see langword="null"/>.</param>
        /// <param name="extraFilter">An optional extra filter to apply when checking for duplicates.</param>
        /// <param name="properties">One or more property selectors indicating which properties to check for duplicates. Each selector should
        /// reference a property of <typeparamref name="TEntity"/>.</param>
        /// <returns>A <see cref="DuplicateCheckResult"/> indicating whether duplicates were found. If duplicates exist, the
        /// result contains the names of the duplicated properties; otherwise, it indicates no duplicates.</returns>
        public static async Task<DuplicateCheckResult> CheckDuplicateAsync<TEntity>(
    this DbSet<TEntity> dbSet,
    TEntity entity,
    Expression<Func<TEntity, bool>>? extraFilter = null,
    params Expression<Func<TEntity, object>>[] properties)
    where TEntity : class, IIdentifiable
        {
            ArgumentNullException.ThrowIfNull(entity);

            var duplicatedColumns = new List<string>();

            foreach (var selector in properties)
            {
                var member = GetMember(selector.Body);
                var propertyName = member.Member.Name;

                var propertyInfo = typeof(TEntity).GetProperty(propertyName)!;
                var value = propertyInfo.GetValue(entity);

                if (value == null)
                {
                    continue;
                }

                var parameter = Expression.Parameter(typeof(TEntity), "x");

                var left = Expression.Property(parameter, propertyName);
                var right = Expression.Constant(value, propertyInfo.PropertyType);

                Expression finalExpression = Expression.Equal(left, right);

                // Exclude current entity
                if (entity.Id != Guid.Empty)
                {
                    var idLeft = Expression.Property(parameter, nameof(IIdentifiable.Id));
                    var idRight = Expression.Constant(entity.Id);

                    var notSelf = Expression.NotEqual(idLeft, idRight);
                    finalExpression = Expression.AndAlso(finalExpression, notSelf);
                }

                // Apply extra filter (EF-safe parameter replacement)
                if (extraFilter != null)
                {
                    var replacedBody = ReplaceParameter(extraFilter.Body, extraFilter.Parameters[0], parameter);
                    finalExpression = Expression.AndAlso(finalExpression, replacedBody);
                }

                var lambda = Expression.Lambda<Func<TEntity, bool>>(finalExpression, parameter);

                var exists = await dbSet.AnyAsync(lambda).ConfigureAwait(false);

                if (exists)
                {
                    duplicatedColumns.Add(propertyName);
                }
            }

            if (duplicatedColumns.Count == 0)
            {
                return DuplicateCheckResult.NoDuplicate();
            }

            var message = $"{string.Join(", ", duplicatedColumns)} already exists.";
            return DuplicateCheckResult.Duplicate(message);
        }

        private static MemberExpression GetMember(Expression expression)
        {
            if (expression is UnaryExpression unary)
            {
                return (MemberExpression)unary.Operand;
            }

            return (MemberExpression)expression;
        }

        /// <summary>
        /// Replaces all occurrences of a specified parameter expression within an expression tree with another
        /// parameter expression.
        /// </summary>
        /// <param name="body">The expression tree in which to replace parameter expressions.</param>
        /// <param name="source">The parameter expression to be replaced within the expression tree.</param>
        /// <param name="target">The parameter expression to substitute for the source parameter expression.</param>
        /// <returns>An expression tree that is equivalent to the original, except with all instances of the source parameter
        /// replaced by the target parameter.</returns>
        private static Expression ReplaceParameter(Expression body, ParameterExpression source, ParameterExpression target)
        {
            return new ParameterReplaceVisitor(source, target).Visit(body)!;
        }

        /// <summary>
        /// Visits an expression tree and replaces all occurrences of a specified parameter with another parameter.
        /// </summary>
        /// <remarks>This visitor is typically used to substitute one parameter expression for another
        /// within an expression tree, such as when combining or modifying lambda expressions. The replacement is
        /// performed only for parameters that exactly match the specified source parameter.</remarks>
        private sealed class ParameterReplaceVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression source;
            private readonly ParameterExpression target;

            public ParameterReplaceVisitor(ParameterExpression source, ParameterExpression target)
            {
                this.source = source;
                this.target = target;
            }

            /// <summary>
            /// Visits the specified parameter expression and replaces it with the target parameter if it matches the
            /// source parameter.
            /// </summary>
            /// <param name="node">The parameter expression to visit and potentially replace.</param>
            /// <returns>The target parameter expression if <paramref name="node"/> matches the source parameter; otherwise, the
            /// result of visiting the parameter using the base implementation.</returns>
            protected override Expression VisitParameter(ParameterExpression node)
                => node == this.source ? this.target : base.VisitParameter(node);
        }
    }

    /// <summary>
    /// Represents the result of a duplicate check operation, indicating whether a duplicate was found and providing an
    /// associated message.
    /// </summary>
    /// <remarks>Use this type to convey the outcome of a check for duplicates, such as in validation or data
    /// processing scenarios. The result includes a flag indicating duplication and an optional message describing the
    /// context or reason.</remarks>
    public sealed class DuplicateCheckResult
    {
        /// <summary>
        /// Gets a value indicating whether the item is identified as a duplicate.
        /// </summary>
        public bool IsDuplicate { get; init; }

        /// <summary> Gets a message providing details about the duplicate check result, such as which properties are duplicated.</summary>
        public string Message { get; init; } = string.Empty;

        /// <summary> Creates a <see cref="DuplicateCheckResult"/> indicating that no duplicates were found. </summary>
        /// <returns>DuplicateCheckResult.</returns>
        public static DuplicateCheckResult NoDuplicate()
            => new() { IsDuplicate = false };

        /// <summary>
        /// Creates a new result indicating that the specified message is a duplicate.
        /// </summary>
        /// <param name="message">The message to be marked as a duplicate. Cannot be null.</param>
        /// <returns>A <see cref="DuplicateCheckResult"/> instance with <see cref="IsDuplicate"/> set to
        /// <see langword="true"/> and the specified message.</returns>
        public static DuplicateCheckResult Duplicate(string message)
        {
            return new() { IsDuplicate = true, Message = message };
        }
    }
}