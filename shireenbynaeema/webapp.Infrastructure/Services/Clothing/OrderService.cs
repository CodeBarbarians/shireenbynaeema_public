namespace Infrastructure
{
    using Application;

    using Domain;

    using Microsoft.EntityFrameworkCore;

    using SharedServices;

    public class OrderService : IOrderService
    {
        private readonly DatabaseContext db;
        private readonly IResponse resp;
        private readonly IEmailService emailService;

        public OrderService(DatabaseContext db, IResponse response, IEmailService emailService)
        {
            this.db = db;
            this.resp = response;
            this.emailService = emailService;
        }

        private string GenerateOrderNumber()
        {
            return $"SBNE-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
        }

        private string GenerateInvoiceNumber()
        {
            return $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
        }

        public async Task<IResponse> PlaceOrder(Guid? userId, OrderPlaceRequest request)
        {
            Customer? customer = null;

            if (userId.HasValue)
                customer = await db.Customers.FirstOrDefaultAsync(c => c.UserId == userId.Value);

            if (customer == null && !string.IsNullOrWhiteSpace(request.Email))
                customer = await db.Customers.FirstOrDefaultAsync(c => c.Email == request.Email);

            if (customer == null)
            {
                customer = new Customer
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Phone = request.Phone,
                    CreatedOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };
                db.Customers.Add(customer);
                await db.SaveChangesAsync();
            }
            else
            {
                if (string.IsNullOrWhiteSpace(customer.Email)) customer.Email = request.Email;
                if (string.IsNullOrWhiteSpace(customer.FirstName)) customer.FirstName = request.FirstName;
                if (string.IsNullOrWhiteSpace(customer.LastName)) customer.LastName = request.LastName;
                if (string.IsNullOrWhiteSpace(customer.Phone)) customer.Phone = request.Phone;
                if (!userId.HasValue && customer.UserId == null) customer.UserId = userId;
            }

            var address = new Address
            {
                Id = Guid.NewGuid(),
                CustomerId = customer.Id,
                Label = "Shipping",
                Street = request.Street,
                City = request.City,
                State = request.State,
                ZipCode = request.ZipCode,
                Country = request.Country,
                IsDefault = true,
                CreatedOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            db.Addresses.Add(address);
            await db.SaveChangesAsync();

            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = GenerateOrderNumber(),
                CustomerId = customer.Id,
                Status = "Confirmed",
                ShippingAddressId = address.Id,
                Notes = request.Notes ?? "",
                CouponCode = request.CouponCode,
                PaymentIntentId = request.PaymentIntentId,
                CreatedOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            if (request.Items != null && request.Items.Any())
            {
                foreach (var item in request.Items)
                {
                    var variant = await db.ProductVariants.FindAsync(item.ProductVariantId);
                    if (variant == null || (variant.Stock - variant.BookedStock) < item.Quantity) continue;

                    var itemTotal = item.UnitPrice * item.Quantity;
                    order.Items.Add(new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        OrderId = order.Id,
                        ProductVariantId = item.ProductVariantId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        Total = itemTotal
                    });

                    variant.BookedStock += item.Quantity;
                    order.Subtotal += itemTotal;
                }
            }
            else if (userId.HasValue)
            {
                var cart = await db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId.Value);
                if (cart == null || !cart.Items.Any()) { resp.IsSuccess = false; resp.Message = "Cart is empty"; return resp; }

                foreach (var cartItem in cart.Items)
                {
                    var variant = await db.ProductVariants.FindAsync(cartItem.ProductVariantId);
                    if (variant == null || (variant.Stock - variant.BookedStock) < cartItem.Quantity) continue;

                    var itemTotal = cartItem.UnitPrice * cartItem.Quantity;
                    order.Items.Add(new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        OrderId = order.Id,
                        ProductVariantId = cartItem.ProductVariantId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.UnitPrice,
                        Total = itemTotal
                    });

                    variant.BookedStock += cartItem.Quantity;
                    order.Subtotal += itemTotal;
                }

                db.CartItems.RemoveRange(cart.Items);
                cart.Items.Clear();
            }
            else
            {
                resp.IsSuccess = false;
                resp.Message = "No items provided";
                return resp;
            }

            if (!order.Items.Any())
            {
                resp.IsSuccess = false;
                resp.Message = "No items available in stock";
                return resp;
            }

            order.Tax = Math.Round(order.Subtotal * 0.0m, 2);
            order.Shipping = 0;
            order.Discount = 0;
            order.Total = order.Subtotal + order.Tax + order.Shipping - order.Discount;

            db.Orders.Add(order);

            var invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                InvoiceNumber = GenerateInvoiceNumber(),
                OrderId = order.Id,
                Amount = order.Total,
                Tax = order.Tax,
                Status = "Pending",
                DueDate = DateTime.UtcNow.AddDays(7),
                CreatedOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            db.Invoices.Add(invoice);

            customer.OrderCount++;
            customer.TotalSpent += order.Total;

            await db.SaveChangesAsync();

            _ = SendOrderConfirmationEmail(order, customer, address);

            resp.IsSuccess = true;
            resp.Data = order.Id;
            return resp;
        }

        public async Task<IResponse> GetOrderHistory(Guid userId)
        {
            var customer = await db.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
            if (customer == null) { resp.IsSuccess = true; resp.Data = new List<OrderDto>(); return resp; }

            var orders = await db.Orders.AsNoTracking().Where(o => o.CustomerId == customer.Id)
                .Include(o => o.Items).ThenInclude(i => i.Variant).ThenInclude(v => v!.Product).ThenInclude(p => p!.Images)
                .OrderByDescending(o => o.CreatedOn)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    Status = o.Status,
                    Subtotal = o.Subtotal,
                    Tax = o.Tax,
                    Shipping = o.Shipping,
                    Discount = o.Discount,
                    CouponCode = o.CouponCode ?? "",
                    Total = o.Total,
                    CreatedOn = o.CreatedOn,
                    Items = o.Items.Select(i => new OrderItemDto
                    {
                        Id = i.Id,
                        ProductName = i.Variant != null && i.Variant.Product != null ? i.Variant.Product.Name : "",
                        Size = i.Variant != null ? i.Variant.Size : "",
                        Color = i.Variant != null ? i.Variant.Color : "",
                        ImageUrl = i.Variant != null && i.Variant.Product != null
                            ? i.Variant.Product.Images.OrderBy(img => img.SortOrder).Select(img => img.ImageUrl).FirstOrDefault() ?? "" : "",
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        Total = i.Total
                    }).ToList()
                }).ToListAsync();

            resp.IsSuccess = true;
            resp.Data = new { entities = orders, totalCount = orders.Count };
            return resp;
        }

        public async Task<IResponse> GetOrderDetail(Guid? userId, Guid orderId)
        {
            var order = await db.Orders.AsNoTracking()
                .Include(o => o.Items).ThenInclude(i => i.Variant).ThenInclude(v => v!.Product).ThenInclude(p => p!.Images)
                .Include(o => o.ShippingAddress)
                .Include(o => o.Customer).ThenInclude(c => c!.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) { resp.IsSuccess = false; resp.Message = "Order not found"; return resp; }

            var orderItemIds = order.Items.Select(i => i.Id).ToList();

            var deliveredQtys = await (from di in db.Set<DeliveryItem>().AsNoTracking()
                                       join d in db.Set<Delivery>().AsNoTracking() on di.DeliveryId equals d.Id
                                       where orderItemIds.Contains(di.OrderItemId) && d.Status != "Cancelled"
                                       group di by di.OrderItemId into g
                                       select new { OrderItemId = g.Key, Qty = g.Sum(di => di.Quantity) }).ToListAsync();

            var returnedQtys = await (from ri in db.Set<ReturnItem>().AsNoTracking()
                                      join r in db.Set<Return>().AsNoTracking() on ri.ReturnId equals r.Id
                                      where orderItemIds.Contains(ri.OrderItemId) && (r.Status == "Approved" || r.Status == "Refunded")
                                      group ri by ri.OrderItemId into g
                                      select new { OrderItemId = g.Key, Qty = g.Sum(ri => ri.Quantity) }).ToListAsync();

            var deliveredMap = deliveredQtys.ToDictionary(x => x.OrderItemId, x => x.Qty);
            var returnedMap = returnedQtys.ToDictionary(x => x.OrderItemId, x => x.Qty);

            var dto = new OrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer?.User != null ? order.Customer.User.DisplayName : $"{order.Customer?.FirstName} {order.Customer?.LastName}",
                CustomerEmail = order.Customer?.User != null ? order.Customer.User.Email : order.Customer?.Email ?? "",
                Status = order.Status,
                Subtotal = order.Subtotal,
                Tax = order.Tax,
                Shipping = order.Shipping,
                Discount = order.Discount,
                CouponCode = order.CouponCode ?? "",
                Total = order.Total,
                Notes = order.Notes,
                CreatedOn = order.CreatedOn,
                ShippingAddress = order.ShippingAddress != null ? new AddressDto
                {
                    Id = order.ShippingAddress.Id,
                    Label = order.ShippingAddress.Label,
                    Street = order.ShippingAddress.Street,
                    City = order.ShippingAddress.City,
                    State = order.ShippingAddress.State,
                    ZipCode = order.ShippingAddress.ZipCode,
                    Country = order.ShippingAddress.Country
                }
                : null,
                Items = order.Items.Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    ProductName = i.Variant != null && i.Variant.Product != null ? i.Variant.Product.Name : "",
                    Size = i.Variant != null ? i.Variant.Size : "",
                    Color = i.Variant != null ? i.Variant.Color : "",
                    ImageUrl = i.Variant != null && i.Variant.Product != null
                        ? i.Variant.Product.Images.OrderBy(img => img.SortOrder).Select(img => img.ImageUrl).FirstOrDefault() ?? "" : "",
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Total = i.Total,
                    DeliveredQuantity = deliveredMap.TryGetValue(i.Id, out var dq) ? dq : 0,
                    ReturnedQuantity = returnedMap.TryGetValue(i.Id, out var rq) ? rq : 0
                }).ToList()
            };

            resp.IsSuccess = true;
            resp.Data = dto;
            return resp;
        }

        public async Task<IResponse> ListOrders(OrderListRequest request)
        {
            var query = db.Orders.AsNoTracking()
                .Include(o => o.Customer).ThenInclude(c => c!.User)
                .Include(o => o.Customer)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
                query = query.Where(o => o.OrderNumber.Contains(request.Search) || (o.Customer != null && o.Customer.Email.Contains(request.Search)));
            if (!string.IsNullOrWhiteSpace(request.Status))
                query = query.Where(o => o.Status == request.Status);

            var totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(o => o.CreatedOn).Skip(request.Skip).Take(request.Take)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    CustomerId = o.CustomerId,
                    CustomerName = o.Customer != null && o.Customer.User != null ? o.Customer.User.DisplayName : $"{o.Customer!.FirstName} {o.Customer.LastName}",
                    CustomerEmail = o.Customer != null && o.Customer.User != null ? o.Customer.User.Email : o.Customer!.Email,
                    Status = o.Status,
                    Subtotal = o.Subtotal,
                    Tax = o.Tax,
                    Shipping = o.Shipping,
                    Discount = o.Discount,
                    Total = o.Total,
                    Notes = o.Notes,
                    CreatedOn = o.CreatedOn
                }).ToListAsync();

            resp.IsSuccess = true;
            resp.Data = items.ToListResponse(request, totalCount);
            return resp;
        }

        public async Task<IResponse> UpdateStatus(Guid orderId, OrderStatusUpdateRequest request)
        {
            var order = await db.Orders.Include(o => o.Items).ThenInclude(i => i.Variant)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) { resp.IsSuccess = false; resp.Message = "Order not found"; return resp; }

            var previousStatus = order.Status;
            order.Status = request.Status;
            order.UpdatedOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (request.Status == "Cancelled" && previousStatus != "Cancelled")
            {
                foreach (var item in order.Items)
                {
                    if (item.Variant != null)
                    {
                        item.Variant.BookedStock = Math.Max(0, item.Variant.BookedStock - item.Quantity);
                    }
                }
            }

            if (request.Status == "Delivered" && previousStatus != "Delivered")
            {
                foreach (var item in order.Items)
                {
                    if (item.Variant != null)
                    {
                        item.Variant.Stock = Math.Max(0, item.Variant.Stock - item.Quantity);
                        item.Variant.BookedStock = Math.Max(0, item.Variant.BookedStock - item.Quantity);
                    }
                }
            }

            await db.SaveChangesAsync();

            resp.IsSuccess = true;
            return resp;
        }

        public async Task<IResponse> Delete(Guid orderId)
        {
            var order = await db.Orders.FindAsync(orderId);
            if (order == null)
            {
                resp.IsSuccess = false;
                resp.Message = "Order not found";
                return resp;
            }

            order.SoftDelete();
            await db.SaveChangesAsync();

            resp.IsSuccess = true;
            resp.Message = "Order deleted successfully";
            return resp;
        }

        private async Task SendOrderConfirmationEmail(Order order, Customer customer, Address address)
        {
            try
            {
                var templatePath = Path.Combine(AppContext.BaseDirectory, "Content", "OrderConfirmationEmailTemplate.html");
                if (!File.Exists(templatePath)) return;
                var template = await File.ReadAllTextAsync(templatePath);

                var customerName = $"{customer.FirstName} {customer.LastName}".Trim();
                var itemCount = order.Items.Count;
                var shippingAddress = $"{address.Street}, {address.City}, {address.State} {address.ZipCode}";

                template = template.Replace("{1}", customerName);
                template = template.Replace("{2}", $"https://shireenbynaeema.com/account/orders");
                template = template.Replace("{3}", $"Rs. {order.Total:N0} PKR");
                template = template.Replace("{5}", order.OrderNumber);
                template = template.Replace("{6}", DateTime.UtcNow.ToString("dd MMM yyyy"));
                template = template.Replace("{7}", $"{itemCount} item(s)");
                template = template.Replace("{8}", shippingAddress);

                await emailService.SendEmail(
                    customer.Email,
                    $"Order Confirmed - {order.OrderNumber}",
                    $"Your order {order.OrderNumber} has been confirmed. Total: Rs. {order.Total:N0} PKR",
                    template,
                    "Notifications");
            }
            catch { }
        }
    }
}
