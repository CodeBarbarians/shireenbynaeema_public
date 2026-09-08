namespace Application
{
    using Domain;

    /// <summary>
    /// Defines a contract for managing file operations in blob storage, including uploading, downloading, retrieving
    /// the public base URL, and updating blob headers.
    /// </summary>
    /// <remarks>Implementations of this interface provide asynchronous methods for interacting with blob
    /// storage. Typical usage scenarios include handling file uploads and downloads, generating public URLs for blob
    /// access, and maintaining blob metadata. Thread safety and performance characteristics depend on the specific
    /// implementation.</remarks>
    public interface IBlobFileService
    {
        /// <summary>
        /// Uploads a file to blob storage based on the provided BlobUploadRequest, which contains the necessary information for the upload process.
        /// </summary>
        /// <param name="request">The BlobUploadRequest containing file upload details such as file content, name, and destination path.</param>
        /// <returns>A Task that returns a BlobUploadResponse containing the upload result, including the file URL and metadata.</returns>
        public Task<BlobUploadResponse> UploadFile(BlobUploadRequest request);

        /// <summary>
        /// Downloads a file from blob storage based on the provided BlobDownloadRequest, which contains the necessary information for the download process.
        /// </summary>
        /// <param name="request">The BlobDownloadRequest containing download details such as the blob name or identifier.</param>
        /// <returns>A Task that returns a BlobDownloadResponse containing the downloaded file content and metadata.</returns>
        public Task<BlobDownloadResponse> DownloadFile(BlobDownloadRequest request);

        /// <summary>
        /// Retrieves the public base URL for accessing the blobs in storage. This method is used to obtain the base URL that can be used to
        /// construct the full URL for accessing individual blobs.
        /// </summary>
        /// <returns>A Task that returns the public base URL string for blob storage access.</returns>
        public Task<string> GetPublicBaseUrl();

        /// <summary>
        /// Updates the headers of all existing blobs in storage. This method is typically used to ensure that the metadata and
        /// access permissions of the blobs are up to date.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation of updating blob headers.</returns>
        public Task UpdateAllExistingBlobHeadersAsync();
    }
}