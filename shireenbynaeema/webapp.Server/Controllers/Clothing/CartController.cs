namespace Server
{
using Application;
using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService cartService;

    public CartController(ICartService cartService) { this.cartService = cartService; }

    [HttpGet]
    public async Task<ActionResult> GetCart()
    {
        var response = await cartService.GetCart(CurrentUser.UserId!.Value);
        return Ok(response);
    }

    [HttpPost("Add")]
    public async Task<ActionResult> AddItem(CartItem_AddEdit request)
    {
        var response = await cartService.AddItem(CurrentUser.UserId!.Value, request);
        return Ok(response);
    }

    [HttpPut("Update")]
    public async Task<ActionResult> UpdateQuantity([FromBody] CartItem_UpdateQuantity request, [FromQuery] Guid cartItemId)
    {
        var response = await cartService.UpdateQuantity(CurrentUser.UserId!.Value, cartItemId, request);
        return Ok(response);
    }

    [HttpDelete("Remove/{cartItemId}")]
    public async Task<ActionResult> RemoveItem(Guid cartItemId)
    {
        var response = await cartService.RemoveItem(CurrentUser.UserId!.Value, cartItemId);
        return Ok(response);
    }

    [HttpDelete("Clear")]
    public async Task<ActionResult> ClearCart()
    {
        var response = await cartService.ClearCart(CurrentUser.UserId!.Value);
        return Ok(response);
    }
}
}