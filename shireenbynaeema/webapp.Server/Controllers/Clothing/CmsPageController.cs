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
public class CmsPageController : ControllerBase
{
    private readonly ICmsPageService cmsPageService;

    public CmsPageController(ICmsPageService cmsPageService) { this.cmsPageService = cmsPageService; }

    [HttpPost("List")]
    [Authorize]
    public async Task<ActionResult> List(ListRequest request)
    {
        var response = await cmsPageService.ListPages(request);
        return Ok(response);
    }

    [HttpGet("BySlug/{slug}")]
    public async Task<ActionResult> GetBySlug(string slug)
    {
        var response = await cmsPageService.GetBySlug(slug);
        return Ok(response);
    }

    [HttpPost("Add")]
    [Authorize]
    public async Task<ActionResult> Add(CmsPage_AddEdit request)
    {
        var entity = new CmsPage
        {
            Id = request.Id ?? Guid.NewGuid(), Slug = request.Slug,
            Title = request.Title, Content = request.Content, IsActive = request.IsActive,
            CreatedOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
        var response = await cmsPageService.AddAsync(entity.ToSaveRequest());
        return Ok(response);
    }

    [HttpPost("Update")]
    [Authorize]
    public async Task<ActionResult> Update(CmsPage_AddEdit request)
    {
        var entity = new CmsPage
        {
            Id = request.Id ?? Guid.NewGuid(), Slug = request.Slug,
            Title = request.Title, Content = request.Content, IsActive = request.IsActive
        };
        var response = await cmsPageService.UpdateAsync(entity.ToUpdateRequest(request.Id));
        return Ok(response);
    }

    [HttpDelete("Delete/{id}")]
    [Authorize]
    public async Task<ActionResult> Delete(Guid id)
    {
        var response = await cmsPageService.DeleteAsync(id);
        return Ok(response);
    }
}
}
