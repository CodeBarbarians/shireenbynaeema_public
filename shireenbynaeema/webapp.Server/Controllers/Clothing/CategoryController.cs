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
public class CategoryController : ControllerBase
{
    private readonly ICategoryService categoryService;

    public CategoryController(ICategoryService categoryService) { this.categoryService = categoryService; }

    [HttpPost("List")]
    public async Task<ActionResult> List(CategoryListRequest request)
    {
        var response = await categoryService.ListCategories(request);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(Guid id)
    {
        var response = await categoryService.GetByIdAsync(id);
        return Ok(response);
    }

    [HttpGet("BySlug/{slug}")]
    public async Task<ActionResult> GetBySlug(string slug)
    {
        var response = await categoryService.GetBySlug(slug);
        return Ok(response);
    }

    [HttpGet("Tree")]
    public async Task<ActionResult> GetTree()
    {
        var response = await categoryService.GetTree();
        return Ok(response);
    }

    [HttpPost("Add")]
    [Authorize]
    public async Task<ActionResult> Add(Category_AddEdit request)
    {
        var entity = new Category
        {
            Id = request.Id ?? Guid.NewGuid(), Name = request.Name, Slug = request.Slug,
            Description = request.Description, ParentId = request.ParentId,
            ImageUrl = request.ImageUrl, SortOrder = request.SortOrder, IsActive = request.IsActive,
            ShowInNav = request.ShowInNav
        };
        var response = await categoryService.AddAsync(entity.ToSaveRequest());
        return Ok(response);
    }

    [HttpPost("Update")]
    [Authorize]
    public async Task<ActionResult> Update(Category_AddEdit request)
    {
        var entity = new Category
        {
            Id = request.Id ?? Guid.NewGuid(), Name = request.Name, Slug = request.Slug,
            Description = request.Description, ParentId = request.ParentId,
            ImageUrl = request.ImageUrl, SortOrder = request.SortOrder, IsActive = request.IsActive,
            ShowInNav = request.ShowInNav
        };
        var response = await categoryService.UpdateAsync(entity.ToUpdateRequest(request.Id));
        return Ok(response);
    }

    [HttpDelete("Delete/{id}")]
    [Authorize]
    public async Task<ActionResult> Delete(Guid id)
    {
        var response = await categoryService.DeleteAsync(id);
        return Ok(response);
    }
}
}