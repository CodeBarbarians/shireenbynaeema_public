namespace Infrastructure
{
using Application;
using Domain;
using Microsoft.EntityFrameworkCore;
using SharedServices;

public class DeliveryService : IDeliveryService
{
    private readonly DatabaseContext db;
    private readonly IResponse resp;

    public DeliveryService(DatabaseContext db, IResponse response) { this.db = db; this.resp = response; }

    public async Task<IResponse> ListDeliveries(ListRequest request)
    {
        var query = db.Deliveries.AsNoTracking().Include(d => d.Order).Include(d => d.Items).AsQueryable();
        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(d => d.CreatedOn).Skip(request.Skip).Take(request.Take)
            .Select(d => new DeliveryDto
            {
                Id = d.Id, OrderId = d.OrderId, OrderNumber = d.Order != null ? d.Order.OrderNumber : "",
                Carrier = d.Carrier, TrackingNumber = d.TrackingNumber,
                Status = d.Status, ShippedOn = d.ShippedOn.HasValue ? d.ShippedOn.Value.ToString("yyyy-MM-dd") : null,
                DeliveredOn = d.DeliveredOn.HasValue ? d.DeliveredOn.Value.ToString("yyyy-MM-dd") : null,
                EstimatedDelivery = d.EstimatedDelivery.HasValue ? d.EstimatedDelivery.Value.ToString("yyyy-MM-dd") : null,
                Items = d.Items.Select(i => new DeliveryItemDto
                {
                    Id = i.Id, OrderItemId = i.OrderItemId,
                    ProductName = i.OrderItem != null && i.OrderItem.Variant != null && i.OrderItem.Variant.Product != null ? i.OrderItem.Variant.Product.Name : "",
                    Size = i.OrderItem != null && i.OrderItem.Variant != null ? i.OrderItem.Variant.Size : "",
                    Color = i.OrderItem != null && i.OrderItem.Variant != null ? i.OrderItem.Variant.Color : "",
                    Quantity = i.Quantity,
                    OrderedQuantity = i.OrderItem != null ? i.OrderItem.Quantity : 0
                }).ToList()
            }).ToListAsync();

        resp.IsSuccess = true;
        resp.Data = items.ToListResponse(request, totalCount);
        return resp;
    }

    public async Task<IResponse> GetById(Guid deliveryId)
    {
        var delivery = await db.Set<Delivery>().AsNoTracking()
            .Include(d => d.Order).Include(d => d.Items).ThenInclude(i => i.OrderItem).ThenInclude(oi => oi!.Variant).ThenInclude(v => v!.Product)
            .FirstOrDefaultAsync(d => d.Id == deliveryId);
        if (delivery == null) { resp.IsSuccess = false; resp.Message = "Delivery not found"; return resp; }

        resp.IsSuccess = true;
        resp.Data = new DeliveryDto
        {
            Id = delivery.Id, OrderId = delivery.OrderId, OrderNumber = delivery.Order?.OrderNumber ?? "",
            Carrier = delivery.Carrier, TrackingNumber = delivery.TrackingNumber,
            Status = delivery.Status,
            ShippedOn = delivery.ShippedOn.HasValue ? delivery.ShippedOn.Value.ToString("yyyy-MM-dd") : null,
            DeliveredOn = delivery.DeliveredOn.HasValue ? delivery.DeliveredOn.Value.ToString("yyyy-MM-dd") : null,
            EstimatedDelivery = delivery.EstimatedDelivery.HasValue ? delivery.EstimatedDelivery.Value.ToString("yyyy-MM-dd") : null,
            Items = delivery.Items.Select(i => new DeliveryItemDto
            {
                Id = i.Id, OrderItemId = i.OrderItemId,
                ProductName = i.OrderItem?.Variant?.Product?.Name ?? "",
                Size = i.OrderItem?.Variant?.Size ?? "",
                Color = i.OrderItem?.Variant?.Color ?? "",
                Quantity = i.Quantity,
                OrderedQuantity = i.OrderItem?.Quantity ?? 0
            }).ToList()
        };
        return resp;
    }

    public async Task<IResponse> Create(DeliveryCreateRequest request)
    {
        var order = await db.Orders.Include(o => o.Items).ThenInclude(i => i.Variant).FirstOrDefaultAsync(o => o.Id == request.OrderId);
        if (order == null) { resp.IsSuccess = false; resp.Message = "Order not found"; return resp; }

        var delivery = new Delivery
        {
            Id = Guid.NewGuid(), OrderId = request.OrderId, Carrier = request.Carrier,
            TrackingNumber = request.TrackingNumber, Status = "Pending",
            ShippedOn = DateTime.UtcNow,
            EstimatedDelivery = DateTime.TryParse(request.EstimatedDelivery, out var ed) ? ed : null,
            CreatedOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        if (request.Items != null && request.Items.Any())
        {
            foreach (var item in request.Items)
            {
                var orderItem = order.Items.FirstOrDefault(i => i.Id == item.OrderItemId);
                if (orderItem == null || item.Quantity <= 0) continue;

                delivery.Items.Add(new DeliveryItem
                {
                    Id = Guid.NewGuid(),
                    DeliveryId = delivery.Id,
                    OrderItemId = item.OrderItemId,
                    Quantity = item.Quantity
                });

                if (orderItem.Variant != null)
                {
                    orderItem.Variant.Stock = Math.Max(0, orderItem.Variant.Stock - item.Quantity);
                    orderItem.Variant.BookedStock = Math.Max(0, orderItem.Variant.BookedStock - item.Quantity);
                }
            }
        }
        else
        {
            foreach (var orderItem in order.Items)
            {
                delivery.Items.Add(new DeliveryItem
                {
                    Id = Guid.NewGuid(),
                    DeliveryId = delivery.Id,
                    OrderItemId = orderItem.Id,
                    Quantity = orderItem.Quantity
                });

                if (orderItem.Variant != null)
                {
                    orderItem.Variant.Stock = Math.Max(0, orderItem.Variant.Stock - orderItem.Quantity);
                    orderItem.Variant.BookedStock = Math.Max(0, orderItem.Variant.BookedStock - orderItem.Quantity);
                }
            }
        }

        var totalDelivered = await (from di in db.Set<DeliveryItem>().AsNoTracking()
                                    join d in db.Set<Delivery>().AsNoTracking() on di.DeliveryId equals d.Id
                                    where d.OrderId == request.OrderId && d.Status != "Cancelled"
                                    select di.Quantity).SumAsync();
        var newDeliveryQty = delivery.Items.Sum(i => i.Quantity);
        var totalOrderQty = order.Items.Sum(i => i.Quantity);

        if (totalDelivered + newDeliveryQty >= totalOrderQty)
            order.Status = "Shipped";

        db.Deliveries.Add(delivery);
        await db.SaveChangesAsync();

        resp.IsSuccess = true;
        resp.Data = delivery.Id;
        return resp;
    }

    public async Task<IResponse> UpdateTracking(Guid deliveryId, DeliveryUpdateRequest request)
    {
        var delivery = await db.Deliveries.FindAsync(deliveryId);
        if (delivery == null) { resp.IsSuccess = false; resp.Message = "Delivery not found"; return resp; }

        delivery.Status = request.Status;
        if (request.Status == "Delivered") delivery.DeliveredOn = DateTime.UtcNow;
        delivery.UpdatedOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var order = await db.Orders.FindAsync(delivery.OrderId);
        if (order != null && request.Status == "Delivered")
        {
            var allDeliveries = await db.Deliveries.Where(d => d.OrderId == delivery.OrderId).ToListAsync();
            var allDelivered = allDeliveries.All(d => d.Status == "Delivered");
            order.Status = allDelivered ? "Delivered" : "Partially Delivered";
        }

        await db.SaveChangesAsync();
        resp.IsSuccess = true;
        return resp;
    }

    public async Task<IResponse> Delete(Guid deliveryId)
    {
        var delivery = await db.Deliveries.FindAsync(deliveryId);
        if (delivery == null) { resp.IsSuccess = false; resp.Message = "Delivery not found"; return resp; }

        delivery.SoftDelete();
        await db.SaveChangesAsync();

        resp.IsSuccess = true;
        resp.Message = "Delivery deleted successfully";
        return resp;
    }
}
}
