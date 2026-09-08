namespace Server
{
using Application;
using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly IOrderService orderService;

    public OrderController(IOrderService orderService) { this.orderService = orderService; }

    [HttpPost("Place")]
    public async Task<ActionResult> PlaceOrder(OrderPlaceRequest request)
    {
        Guid? userId = CurrentUser.UserId;
        var response = await orderService.PlaceOrder(userId, request);
        return Ok(response);
    }

    [HttpGet("History")]
    [Authorize]
    public async Task<ActionResult> GetHistory()
    {
        var response = await orderService.GetOrderHistory(CurrentUser.UserId!.Value);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetDetail(Guid id)
    {
        Guid? userId = CurrentUser.UserId;
        var response = await orderService.GetOrderDetail(userId, id);
        return Ok(response);
    }

    [HttpPost("Admin/List")]
    [Authorize]
    public async Task<ActionResult> ListAdmin(OrderListRequest request)
    {
        var response = await orderService.ListOrders(request);
        return Ok(response);
    }

    [HttpPut("Admin/UpdateStatus/{id}")]
    [Authorize]
    public async Task<ActionResult> UpdateStatus(Guid id, OrderStatusUpdateRequest request)
    {
        var response = await orderService.UpdateStatus(id, request);
        return Ok(response);
    }

    [HttpDelete("Admin/Delete/{id}")]
    [Authorize]
    public async Task<ActionResult> Delete(Guid id)
    {
        var response = await orderService.Delete(id);
        return Ok(response);
    }
}
}
