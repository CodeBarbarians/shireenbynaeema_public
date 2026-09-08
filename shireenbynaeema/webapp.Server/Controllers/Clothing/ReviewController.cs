namespace Server
{
using Application;
using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class ReviewController : ControllerBase
{
    private readonly IRatingService ratingService;

    public ReviewController(IRatingService ratingService) { this.ratingService = ratingService; }

    [HttpGet("Product/{productId}")]
    public async Task<ActionResult> GetByProduct(Guid productId)
    {
        var response = await ratingService.GetByProduct(productId);
        return Ok(response);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult> Create(ReviewCreateRequest request)
    {
        var response = await ratingService.Create(CurrentUser.UserId!.Value, request);
        return Ok(response);
    }
}
}
