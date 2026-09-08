namespace Infrastructure
{
using Application;
using Domain;
using Microsoft.EntityFrameworkCore;
using SharedServices;

public class InvoiceService : IInvoiceService
{
    private readonly DatabaseContext db;
    private readonly IResponse resp;

    public InvoiceService(DatabaseContext db, IResponse response) { this.db = db; this.resp = response; }

    public async Task<IResponse> ListInvoices(ListRequest request)
    {
        var query = db.Invoices.AsNoTracking().Include(i => i.Order).AsQueryable();
        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(i => i.CreatedOn).Skip(request.Skip).Take(request.Take)
            .Select(i => new InvoiceDto
            {
                Id = i.Id, InvoiceNumber = i.InvoiceNumber, OrderId = i.OrderId,
                OrderNumber = i.Order != null ? i.Order.OrderNumber : "",
                Amount = i.Amount, Tax = i.Tax, Status = i.Status,
                DueDate = i.DueDate, PaidDate = i.PaidDate, CreatedOn = i.CreatedOn
            }).ToListAsync();

        resp.IsSuccess = true;
        resp.Data = items.ToListResponse(request, totalCount);
        return resp;
    }

    public async Task<IResponse> GetById(Guid invoiceId)
    {
        var invoice = await db.Invoices.AsNoTracking().Include(i => i.Order).ThenInclude(o => o!.Customer).ThenInclude(c => c!.User)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);
        if (invoice == null) { resp.IsSuccess = false; resp.Message = "Invoice not found"; return resp; }

        resp.IsSuccess = true;
        resp.Data = new InvoiceDto
        {
            Id = invoice.Id, InvoiceNumber = invoice.InvoiceNumber, OrderId = invoice.OrderId,
            OrderNumber = invoice.Order?.OrderNumber ?? "",
            Amount = invoice.Amount, Tax = invoice.Tax, Status = invoice.Status,
            DueDate = invoice.DueDate, PaidDate = invoice.PaidDate, CreatedOn = invoice.CreatedOn
        };
        return resp;
    }

    public async Task<IResponse> GetByOrder(Guid orderId)
    {
        var invoice = await db.Invoices.AsNoTracking().FirstOrDefaultAsync(i => i.OrderId == orderId);
        resp.IsSuccess = invoice != null;
        resp.Data = invoice;
        return resp;
    }

    public async Task<IResponse> Generate(Guid orderId)
    {
        var existing = await db.Invoices.FirstOrDefaultAsync(i => i.OrderId == orderId);
        if (existing != null) { resp.IsSuccess = false; resp.Message = "Invoice already exists"; return resp; }

        var order = await db.Orders.FindAsync(orderId);
        if (order == null) { resp.IsSuccess = false; resp.Message = "Order not found"; return resp; }

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            OrderId = orderId, Amount = order.Total, Tax = order.Tax, Status = "Pending",
            DueDate = DateTime.UtcNow.AddDays(7), CreatedOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        db.Invoices.Add(invoice);
        await db.SaveChangesAsync();

        resp.IsSuccess = true;
        resp.Data = invoice.Id;
        return resp;
    }

    public async Task<IResponse> MarkPaid(Guid invoiceId, string paymentId)
    {
        var invoice = await db.Invoices.FindAsync(invoiceId);
        if (invoice == null) { resp.IsSuccess = false; resp.Message = "Invoice not found"; return resp; }

        invoice.Status = "Paid";
        invoice.PaidDate = DateTime.UtcNow;
        invoice.PaymentIntentId = paymentId;
        invoice.UpdatedOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        await db.SaveChangesAsync();

        resp.IsSuccess = true;
        return resp;
    }

    public async Task<IResponse> Delete(Guid invoiceId)
    {
        var invoice = await db.Invoices.FindAsync(invoiceId);
        if (invoice == null) { resp.IsSuccess = false; resp.Message = "Invoice not found"; return resp; }

        invoice.SoftDelete();
        await db.SaveChangesAsync();

        resp.IsSuccess = true;
        resp.Message = "Invoice deleted successfully";
        return resp;
    }
}
}