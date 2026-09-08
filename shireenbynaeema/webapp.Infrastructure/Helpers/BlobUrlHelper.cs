namespace Infrastructure
{
    /// <summary>
    /// Helper class for generating URL-safe blob URLs by encoding each segment of the blob path to handle special characters safely.
    /// This class provides a method to construct a fully qualified URL pointing to a blob in Azure Blob Storage, ensuring that all path segments are properly
    /// URL-encoded to prevent issues with special characters in blob names.
    /// The GetSafeBlobUrl method takes a base URL and a blob path, encodes each segment of the path, and combines it with the base URL to produce a safe and valid blob URL.
    /// This is essential for applications that need to generate URLs for blobs that may contain spaces or other special characters,
    /// ensuring that the generated URLs are correctly formatted and can be accessed without errors. In summary,
    /// the BlobUrlHelper class is a utility for creating safe and valid blob URLs by encoding each segment of the blob path,
    /// making it easier to work with blobs that have special characters in their names.
    /// </summary>
    public static class BlobUrlHelper
    {
        /// <summary>
        /// Generates a URL-safe version of a blob path by encoding each segment to handle special characters safely.
        /// </summary>
        /// <param name="baseBlobUrl">The base URL of the blob storage container.</param>
        /// <param name="blobPath">The relative path of the blob, potentially containing special characters.</param>
        /// <returns>A fully qualified URL pointing to the blob with all path segments properly URL-encoded.</returns>
        /// <exception cref="ArgumentException">Thrown if the base URL or blob path is null, empty, or whitespace.</exception>
        public static string GetSafeBlobUrl(string baseBlobUrl, string blobPath)
        {
            if (string.IsNullOrWhiteSpace(baseBlobUrl))
            {
                throw new ArgumentException("Base URL cannot be empty.", nameof(baseBlobUrl));
            }

            if (string.IsNullOrWhiteSpace(blobPath))
            {
                throw new ArgumentException("Blob path cannot be empty.", nameof(blobPath));
            }

            // Split the path by '/' and encode each segment
            var encodedSegments = blobPath
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Select(Uri.EscapeDataString);

            // Recombine the path
            string encodedPath = string.Join("/", encodedSegments);

            // Combine with base URL (ensure no double slash)
            return baseBlobUrl.TrimEnd('/') + "/" + encodedPath;
        }
    }
}