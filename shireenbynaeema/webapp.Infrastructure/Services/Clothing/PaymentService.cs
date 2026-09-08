namespace Infrastructure
{
    using Application;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using SharedServices;
    using Stripe;

    public class PaymentService : IPaymentService
    {
        private const string EventPaymentIntentSucceeded = "payment_intent.succeeded";
        private const string EventPaymentIntentFailed = "payment_intent.payment_failed";

        private readonly DatabaseContext db;
        private readonly IResponse resp;
        private readonly string webhookSecret;

        public PaymentService(DatabaseContext db, IResponse response, IConfiguration configuration)
        {
            this.db = db;
            this.resp = response;
            StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
            this.webhookSecret = configuration["Stripe:WebhookSecret"] ?? string.Empty;
        }

        public async Task<IResponse> CreatePaymentIntent(decimal amount, string currency, string orderId)
        {
            try
            {
                var order = await db.Orders.FindAsync(Guid.Parse(orderId));
                if (order == null)
                {
                    resp.IsSuccess = false;
                    resp.Message = "Order not found";
                    return resp;
                }

                var amountInCents = (long)(amount * 100);

                var options = new PaymentIntentCreateOptions
                {
                    Amount = amountInCents,
                    Currency = currency,
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true
                    },
                    Metadata = new Dictionary<string, string>
                    {
                        { "orderId", orderId },
                        { "orderNumber", order.OrderNumber }
                    }
                };

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);

                resp.IsSuccess = true;
                resp.Data = new
                {
                    paymentIntentId = paymentIntent.Id,
                    clientSecret = paymentIntent.ClientSecret,
                    amount = amount,
                    currency = currency
                };
                return resp;
            }
            catch (StripeException ex)
            {
                resp.IsSuccess = false;
                resp.Message = ex.Message;
                return resp;
            }
        }

        public async Task<IResponse> GetPaymentIntent(string paymentIntentId)
        {
            try
            {
                var service = new PaymentIntentService();
                var paymentIntent = await service.GetAsync(paymentIntentId);

                resp.IsSuccess = true;
                resp.Data = new
                {
                    id = paymentIntent.Id,
                    status = paymentIntent.Status,
                    amount = paymentIntent.Amount,
                    currency = paymentIntent.Currency
                };
                return resp;
            }
            catch (StripeException ex)
            {
                resp.IsSuccess = false;
                resp.Message = ex.Message;
                return resp;
            }
        }

        public async Task<IResponse> HandleWebhook(string json, string stripeSignature)
        {
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, webhookSecret);

                if (stripeEvent.Type == EventPaymentIntentSucceeded)
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    if (paymentIntent != null &&
                        paymentIntent.Metadata != null &&
                        paymentIntent.Metadata.TryGetValue("orderId", out var orderIdStr) &&
                        Guid.TryParse(orderIdStr, out var orderId))
                    {
                        var order = await db.Orders.FindAsync(orderId);
                        if (order != null)
                        {
                            order.Status = "Confirmed";
                            order.PaymentIntentId = paymentIntent.Id;
                            order.UpdatedOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                            var invoice = await db.Invoices.FirstOrDefaultAsync(i => i.OrderId == orderId);
                            if (invoice != null && invoice.Status != "Paid")
                            {
                                invoice.Status = "Paid";
                                invoice.PaidDate = DateTime.UtcNow;
                                invoice.PaymentIntentId = paymentIntent.Id;
                                invoice.UpdatedOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                            }

                            await db.SaveChangesAsync();
                        }
                    }
                }
                else if (stripeEvent.Type == EventPaymentIntentFailed)
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    if (paymentIntent != null &&
                        paymentIntent.Metadata != null &&
                        paymentIntent.Metadata.TryGetValue("orderId", out var orderIdStr) &&
                        Guid.TryParse(orderIdStr, out var orderId))
                    {
                        var order = await db.Orders.FindAsync(orderId);
                        if (order != null)
                        {
                            order.Status = "PaymentFailed";
                            order.UpdatedOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                            await db.SaveChangesAsync();
                        }
                    }
                }

                resp.IsSuccess = true;
                resp.Data = new { received = true };
                return resp;
            }
            catch (StripeException ex)
            {
                resp.IsSuccess = false;
                resp.Message = $"Webhook error: {ex.Message}";
                return resp;
            }
        }

        public async Task<IResponse> ProcessRefund(string paymentIntentId, long amountInCents)
        {
            try
            {
                var options = new RefundCreateOptions
                {
                    PaymentIntent = paymentIntentId,
                    Amount = amountInCents
                };

                var service = new RefundService();
                var refund = await service.CreateAsync(options);

                resp.IsSuccess = true;
                resp.Data = new
                {
                    refundId = refund.Id,
                    status = refund.Status,
                    amount = refund.Amount
                };
                return resp;
            }
            catch (StripeException ex)
            {
                resp.IsSuccess = false;
                resp.Message = ex.Message;
                return resp;
            }
        }
    }
}
