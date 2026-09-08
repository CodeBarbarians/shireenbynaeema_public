namespace Infrastructure
{
    using Application;
    using Domain;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using SharedServices;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Formats.Webp;
    using SixLabors.ImageSharp.Processing;

    public class ImageService : IImageService
    {
        private readonly DatabaseContext db;
        private readonly IResponse resp;
        private readonly string uploadsRoot;
        private readonly IHttpContextAccessor httpContextAccessor;

        public ImageService(DatabaseContext db, IResponse response, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            this.db = db;
            this.resp = response;
            this.httpContextAccessor = httpContextAccessor;
            this.uploadsRoot = Path.Combine(env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsRoot);
        }

        private string GetBaseUrl()
        {
            var ctx = httpContextAccessor.HttpContext;
            if (ctx == null) return "";
            var request = ctx.Request;
            return $"{request.Scheme}://{request.Host}";
        }

        private async Task<string?> SaveToStorage(MemoryStream stream, string path)
        {
            var fullPath = Path.Combine(uploadsRoot, path);
            var directory = Path.GetDirectoryName(fullPath);
            if (directory != null)
                Directory.CreateDirectory(directory);

            await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await stream.CopyToAsync(fileStream);

            return $"{GetBaseUrl()}/uploads/{path}";
        }

        private async Task<MemoryStream> CompressImage(IFormFile file, int maxWidth)
        {
            using var originalStream = new MemoryStream();
            await file.CopyToAsync(originalStream);
            originalStream.Position = 0;

            using var image = Image.Load(originalStream);
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(maxWidth, maxWidth),
                Mode = ResizeMode.Max
            }));

            var compressedStream = new MemoryStream();
            await image.SaveAsWebpAsync(compressedStream, new WebpEncoder { Quality = 80 });
            compressedStream.Position = 0;
            return compressedStream;
        }

        public async Task<IResponse> Upload(IFormFile file, Guid? productId)
        {
            if (file == null || file.Length == 0)
            {
                resp.IsSuccess = false;
                resp.Message = "No file uploaded";
                return resp;
            }

            try
            {
                var path = productId.HasValue
                    ? $"products/{productId.Value}/{Guid.NewGuid()}.webp"
                    : $"products/{Guid.NewGuid()}.webp";

                using var compressedStream = await CompressImage(file, 1200);
                var url = await SaveToStorage(compressedStream, path);

                if (url == null)
                {
                    resp.IsSuccess = false;
                    resp.Message = "Failed to save file";
                    return resp;
                }

                if (productId.HasValue)
                {
                    var imageEntity = new ProductImage
                    {
                        Id = Guid.NewGuid(),
                        ProductId = productId.Value,
                        ImageUrl = url,
                        AltText = file.FileName,
                        SortOrder = await db.ProductImages.CountAsync(i => i.ProductId == productId.Value),
                        IsPrimary = await db.ProductImages.CountAsync(i => i.ProductId == productId.Value) == 0
                    };
                    db.ProductImages.Add(imageEntity);
                    await db.SaveChangesAsync();
                }

                resp.IsSuccess = true;
                resp.Data = new { url };
                return resp;
            }
            catch (Exception ex)
            {
                resp.IsSuccess = false;
                resp.Message = $"Upload failed: {ex.Message}";
                return resp;
            }
        }

        public async Task<IResponse> UploadCategoryImage(IFormFile file, Guid categoryId)
        {
            if (file == null || file.Length == 0)
            {
                resp.IsSuccess = false;
                resp.Message = "No file uploaded";
                return resp;
            }

            try
            {
                var path = $"categories/{Guid.NewGuid()}.webp";
                using var compressedStream = await CompressImage(file, 800);
                var url = await SaveToStorage(compressedStream, path);

                if (url == null)
                {
                    resp.IsSuccess = false;
                    resp.Message = "Failed to save file";
                    return resp;
                }

                var category = await db.Categories.FindAsync(categoryId);
                if (category != null)
                {
                    category.ImageUrl = url;
                    await db.SaveChangesAsync();
                }

                resp.IsSuccess = true;
                resp.Data = new { url };
                return resp;
            }
            catch (Exception ex)
            {
                resp.IsSuccess = false;
                resp.Message = $"Upload failed: {ex.Message}";
                return resp;
            }
        }

        public async Task<IResponse> UploadReturnAttachment(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                resp.IsSuccess = false;
                resp.Message = "No file uploaded";
                return resp;
            }

            try
            {
                var path = $"returns/{Guid.NewGuid()}.webp";
                using var compressedStream = await CompressImage(file, 1200);
                var url = await SaveToStorage(compressedStream, path);

                if (url == null)
                {
                    resp.IsSuccess = false;
                    resp.Message = "Failed to save file";
                    return resp;
                }

                resp.IsSuccess = true;
                resp.Data = new { url };
                return resp;
            }
            catch (Exception ex)
            {
                resp.IsSuccess = false;
                resp.Message = $"Upload failed: {ex.Message}";
                return resp;
            }
        }

        public async Task<IResponse> DeleteProductImage(Guid imageId)
        {
            var image = await db.ProductImages.FindAsync(imageId);
            if (image == null) { resp.IsSuccess = false; resp.Message = "Image not found"; return resp; }

            if (!string.IsNullOrEmpty(image.ImageUrl))
            {
                var relativePath = image.ImageUrl.Replace($"{GetBaseUrl()}/", "").TrimStart('/');
                var fullPath = Path.Combine(uploadsRoot, relativePath);
                if (File.Exists(fullPath)) File.Delete(fullPath);
            }

            db.ProductImages.Remove(image);
            await db.SaveChangesAsync();

            resp.IsSuccess = true;
            resp.Message = "Image deleted successfully";
            return resp;
        }

        public async Task<IResponse> DeleteCategoryImage(Guid categoryId)
        {
            var category = await db.Categories.FindAsync(categoryId);
            if (category == null) { resp.IsSuccess = false; resp.Message = "Category not found"; return resp; }

            if (!string.IsNullOrEmpty(category.ImageUrl))
            {
                var relativePath = category.ImageUrl.Replace($"{GetBaseUrl()}/", "").TrimStart('/');
                var fullPath = Path.Combine(uploadsRoot, relativePath);
                if (File.Exists(fullPath)) File.Delete(fullPath);
            }

            category.ImageUrl = "";
            await db.SaveChangesAsync();

            resp.IsSuccess = true;
            resp.Message = "Category image deleted successfully";
            return resp;
        }

        public async Task<IResponse> UploadCategoryImages(IFormFile file, Guid categoryId)
        {
            if (file == null || file.Length == 0)
            {
                resp.IsSuccess = false;
                resp.Message = "No file uploaded";
                return resp;
            }

            try
            {
                var path = $"categories/{categoryId}/{Guid.NewGuid()}.webp";
                using var compressedStream = await CompressImage(file, 800);
                var url = await SaveToStorage(compressedStream, path);

                if (url == null)
                {
                    resp.IsSuccess = false;
                    resp.Message = "Failed to save file";
                    return resp;
                }

                var existingCount = await db.CategoryImages.CountAsync(i => i.CategoryId == categoryId);
                var image = new CategoryImage
                {
                    Id = Guid.NewGuid(),
                    CategoryId = categoryId,
                    ImageUrl = url,
                    AltText = file.FileName,
                    SortOrder = existingCount,
                    IsPrimary = existingCount == 0
                };
                db.CategoryImages.Add(image);
                await db.SaveChangesAsync();

                resp.IsSuccess = true;
                resp.Data = new { url, image.Id };
                return resp;
            }
            catch (Exception ex)
            {
                resp.IsSuccess = false;
                resp.Message = $"Upload failed: {ex.Message}";
                return resp;
            }
        }

        public async Task<IResponse> DeleteCategoryImageById(Guid imageId)
        {
            var image = await db.CategoryImages.FindAsync(imageId);
            if (image == null) { resp.IsSuccess = false; resp.Message = "Image not found"; return resp; }

            if (!string.IsNullOrEmpty(image.ImageUrl))
            {
                var relativePath = image.ImageUrl.Replace($"{GetBaseUrl()}/", "").TrimStart('/');
                var fullPath = Path.Combine(uploadsRoot, relativePath);
                if (File.Exists(fullPath)) File.Delete(fullPath);
            }

            db.CategoryImages.Remove(image);
            await db.SaveChangesAsync();

            resp.IsSuccess = true;
            resp.Message = "Image deleted successfully";
            return resp;
        }
    }
}
