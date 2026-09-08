namespace Server
{
    using Application;
    using Domain;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using SharedServices;

    /// <summary>
    /// Provides API endpoints for uploading and downloading files to and from Azure Blob Storage. Requires
    /// authentication for all actions.
    /// </summary>
    /// <remarks>This controller supports file upload and download operations using Azure Blob Storage as the
    /// backend. All endpoints require the caller to be authenticated. Use this controller to manage file attachments in
    /// scenarios where secure, cloud-based storage is needed.</remarks>
    [Route("api/[controller]")]
    [Authorize]
    public class AttachmentController : ControllerBase
    {
        private readonly string? rootFolder;
        private readonly string filesFolder;
        private readonly IBlobFileService blobFileService;
        private readonly IResponse response;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttachmentController"/> class with the specified blob file service,.
        /// application settings, and response handler.
        /// </summary>
        /// <param name="blobFileService">The service used to manage blob file operations.</param>
        /// <param name="configuration">The application settings configuration containing Azure Blob Storage options. If null, default folder values
        /// are used.</param>
        /// <param name="response">The response handler used to generate HTTP responses.</param>
        public AttachmentController(IBlobFileService blobFileService, IAppSettingsConfig configuration, IResponse response)
        {
            this.blobFileService = blobFileService;
            this.rootFolder = configuration?.AzureBlobStorage.RootFolder ?? string.Empty;
            this.filesFolder = configuration?.AzureBlobStorage.FilesFolder ?? string.Empty;
            this.response = response;
        }

        /// <summary>
        /// Handles HTTP POST request to upload a file to Azure Blob Storage.
        /// </summary>
        /// <param name="file">The file sent from the client as form-data.</param>
        /// <returns>
        /// Returns 200 OK with the uploaded file URL if successful;
        /// otherwise returns 400 BadRequest with an error message.
        /// </returns>
        [HttpPost]
        [Route("UploadFileToBlob")]
        public async Task<ActionResult> UploadFileToBlob(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return this.BadRequest(this.response.SetFailure("No file uploaded."));
            }

            var fileName = $"{file.FileName}";
            var blobURL = $"{this.rootFolder}/{this.filesFolder}/{fileName}";

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var uploadResponse = await this.blobFileService.UploadFile(new BlobUploadRequest
                {
                    StreamBlobContent = memoryStream,
                    BlobName = blobURL,
                });

                if (uploadResponse.Success)
                {
                    var response = new AttachmentResponse
                    {
                        IsSuccess = true,
                        Url = uploadResponse.BlobUrl,
                        PreviewUrl = uploadResponse.BlobUrl,
                    };

                    return this.Ok(response);
                }

#pragma warning disable CA2201 // Do not raise reserved exception types
                return this.BadRequest(new AttachmentResponse
                {
                    IsSuccess = false,
                    Error = new SharedServices.Error(ex: new Exception(uploadResponse.Message)),
                });
#pragma warning restore CA2201 // Do not raise reserved exception types
            }
        }

        /// <summary>
        /// Handles HTTP POST request to upload a file to Azure Blob Storage
        /// and returns detailed information about the uploaded file.
        /// </summary>
        /// <param name="file">The file sent from the client as form-data.</param>
        /// <returns>
        /// Returns 200 OK with detailed file information if successful;
        /// otherwise returns 400 BadRequest with an error message.
        /// </returns>
        [HttpPost]
        [Route("UploadFileWithDetails")]
        public async Task<ActionResult> UploadFileWithDetails(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return this.BadRequest(this.response.SetFailure("No file uploaded."));
            }

            var fileName = $"{file.FileName}";
            var blobURL = $"{this.rootFolder}/{this.filesFolder}/{fileName}";

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0; // Reset before upload

                var uploadResponse = await this.blobFileService.UploadFile(new BlobUploadRequest
                {
                    StreamBlobContent = memoryStream,
                    BlobName = blobURL,
                });

                if (uploadResponse.Success)
                {
                    var response = new
                    {
                        FileName = file.FileName,
                        Extension = Path.GetExtension(file.FileName),
                        Path = uploadResponse.BlobName,
                        PreviewUrl = uploadResponse.BlobUrl,
                    };

                    return this.Ok(response);
                }
                else
                {
                    return this.BadRequest(uploadResponse);
                }
            }
        }

        /// <summary>
        /// Handles HTTP POST request to download a file from Azure Blob Storage
        /// based on the provided blob name or file details.
        /// </summary>
        /// <param name="request">
        /// The download request containing the blob name, file name,
        /// and optional extension information.
        /// </param>
        /// <returns>
        /// Returns the requested file as a downloadable stream if found;
        /// otherwise returns an appropriate error response.
        /// </returns>
        [HttpPost]
        [Route("DownloadFileFromBlob")]
        public async Task<IActionResult> DownloadFileFromBlob([FromBody] BlobDownloadRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.BlobName) && string.IsNullOrWhiteSpace(request.FileName))
                {
                    return this.BadRequest(this.response.SetFailure("Either BlobUrl or BlobName must be provided."));
                }

                var downloadResponse = await this.blobFileService.DownloadFile(request);

                if (!downloadResponse.Success || downloadResponse.BlobContent == null)
                {
                    return this.NotFound(this.response.SetFailure(downloadResponse.Message));
                }

                // Determine content type based on extension
                var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
                var fileName = request.FileName;

                // Ensure file name has extension
                if (!string.IsNullOrWhiteSpace(request.Extension) && !fileName.EndsWith(request.Extension, StringComparison.OrdinalIgnoreCase))
                {
                    fileName += request.Extension;
                }

                // Try to get MIME type
                if (!provider.TryGetContentType(fileName, out var contentType))
                {
                    contentType = "application/octet-stream";
                }

                // Return file stream as download
                return this.File(downloadResponse.BlobContent, contentType, fileName);
            }
            catch (Exception ex)
            {
                return this.BadRequest(this.response.SetFailure(ex.Message));
            }
        }
    }
}