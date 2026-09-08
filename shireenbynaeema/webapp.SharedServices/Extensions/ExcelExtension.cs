namespace SharedServices
{
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Text;

    using CsvHelper;
    using CsvHelper.Configuration;

    /// <summary>
    /// Extension methods for generating CSV files from lists of objects. This class provides a method to convert a list of any type into a CSV format,
    /// using the CsvHelper library for efficient CSV generation. The method dynamically maps object properties to CSV columns, allowing for flexible and
    /// reusable CSV generation across different types of data. The generated CSV includes a header row with property names or display names, and the
    /// data rows correspond to the values of the properties in the list. This extension is useful for exporting data in a format that can be easily
    /// opened in spreadsheet applications like Microsoft Excel or Google Sheets.
    /// </summary>
    public static class ExcelExtension
    {
        /// <summary>
        /// Generates a CSV file from a list of objects of type T. The method uses the CsvHelper library to create a CSV representation of the list,
        /// including a header row with property names or display names, and data rows corresponding to the property values.
        /// </summary>
        /// <typeparam name="T">T.</typeparam>
        /// <param name="list">list.</param>
        /// <returns>byte.</returns>
        public static byte[] GenerateCsvFromList<T>(this List<T> list)
        {
            if (list == null || list.Count == 0)
            {
                return [];
            }

            using var memory = new MemoryStream();
            using var writer = new StreamWriter(memory, new UTF8Encoding(true)); // BOM included
            using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Encoding = Encoding.UTF8,
                Quote = '"',
            });

            // Dynamically create a mapping from properties to DisplayName
            var properties = typeof(T).GetProperties().Where(p => p.CanRead).ToArray();
            foreach (var prop in properties)
            {
                var displayNameAttr = prop.GetCustomAttribute<DisplayNameAttribute>();
                var headerName = displayNameAttr != null ? displayNameAttr.DisplayName : prop.Name;
                csv.Context.RegisterClassMap(new DynamicClassMap<T>(properties));
                break; // register map only once
            }

            csv.WriteRecords(list);
            writer.Flush();
            return memory.ToArray();
        }

        // Helper: Dynamic CsvHelper ClassMap using DisplayName

        /// <summary>
        /// A dynamic ClassMap for CsvHelper that maps properties of type T to CSV columns, using the DisplayName attribute if available. This class is used to
        /// register the mapping for the CSV export.
        /// </summary>
        /// <typeparam name="T">T.</typeparam>
        public sealed class DynamicClassMap<T> : ClassMap<T>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DynamicClassMap{T}"/> class using the specified properties to define the.
            /// mapping.
            /// </summary>
            /// <remarks>If a property has a DisplayNameAttribute, its DisplayName is used as the
            /// column name in the mapping; otherwise, the property name is used. The order of properties in the array
            /// determines the order of the mapped columns.</remarks>
            /// <param name="properties">An array of PropertyInfo objects representing the properties to include in the mapping. Each property
            /// may have a DisplayNameAttribute to specify a custom column name.</param>
            public DynamicClassMap(PropertyInfo[] properties)
            {
                foreach (var prop in properties)
                {
                    var displayNameAttr = prop.GetCustomAttribute<DisplayNameAttribute>();
                    var name = displayNameAttr != null ? displayNameAttr.DisplayName : prop.Name;
                    this.Map(typeof(T), prop).Name(name);
                }
            }
        }
    }
}