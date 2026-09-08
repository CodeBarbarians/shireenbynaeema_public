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
public class PaymentController : ControllerBase
{
    private readonly IPaymentService paymentService;
    private readonly IOrderService orderService;
    private readonly IInvoiceService invoiceService;

    public PaymentController(IPaymentService paymentService, IOrderService orderService, IInvoiceService invoiceService)
    {
        this.paymentService = paymentService;
        this.orderService = orderService;
        this.invoiceService = invoiceService;
    }

    [HttpPost("CreateIntent")]
    public async Task<ActionResult> CreateIntent([FromBody] PaymentIntentRequest request)
    {
        var orderResp = await orderService.GetOrderDetail(CurrentUser.UserId, Guid.Parse(request.OrderId));
        if (orderResp is not Response { IsSuccess: true, Data: OrderDto order })
            return BadRequest(orderResp);

        var resp = await paymentService.CreatePaymentIntent(order.Total, "pkr", request.OrderId);
        return Ok(resp);
    }

    [HttpGet("Status/{id}")]
    public async Task<ActionResult> GetStatus(string id)
    {
        var resp = await paymentService.GetPaymentIntent(id);
        return Ok(resp);
    }

    [HttpPost("Webhook")]
    public async Task<ActionResult> Webhook()
    {
        using var reader = new StreamReader(Request.Body);
        var json = await reader.ReadToEndAsync();
        var stripeSignature = Request.Headers["Stripe-Signature"].ToString();
        var resp = await paymentService.HandleWebhook(json, stripeSignature);
        return Ok(resp);
    }

    [HttpPost("Refund")]
    public async Task<ActionResult> Refund([FromBody] RefundRequest request)
    {
        var resp = await paymentService.ProcessRefund(request.PaymentIntentId, request.AmountInCents);
        return Ok(resp);
    }
}

public class PaymentIntentRequest
{
    public string OrderId { get; set; } = string.Empty;
}

public class PaymentConfirmRequest
{
    public string PaymentIntentId { get; set; } = string.Empty;
}

public class RefundRequest
{
    public string PaymentIntentId { get; set; } = string.Empty;
    public long AmountInCents { get; set; }
}
}