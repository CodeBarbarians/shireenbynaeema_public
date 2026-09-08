namespace Infrastructure
{
using Application;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Domain;
using SharedServices;

/// <summary>
/// Provides file upload, download, and header management services for Azure Blob Storage containers, including
/// automatic content type detection, unique naming, and public URL generation.
/// </summary>
/// <remarks>BlobFileService enables integration with Azure Blob Storage for handling files in a container. It
/// supports uploading files with automatic content type inference and timestamped naming to prevent collisions,
/// downloading files as streams, and updating HTTP headers for all blobs in the container. Public URLs are generated
/// for uploaded files, facilitating direct access. The service is designed for use in applications requiring reliable
/// blob storage operations, and it abstracts away Azure-specific details for ease of use.</remarks>
public class BlobFileService : IBlobFileService
{
    private readonly string connectionString;
    private readonly string containerName;
    private readonly string publicBlobBaseUrl;
    private readonly BlobContainerClient blobContainerClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlobFileService"/> class using the specified application settings configuration.
    /// </summary>
    /// <remarks>The BlobFileService requires Azure Blob Storage settings to be present in the configuration.
    /// If the configuration does not specify a connection string or container name, default empty values are used,
    /// which may result in an unusable service instance.</remarks>
    /// <param name="config">The application settings configuration containing Azure Blob Storage connection information. Must provide valid
    /// connection string and container name settings.</param>
    public BlobFileService(IAppSettingsConfig config)
    {
        this.connectionString = config.AzureBlobStorage?.ConnectionString ?? string.Empty;

        this.containerName = config.AzureBlobStorage?.ContainerName ?? string.Empty;

        this.blobContainerClient = new BlobContainerClient(this.connectionString, this.containerName);

        this.publicBlobBaseUrl = $"https://{this.blobContainerClient.AccountName}.blob.core.windows.net/{this.containerName}";
    }

    /// <summary>
    /// Gets the public base URL used for accessing blob storage resources.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the public base URL as a string.</returns>
    public Task<string> GetPublicBaseUrl() => Task.FromResult(this.publicBlobBaseUrl);

    /// <summary>
    /// Uploads a file to Azure Blob Storage with automatic content type detection, unique timestamped naming,
    /// and support for both <see cref="Stream"/> and byte array content.
    /// The uploaded file is stored in the specified blob container, and a public URL is generated for access.
    /// </summary>
    /// <param name="request">
    /// The <see cref="BlobUploadRequest"/> containing the blob name, optional directory path,
    /// and the file content to upload (either <see cref="BlobUploadRequest.StreamBlobContent"/> or <see cref="BlobUploadRequest.BlobContent"/>).
    /// </param>
    /// <returns>
    /// A <see cref="BlobUploadResponse"/> containing:
    /// <list type="bullet">
    ///   <item><c>IsSuccess</c>: True if the upload succeeded; false otherwise.</item>
    ///   <item><c>Message</c>: Success or error message describing the result.</item>
    ///   <item><c>BlobPath</c>: The full path of the uploaded blob in the container.</item>
    ///   <item><c>BlobUrl</c>: The public URL to access the uploaded file.</item>
    ///   <item><c>DocumentFormat</c>: The inferred document format based on the file extension.</item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// - Appends a UTC timestamp to the filename to ensure uniqueness.
    /// - Determines content type using <see cref="Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider"/>, with support for .heic, .heif, and .avif files.
    /// - Sets the blob <c>ContentDisposition</c> to "inline".
    /// - If both <c>StreamBlobContent</c> and <c>BlobContent</c> are null, the method returns a failure response.
    /// </remarks>
    public async Task<BlobUploadResponse> UploadFile(BlobUploadRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.BlobName))
        {
            return new BlobUploadResponse(false, "BlobName must be provided.");
        }

        try
        {
            // Determine content type
            // Determine content type
            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();

            // Add missing mappings
            provider.Mappings[".heic"] = "image/heic";
            provider.Mappings[".heif"] = "image/heif";
            provider.Mappings[".avif"] = "image/avif";

            // Try get the type
            if (!provider.TryGetContentType(request.BlobName, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            var headers = new BlobHttpHeaders
            {
                ContentType = contentType,
                ContentDisposition = "inline",
            };

            // ?? Extract path, filename, and extension
            var directory = Path.GetDirectoryName(request.BlobName)?.Replace("\\", "/");
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(request.BlobName);
            var extension = Path.GetExtension(request.BlobName);

            // ?? Append UTC timestamp (ensures uniqueness)
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var finalFileName = $"{fileNameWithoutExt}_{timestamp}{extension}";

            // ?? Combine path + new filename
            var blobPath = string.IsNullOrEmpty(directory)
                ? finalFileName
                : $"{directory}/{finalFileName}";

            // ?? Upload directly (no version check)
            var blobClient = this.blobContainerClient.GetBlobClient(blobPath);
            BlobContentInfo uploadResult;

            if (request.StreamBlobContent != null)
            {
                uploadResult = (await blobClient.UploadAsync(
                    request.StreamBlobContent,
                    new BlobUploadOptions { HttpHeaders = headers })).Value;
            }
            else if (request.BlobContent != null)
            {
                using var stream = request.BlobContent.ToMemoryStream();
                uploadResult = (await blobClient.UploadAsync(
                    stream,
                    new BlobUploadOptions { HttpHeaders = headers })).Value;
            }
            else
            {
                return new BlobUploadResponse(false, "No content provided for upload.");
            }

            var blobUrl = $"{this.publicBlobBaseUrl}/{blobPath}";
            var docFormat = this.GetDocFormatFromBlobName(request.BlobName);

            return new BlobUploadResponse(
                true,
                "File uploaded successfully.",
                blobPath,
                blobUrl,
                docFormat);
        }
        catch (Exception ex)
        {
            return new BlobUploadResponse(false, $"File upload failed. Reason: {ex.Message}");
        }
    }

    /// <summary>
    /// Downloads a file from Azure Blob Storage using the specified blob name.
    /// Automatically retrieves the blob content as a <see cref="Stream"/> for further processing or saving locally.
    /// </summary>
    /// <param name="request">
    /// The <see cref="BlobDownloadRequest"/> containing the <c>BlobName</c> to identify the file in the storage container.
    /// </param>
    /// <returns>
    /// A <see cref="BlobDownloadResponse"/> containing:
    /// <list type="bullet">
    ///   <item><c>IsSuccess</c>: True if the download succeeded; false otherwise.</item>
    ///   <item><c>Message</c>: Success or error message describing the result.</item>
    ///   <item><c>Content</c>: The <see cref="Stream"/> of the downloaded blob. Returns <see cref="Stream.Null"/> on failure.</item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// - If the <c>BlobName</c> is null or empty, the method returns a failure response immediately.
    /// - Exceptions during download are caught, and the response indicates failure with the exception message.
    /// </remarks>
    public async Task<BlobDownloadResponse> DownloadFile(BlobDownloadRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.BlobName))
        {
            return new BlobDownloadResponse(false, "BlobName must be provided.", Stream.Null);
        }

        try
        {
            var blobClient = this.blobContainerClient.GetBlobClient(request.BlobName);

            var blobDownloadInfo = await blobClient.DownloadAsync();

            return new BlobDownloadResponse(true, "File downloaded successfully.", blobDownloadInfo.Value.Content);
        }
        catch (Exception ex)
        {
            return new BlobDownloadResponse(false, $"File download failed. Reason: {ex.Message}", Stream.Null);
        }
    }

    /// <summary>
    /// Iterates through all blobs in the configured Azure Blob Storage container and updates their HTTP headers
    /// to ensure the correct Content Type and Content Disposition are set.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <remarks>
    /// - Uses <see cref="Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider"/> to infer content types.
    /// - Sets <c>ContentDisposition</c> to "inline" for all blobs.
    /// - Logs progress to the console, including successes and failures.
    /// - Updates all existing blobs; new blobs added after this method runs will not be affected.
    /// - Any blob that cannot have headers updated will be counted as failed, but the process continues for other blobs.
    /// </remarks>
    public async Task UpdateAllExistingBlobHeadersAsync()
    {
        Console.WriteLine("=== Starting blob header update process ===");

        var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
        int total = 0, updated = 0, failed = 0;

        await foreach (var blobItem in this.blobContainerClient.GetBlobsAsync())
        {
            total++;
            var blobClient = this.blobContainerClient.GetBlobClient(blobItem.Name);

            try
            {
                if (!provider.TryGetContentType(blobItem.Name, out var contentType))
                {
                    contentType = "application/octet-stream";
                }

                var headers = new BlobHttpHeaders
                {
                    ContentType = contentType,
                    ContentDisposition = "inline",
                };

                await blobClient.SetHttpHeadersAsync(headers);
                updated++;

                Console.WriteLine($"? Updated [{updated}/{total}]: {blobItem.Name} ? {contentType}");
            }
            catch (Exception ex)
            {
                failed++;
                Console.WriteLine($"? Failed [{failed}/{total}]: {blobItem.Name} ? {ex.Message}");
            }
        }

        Console.WriteLine("=== Blob header update completed ===");
        Console.WriteLine($"Total blobs: {total}");
        Console.WriteLine($"Updated successfully: {updated}");
        Console.WriteLine($"Failed: {failed}");
    }

    /// <summary>
    /// Determines the document format of a file based on its blob name's file extension.
    /// </summary>
    /// <param name="blobName">
    /// The name of the blob or file, including its extension (e.g., "document.pdf").
    /// </param>
    /// <returns>
    /// A <see cref="DocFormat"/> enum value corresponding to the file extension.
    /// Returns <see cref="DocFormat.Stream"/> if the extension cannot be mapped to a known format.
    /// </returns>
    /// <remarks>
    /// - The method ignores case when parsing the extension.
    /// - Leading periods ('.') in the extension are removed before parsing.
    /// - Common extensions like "pdf", "jpg", "png", etc., are mapped to the corresponding <see cref="DocFormat"/> values.
    /// </remarks>
    private DocFormat GetDocFormatFromBlobName(string blobName)
    {
        var extension = Path.GetExtension(blobName)?.TrimStart('.').ToLowerInvariant();
        return Enum.TryParse<DocFormat>(extension, true, out var format) ? format : DocFormat.Stream;
    }
}
}