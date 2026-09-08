namespace SharedServices
{
    /// <summary>
    /// Extension methods for IEnumerable and IQueryable collections, providing utility functions for chunking and pagination. The Chunkify method allows you
    /// to split an IEnumerable collection into smaller chunks of a specified size, which can be useful for processing large datasets in manageable portions.
    /// The Paginate method enables you to apply pagination to an IQueryable collection based on the parameters defined in a ListRequest object, allowing for
    /// efficient retrieval of subsets of data in scenarios such as API responses or database queries. These extension methods enhance the functionality of
    /// collections by providing convenient ways to handle large datasets and implement pagination logic in a clean and reusable manner.
    /// </summary>
    public static class EnumerableExtension
    {
        /// <summary>
        /// Divides a sequence into consecutive chunks of a specified size, up to a given total number of elements.
        /// </summary>
        /// <remarks>Enumeration of the returned sequence is deferred and performed lazily. If totalSize
        /// is less than the number of elements in the source sequence, only the first totalSize elements are included
        /// in the output. If totalSize is zero, the result is an empty sequence.</remarks>
        /// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
        /// <param name="list">The sequence to divide into chunks.</param>
        /// <param name="totalSize">The total number of elements from the sequence to include in the output. Must be non-negative.</param>
        /// <param name="chunkSize">The maximum number of elements in each chunk. Must be greater than zero.</param>
        /// <returns>An enumerable of enumerables, where each inner enumerable contains up to chunkSize elements from the source
        /// sequence. The last chunk may contain fewer elements if the totalSize is not a multiple of chunkSize.</returns>
        public static IEnumerable<IEnumerable<T>> Chunkify<T>(this IEnumerable<T> list, int totalSize, int chunkSize)
        {
            int i = 0;
            while (i < totalSize)
            {
                yield return list.Skip(i).Take(chunkSize);
                i += chunkSize;
            }
        }

        /// <summary>
        /// Applies pagination to an IQueryable collection based on the parameters defined in a ListRequest object. This method takes an IQueryable collection and a ListRequest
        /// object as input and returns a paginated IQueryable collection.
        /// </summary>
        /// <typeparam name="T">T.</typeparam>
        /// <param name="list">list.</param>
        /// <param name="request">request.</param>
        /// <returns>IQueryable.</returns>
        public static IQueryable<T> Paginate<T>(this IQueryable<T> list, ListRequest request)
        {
            return list.Skip(request.Skip).Take(request.Take);
        }
    }
}