namespace Server
{
using Application;
using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedServices;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ReturnController : ControllerBase
{
    private readonly IReturnService returnService;
    private readonly IImageService imageService;

    public ReturnController(IReturnService returnService, IImageService imageService)
    {
        this.returnService = returnService;
        this.imageService = imageService;
    }

    [HttpPost("Request")]
    public async Task<ActionResult> RequestReturn(ReturnRequestDto request)
    {
        var response = await returnService.RequestReturn(CurrentUser.UserId!.Value, request);
        return Ok(response);
    }

    [HttpGet("MyReturns")]
    public async Task<ActionResult> GetMyReturns()
    {
        var response = await returnService.GetByUser(CurrentUser.UserId!.Value);
        return Ok(response);
    }

    [HttpPost("UploadAttachment")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult> UploadAttachment(IFormFile file)
    {
        var response = await imageService.UploadReturnAttachment(file);
        return Ok(response);
    }

    [HttpPost("Admin/List")]
    public async Task<ActionResult> List(ListRequest request)
    {
        var response = await returnService.ListReturns(request);
        return Ok(response);
    }

    [HttpGet("Admin/{id}")]
    public async Task<ActionResult> GetById(Guid id)
    {
        var response = await returnService.GetById(id);
        return Ok(response);
    }

    [HttpPut("Admin/Process/{id}")]
    public async Task<ActionResult> Process(Guid id, ReturnProcessDto request)
    {
        var response = await returnService.Process(id, request);
        return Ok(response);
    }

    [HttpDelete("Admin/Delete/{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var response = await returnService.Delete(id);
        return Ok(response);
    }
}
}
