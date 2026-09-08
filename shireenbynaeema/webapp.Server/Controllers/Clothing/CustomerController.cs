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
public class CustomerController : ControllerBase
{
    private readonly ICustomerService customerService;

    public CustomerController(ICustomerService customerService) { this.customerService = customerService; }

    [HttpGet("Profile")]
    public async Task<ActionResult> GetProfile()
    {
        var response = await customerService.GetProfile(CurrentUser.UserId!.Value);
        return Ok(response);
    }

    [HttpPut("Profile")]
    public async Task<ActionResult> UpdateProfile(CustomerDto request)
    {
        var response = await customerService.UpdateProfile(CurrentUser.UserId!.Value, request);
        return Ok(response);
    }

    [HttpPost("Address")]
    public async Task<ActionResult> AddAddress(AddressDto request)
    {
        var response = await customerService.AddAddress(CurrentUser.UserId!.Value, request);
        return Ok(response);
    }

    [HttpPut("Address/{id}")]
    public async Task<ActionResult> UpdateAddress(Guid id, AddressDto request)
    {
        var response = await customerService.UpdateAddress(CurrentUser.UserId!.Value, id, request);
        return Ok(response);
    }

    [HttpPost("Admin/List")]
    public async Task<ActionResult> List(ListRequest request)
    {
        var response = await customerService.ListCustomers(request);
        return Ok(response);
    }

    [HttpGet("Admin/{id}")]
    public async Task<ActionResult> GetDetail(Guid id)
    {
        var response = await customerService.GetCustomerDetail(id);
        return Ok(response);
    }

    [HttpDelete("Admin/Delete/{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var response = await customerService.Delete(id);
        return Ok(response);
    }

    [HttpDelete("Address/{id}")]
    public async Task<ActionResult> DeleteAddress(Guid id)
    {
        var response = await customerService.DeleteAddress(CurrentUser.UserId!.Value, id);
        return Ok(response);
    }
}
}