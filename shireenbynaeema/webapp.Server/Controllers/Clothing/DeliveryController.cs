namespace Server
{
using Application;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedServices;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DeliveryController : ControllerBase
{
    private readonly IDeliveryService deliveryService;

    public DeliveryController(IDeliveryService deliveryService) { this.deliveryService = deliveryService; }

    [HttpPost("Admin/List")]
    public async Task<ActionResult> List(ListRequest request)
    {
        var response = await deliveryService.ListDeliveries(request);
        return Ok(response);
    }

    [HttpGet("Admin/{id}")]
    public async Task<ActionResult> GetById(Guid id)
    {
        var response = await deliveryService.GetById(id);
        return Ok(response);
    }

    [HttpPost("Admin/Create")]
    public async Task<ActionResult> Create(DeliveryCreateRequest request)
    {
        var response = await deliveryService.Create(request);
        return Ok(response);
    }

    [HttpPut("Admin/UpdateTracking/{id}")]
    public async Task<ActionResult> UpdateTracking(Guid id, DeliveryUpdateRequest request)
    {
        var response = await deliveryService.UpdateTracking(id, request);
        return Ok(response);
    }

    [HttpDelete("Admin/Delete/{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var response = await deliveryService.Delete(id);
        return Ok(response);
    }
}
}