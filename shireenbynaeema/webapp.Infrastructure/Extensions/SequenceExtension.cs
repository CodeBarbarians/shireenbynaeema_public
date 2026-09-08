namespace Infrastructure
{
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Provides extension methods for configuring entity properties to use database sequences as default values in
    /// Entity Framework Core models.
    /// </summary>
    /// <remarks>These extension methods simplify the process of associating a database sequence with an
    /// entity property, enabling automatic value generation for primary keys or other fields. This is particularly
    /// useful when working with SQL Server or other relational databases that support sequences. The methods are
    /// intended for use during model configuration in the Entity Framework Core fluent API.</remarks>
    public static class SequenceExtensions
    {
        /// <summary>
        /// Configures a database sequence and sets the specified property of the entity type to use the sequence as its
        /// default value.
        /// </summary>
        /// <remarks>This method creates a sequence in the database and configures the specified property
        /// to use the sequence for its default value. Use this to enable automatic value generation for properties such
        /// as primary keys or other numeric fields. The sequence is created with the specified start and increment
        /// values. If a schema is provided, the sequence is created in that schema; otherwise, the default schema is
        /// used.</remarks>
        /// <typeparam name="T">The entity type on which the property is configured.</typeparam>
        /// <param name="modelBuilder">The model builder used to configure the entity and sequence.</param>
        /// <param name="sequenceName">The name of the database sequence to create or configure.</param>
        /// <param name="propertyName">The name of the property on the entity type that will use the sequence as its default value.</param>
        /// <param name="start">The starting value for the sequence. Defaults to 1.</param>
        /// <param name="increment">The increment value for the sequence. Defaults to 1.</param>
        /// <param name="schema">The schema in which to create the sequence. If null, the default schema is used.</param>
        public static void UseSequence<T>(
            this ModelBuilder modelBuilder,
            string sequenceName,
            string propertyName,
            long start = 1,
            int increment = 1,
            string? schema = null)
            where T : class
        {
            if (schema == null)
            {
                modelBuilder.HasSequence<long>(sequenceName)
                    .StartsAt(start)
                    .IncrementsBy(increment);
            }
            else
            {
                modelBuilder.HasSequence<long>(sequenceName, schema)
                    .StartsAt(start)
                    .IncrementsBy(increment);
            }

            var entity = modelBuilder.Entity<T>();

            entity.Property(propertyName)
                  .HasDefaultValueSql(
                      schema == null
                          ? $"NEXT VALUE FOR {sequenceName}"
                          : $"NEXT VALUE FOR {schema}.{sequenceName}");
        }
    }
}