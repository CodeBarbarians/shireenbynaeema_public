namespace Server
{
using Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ImageController : ControllerBase
{
    private readonly IImageService imageService;

    public ImageController(IImageService imageService) { this.imageService = imageService; }

    [HttpPost("Upload")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult> Upload(IFormFile file, [FromQuery] Guid? productId)
    {
        var response = await imageService.Upload(file, productId);
        return Ok(response);
    }

    [HttpPost("UploadCategory")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult> UploadCategory(IFormFile file, [FromQuery] Guid categoryId)
    {
        var response = await imageService.UploadCategoryImage(file, categoryId);
        return Ok(response);
    }

    [HttpDelete("DeleteProductImage/{id}")]
    public async Task<ActionResult> DeleteProductImage(Guid id)
    {
        var response = await imageService.DeleteProductImage(id);
        return Ok(response);
    }

    [HttpDelete("DeleteCategoryImage/{categoryId}")]
    public async Task<ActionResult> DeleteCategoryImage(Guid categoryId)
    {
        var response = await imageService.DeleteCategoryImage(categoryId);
        return Ok(response);
    }

    [HttpPost("UploadCategoryImages")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult> UploadCategoryImages(IFormFile file, [FromQuery] Guid categoryId)
    {
        var response = await imageService.UploadCategoryImages(file, categoryId);
        return Ok(response);
    }

    [HttpDelete("DeleteCategoryImageById/{id}")]
    public async Task<ActionResult> DeleteCategoryImageById(Guid id)
    {
        var response = await imageService.DeleteCategoryImageById(id);
        return Ok(response);
    }
}
}