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
public class InvoiceController : ControllerBase
{
    private readonly IInvoiceService invoiceService;

    public InvoiceController(IInvoiceService invoiceService) { this.invoiceService = invoiceService; }

    [HttpPost("Admin/List")]
    public async Task<ActionResult> List(ListRequest request)
    {
        var response = await invoiceService.ListInvoices(request);
        return Ok(response);
    }

    [HttpGet("Admin/{id}")]
    public async Task<ActionResult> GetById(Guid id)
    {
        var response = await invoiceService.GetById(id);
        return Ok(response);
    }

    [HttpPost("Admin/Generate")]
    public async Task<ActionResult> Generate([FromBody] InvoiceGenerateRequest request)
    {
        var response = await invoiceService.Generate(request.OrderId);
        return Ok(response);
    }

    [HttpPut("Admin/MarkPaid/{id}")]
    public async Task<ActionResult> MarkPaid(Guid id, [FromBody] InvoiceMarkPaidRequest request)
    {
        var response = await invoiceService.MarkPaid(id, request.PaymentId);
        return Ok(response);
    }

    [HttpDelete("Admin/Delete/{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var response = await invoiceService.Delete(id);
        return Ok(response);
    }
}

public class InvoiceGenerateRequest
{
    public Guid OrderId { get; set; }
}

public class InvoiceMarkPaidRequest
{
    public string PaymentId { get; set; } = string.Empty;
}
}