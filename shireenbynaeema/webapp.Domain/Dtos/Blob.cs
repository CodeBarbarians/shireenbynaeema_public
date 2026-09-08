namespace Domain
{
    using SharedServices;

    /// <summary>
    /// Request model used to upload content to blob storage.
    /// Supports both text-based and stream-based uploads.
    /// </summary>
    public class BlobUploadRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlobUploadRequest"/> class.
        /// Initializes a new upload request using text content.
        /// </summary>
        /// <param name="blobName">blobMame.</param>
        /// <param name="blobContent">blobContent.</param>
        /// <param name="baseUrl">baseUrl.</param>
        public BlobUploadRequest(string blobName, string blobContent, string baseUrl)
        {
            this.BlobName = blobName ?? throw new ArgumentNullException(nameof(blobName));
            this.BlobContent = blobContent ?? throw new ArgumentNullException(nameof(blobContent));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobUploadRequest"/> class.
        /// Default constructor for manual property assignment.
        /// </summary>
        public BlobUploadRequest()
        {
            this.BlobName = string.Empty;
            this.BlobContent = string.Empty;
        }

        /// <summary>Gets or sets virtual path or filename inside the container.</summary>
        public string BlobName { get; set; } = string.Empty;

        /// <summary>Gets or sets text content to upload (used for JSON or text files).</summary>
        public string BlobContent { get; set; } = string.Empty;

        /// <summary>Gets or sets binary stream content for file uploads.</summary>
        public Stream? StreamBlobContent { get; set; }
    }

    /// <summary>
    /// Response returned after uploading a blob.
    /// </summary>
    public class BlobUploadResponse
    {
        /// <summary>Initializes a new instance of the <see cref="BlobUploadResponse"/> class.Creates a successful upload response with metadata.</summary>
        /// <param name="success">success.</param>
        /// <param name="message">message.</param>
        /// <param name="blobName">blobName.</param>
        /// <param name="blobUrl">blobUrl.</param>
        /// <param name="docFormat">docFormat.</param>
        public BlobUploadResponse(bool success, string message, string blobName, string blobUrl, DocFormat docFormat)
        {
            this.Success = success;
            this.Message = message ?? string.Empty;
            this.BlobName = blobName ?? string.Empty;
            this.BlobUrl = blobUrl ?? string.Empty;
            this.DocFormat = docFormat;
        }

        /// <summary>Initializes a new instance of the <see cref="BlobUploadResponse"/> class.Creates a basic upload response.</summary>
        /// <param name="success">success.</param>
        /// <param name="message">message.</param>
        public BlobUploadResponse(bool success, string message)
        {
            this.Success = success;
            this.Message = message ?? string.Empty;
            this.BlobName = string.Empty;
            this.BlobUrl = string.Empty;
            this.DocFormat = default;
        }

        /// <summary>Gets or sets a value indicating whether indicates whether upload succeeded.</summary>
        public bool Success { get; set; }

        /// <summary>Gets or sets operation result message.</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>Gets or sets name of uploaded blob.</summary>
        public string BlobName { get; set; } = string.Empty;

        /// <summary>Gets or sets accessible URL of uploaded blob.</summary>
        public string BlobUrl { get; set; } = string.Empty;

        /// <summary>Gets or sets detected document format.</summary>
        public DocFormat DocFormat { get; set; }
    }

    /// <summary>
    /// Request model used to download a blob file.
    /// </summary>
    public class BlobDownloadRequest
    {
        /// <summary>Gets or sets name of the blob in storage.</summary>
        public string BlobName { get; set; } = string.Empty;

        /// <summary>Gets or sets desired filename for the downloaded file.</summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>Gets or sets file extension of the blob.</summary>
        public string Extension { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response returned when downloading a blob.
    /// </summary>
    public class BlobDownloadResponse
    {
        /// <summary>Initializes a new instance of the <see cref="BlobDownloadResponse"/> class.Creates a download response with blob content.</summary>
        /// <param name="success">success.</param>
        /// <param name="message">message.</param>
        /// <param name="blobContent">blobContent.</param>
        public BlobDownloadResponse(bool success, string message, Stream blobContent)
        {
            this.Success = success;
            this.Message = message ?? string.Empty;
            this.BlobContent = blobContent ?? throw new ArgumentNullException(nameof(blobContent));
        }

        /// <summary>Gets or sets a value indicating whether indicates whether download succeeded.</summary>
        public bool Success { get; set; }

        /// <summary>Gets or sets operation result message.</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>Gets or sets downloaded blob content stream.</summary>
        public Stream BlobContent { get; set; }
    }
}