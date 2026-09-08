namespace Server
{
using Application;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedServices;

[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly IProductService productService;

    public ProductController(IProductService productService) { this.productService = productService; }

    [HttpPost("List")]
    public async Task<ActionResult> List(ProductListRequest request)
    {
        var response = await productService.ListProducts(request);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(Guid id)
    {
        var response = await productService.GetByIdAsync(id);
        return Ok(response);
    }

    [HttpGet("ByCategory/{slug}")]
    public async Task<ActionResult> GetByCategory(string slug)
    {
        var response = await productService.GetByCategory(slug);
        return Ok(response);
    }

    [HttpGet("Featured")]
    public async Task<ActionResult> GetFeatured()
    {
        var response = await productService.GetFeatured();
        return Ok(response);
    }

    [HttpGet("NewArrivals")]
    public async Task<ActionResult> GetNewArrivals()
    {
        var response = await productService.GetNewArrivals();
        return Ok(response);
    }

    [HttpGet("Search")]
    public async Task<ActionResult> Search([FromQuery] string q)
    {
        var response = await productService.Search(q);
        return Ok(response);
    }

    [HttpPost("Add")]
    [Authorize]
    public async Task<ActionResult> Add(Product_AddEdit request)
    {
        var response = await productService.AddProduct(request);
        return Ok(response);
    }

    [HttpPost("Update")]
    [Authorize]
    public async Task<ActionResult> Update(Product_AddEdit request)
    {
        var response = await productService.UpdateProduct(request);
        return Ok(response);
    }

    [HttpDelete("Delete/{id}")]
    [Authorize]
    public async Task<ActionResult> Delete(Guid id)
    {
        var response = await productService.DeleteAsync(id);
        return Ok(response);
    }
}
}