namespace Infrastructure
{
    using Application;
    using Domain;
    using Microsoft.EntityFrameworkCore;
    using SharedServices;

    public class ReturnService : IReturnService
    {
        private readonly DatabaseContext db;
        private readonly IResponse resp;

        public ReturnService(DatabaseContext db, IResponse response) { this.db = db; this.resp = response; }

        public async Task<IResponse> ListReturns(ListRequest request)
        {
            var query = db.Returns.AsNoTracking().Include(r => r.Order).Include(r => r.Items).AsQueryable();
            var totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(r => r.CreatedOn).Skip(request.Skip).Take(request.Take)
                .Select(r => new ReturnDto
                {
                    Id = r.Id, OrderId = r.OrderId, OrderNumber = r.Order != null ? r.Order.OrderNumber : "",
                    Reason = r.Reason, Status = r.Status,
                    RefundAmount = r.RefundAmount, RefundPaymentId = r.RefundPaymentId,
                    BankAccount = r.BankAccount, BankName = r.BankName, AccountHolderName = r.AccountHolderName,
                    Attachments = r.Attachments,
                    Notes = r.Notes,
                    RequestedOn = r.RequestedOn.HasValue ? r.RequestedOn.Value.ToString("yyyy-MM-dd") : null,
                    ProcessedOn = r.ProcessedOn.HasValue ? r.ProcessedOn.Value.ToString("yyyy-MM-dd") : null,
                    Items = r.Items.Select(i => new ReturnItemDto
                    {
                        Id = i.Id, OrderItemId = i.OrderItemId,
                        ProductName = i.OrderItem != null && i.OrderItem.Variant != null && i.OrderItem.Variant.Product != null ? i.OrderItem.Variant.Product.Name : "",
                        Size = i.OrderItem != null && i.OrderItem.Variant != null ? i.OrderItem.Variant.Size : "",
                        Color = i.OrderItem != null && i.OrderItem.Variant != null ? i.OrderItem.Variant.Color : "",
                        Quantity = i.Quantity,
                        OrderedQuantity = i.OrderItem != null ? i.OrderItem.Quantity : 0,
                        Reason = i.Reason
                    }).ToList()
                }).ToListAsync();

            resp.IsSuccess = true;
            resp.Data = items.ToListResponse(request, totalCount);
            return resp;
        }

        public async Task<IResponse> GetById(Guid returnId)
        {
            var ret = await db.Set<Return>().AsNoTracking()
                .Include(r => r.Order).Include(r => r.Items).ThenInclude(i => i.OrderItem).ThenInclude(oi => oi!.Variant).ThenInclude(v => v!.Product)
                .FirstOrDefaultAsync(r => r.Id == returnId);
            if (ret == null) { resp.IsSuccess = false; resp.Message = "Return not found"; return resp; }

            resp.IsSuccess = true;
            resp.Data = new ReturnDto
            {
                Id = ret.Id, OrderId = ret.OrderId, OrderNumber = ret.Order?.OrderNumber ?? "",
                Reason = ret.Reason, Status = ret.Status, RefundAmount = ret.RefundAmount,
                RefundPaymentId = ret.RefundPaymentId,
                BankAccount = ret.BankAccount, BankName = ret.BankName, AccountHolderName = ret.AccountHolderName,
                Attachments = ret.Attachments,
                Notes = ret.Notes,
                RequestedOn = ret.RequestedOn.HasValue ? ret.RequestedOn.Value.ToString("yyyy-MM-dd") : null,
                ProcessedOn = ret.ProcessedOn.HasValue ? ret.ProcessedOn.Value.ToString("yyyy-MM-dd") : null,
                Items = ret.Items.Select(i => new ReturnItemDto
                {
                    Id = i.Id, OrderItemId = i.OrderItemId,
                    ProductName = i.OrderItem?.Variant?.Product?.Name ?? "",
                    Size = i.OrderItem?.Variant?.Size ?? "",
                    Color = i.OrderItem?.Variant?.Color ?? "",
                    Quantity = i.Quantity,
                    OrderedQuantity = i.OrderItem?.Quantity ?? 0,
                    Reason = i.Reason
                }).ToList()
            };
            return resp;
        }

        public async Task<IResponse> GetByUser(Guid userId)
        {
            var customer = await db.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
            if (customer == null) { resp.IsSuccess = true; resp.Data = new List<ReturnDto>(); return resp; }

            var returns = await db.Returns.AsNoTracking()
                .Where(r => r.Order != null && r.Order.CustomerId == customer.Id)
                .Include(r => r.Order).Include(r => r.Items).ThenInclude(i => i.OrderItem).ThenInclude(oi => oi!.Variant).ThenInclude(v => v!.Product)
                .OrderByDescending(r => r.CreatedOn)
                .Select(r => new ReturnDto
                {
                    Id = r.Id, OrderId = r.OrderId, OrderNumber = r.Order != null ? r.Order.OrderNumber : "",
                    Reason = r.Reason, Status = r.Status, RefundAmount = r.RefundAmount,
                    BankAccount = r.BankAccount, BankName = r.BankName, AccountHolderName = r.AccountHolderName,
                    Notes = r.Notes,
                    RequestedOn = r.RequestedOn.HasValue ? r.RequestedOn.Value.ToString("yyyy-MM-dd") : null,
                    ProcessedOn = r.ProcessedOn.HasValue ? r.ProcessedOn.Value.ToString("yyyy-MM-dd") : null,
                    Items = r.Items.Select(i => new ReturnItemDto
                    {
                        Id = i.Id, OrderItemId = i.OrderItemId,
                        ProductName = i.OrderItem != null && i.OrderItem.Variant != null && i.OrderItem.Variant.Product != null ? i.OrderItem.Variant.Product.Name : "",
                        Size = i.OrderItem != null && i.OrderItem.Variant != null ? i.OrderItem.Variant.Size : "",
                        Color = i.OrderItem != null && i.OrderItem.Variant != null ? i.OrderItem.Variant.Color : "",
                        Quantity = i.Quantity,
                        OrderedQuantity = i.OrderItem != null ? i.OrderItem.Quantity : 0,
                        Reason = i.Reason
                    }).ToList()
                }).ToListAsync();

            resp.IsSuccess = true;
            resp.Data = returns;
            return resp;
        }

        public async Task<IResponse> RequestReturn(Guid userId, ReturnRequestDto request)
        {
            var customer = await db.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
            if (customer == null) { resp.IsSuccess = false; resp.Message = "Customer not found"; return resp; }

            var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == request.OrderId && o.CustomerId == customer.Id);
            if (order == null) { resp.IsSuccess = false; resp.Message = "Order not found"; return resp; }

            var ret = new Return
            {
                Id = Guid.NewGuid(), OrderId = request.OrderId, Reason = request.Reason,
                Status = "Requested", RequestedOn = DateTime.UtcNow,
                BankAccount = request.BankAccount, BankName = request.BankName,
                AccountHolderName = request.AccountHolderName,
                Attachments = request.Attachments ?? string.Empty,
                CreatedOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            if (request.Items != null && request.Items.Any())
            {
                foreach (var item in request.Items)
                {
                    var orderItem = order.Items.FirstOrDefault(i => i.Id == item.OrderItemId);
                    if (orderItem == null || item.Quantity <= 0) continue;

                    ret.Items.Add(new ReturnItem
                    {
                        Id = Guid.NewGuid(),
                        ReturnId = ret.Id,
                        OrderItemId = item.OrderItemId,
                        Quantity = item.Quantity,
                        Reason = item.Reason
                    });
                }
            }
            else
            {
                foreach (var orderItem in order.Items)
                {
                    ret.Items.Add(new ReturnItem
                    {
                        Id = Guid.NewGuid(),
                        ReturnId = ret.Id,
                        OrderItemId = orderItem.Id,
                        Quantity = orderItem.Quantity,
                        Reason = request.Reason
                    });
                }
            }

            db.Returns.Add(ret);
            await db.SaveChangesAsync();

            resp.IsSuccess = true;
            resp.Data = ret.Id;
            return resp;
        }

        public async Task<IResponse> Process(Guid returnId, ReturnProcessDto request)
        {
            var ret = await db.Returns.Include(r => r.Items).ThenInclude(i => i.OrderItem).ThenInclude(oi => oi!.Variant)
                .FirstOrDefaultAsync(r => r.Id == returnId);
            if (ret == null) { resp.IsSuccess = false; resp.Message = "Return not found"; return resp; }

            ret.Status = request.Status;
            ret.ProcessedOn = DateTime.UtcNow;
            ret.UpdatedOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (request.Notes != null) ret.Notes = request.Notes;

            if (request.Status == "Approved")
            {
                if (request.RefundAmount.HasValue)
                    ret.RefundAmount = request.RefundAmount;
            }
            else if (request.Status == "InCheckup")
            {
                foreach (var item in ret.Items)
                {
                    if (item.OrderItem?.Variant != null)
                    {
                        item.OrderItem.Variant.Stock += item.Quantity;
                    }
                }
            }
            else if (request.Status == "Refunded")
            {
                if (request.RefundAmount.HasValue)
                    ret.RefundAmount = request.RefundAmount;

                foreach (var item in ret.Items)
                {
                    if (item.OrderItem?.Variant != null)
                    {
                        item.OrderItem.Variant.BookedStock = Math.Max(0, item.OrderItem.Variant.BookedStock - item.Quantity);
                    }
                }

                var order = await db.Orders.FindAsync(ret.OrderId);
                if (order != null)
                {
                    var totalReturned = await (from ri in db.Set<ReturnItem>().AsNoTracking()
                                               join r in db.Set<Return>().AsNoTracking() on ri.ReturnId equals r.Id
                                               where r.OrderId == ret.OrderId && (r.Status == "Refunded" || r.Status == "Completed")
                                               select ri.Quantity).SumAsync();
                    var newReturnQty = ret.Items.Sum(i => i.Quantity);
                    var totalOrderQty = order.Items.Sum(i => i.Quantity);

                    if (totalReturned + newReturnQty >= totalOrderQty)
                        order.Status = "Returned";
                    else if (order.Status != "Returned")
                        order.Status = "Partially Returned";
                }
            }
            else if (request.Status == "Completed")
            {
                ret.RefundPaymentId = request.Notes;
            }

            await db.SaveChangesAsync();
            resp.IsSuccess = true;
            return resp;
        }

        public async Task<IResponse> Delete(Guid returnId)
        {
            var ret = await db.Returns.FindAsync(returnId);
            if (ret == null) { resp.IsSuccess = false; resp.Message = "Return not found"; return resp; }

            ret.SoftDelete();
            await db.SaveChangesAsync();

            resp.IsSuccess = true;
            resp.Message = "Return deleted successfully";
            return resp;
        }
    }
}
