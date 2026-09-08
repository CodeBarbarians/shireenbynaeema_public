namespace Infrastructure
{
    using Application;
    using Domain;
    using Microsoft.EntityFrameworkCore;
    using SharedServices;

    public class ReportService : IReportService
    {
        private readonly DatabaseContext db;
        private readonly IResponse resp;

        public ReportService(DatabaseContext db, IResponse response)
        {
            this.db = db;
            this.resp = response;
        }

        public async Task<IResponse> GetSalesReport(ReportFilterRequest filter)
        {
            var query = db.Orders
                .Where(o => !o.IsDeleted)
                .Include(o => o.Items).ThenInclude(i => i.Variant).ThenInclude(v => v!.Product).ThenInclude(p => p!.Category)
                .AsQueryable();

            if (filter.FromDate.HasValue)
                query = query.Where(o => o.CreatedOn >= filter.FromDate.Value);
            if (filter.ToDate.HasValue)
                query = query.Where(o => o.CreatedOn <= filter.ToDate.Value);
            if (filter.ProductId.HasValue)
                query = query.Where(o => o.Items.Any(i => i.Variant!.ProductId == filter.ProductId.Value));
            if (filter.CategoryId.HasValue)
                query = query.Where(o => o.Items.Any(i => i.Variant!.Product!.CategoryId == filter.CategoryId.Value));

            var orders = await query.OrderByDescending(o => o.CreatedOn).ToListAsync();

            var items = orders.SelectMany(o => o.Items.Select(i => new ReportSalesRow
            {
                OrderNumber = o.OrderNumber,
                OrderDate = o.CreatedOn.HasValue ? DateTimeOffset.FromUnixTimeSeconds(o.CreatedOn.Value).ToString("dd MMM yyyy") : "",
                ProductName = i.Variant?.Product?.Name ?? "",
                Category = i.Variant?.Product?.Category?.Name ?? "",
                Size = i.Variant?.Size ?? "",
                Color = i.Variant?.Color ?? "",
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Total = i.Total,
                Status = o.Status
            })).ToList();

            var summary = new ReportSummary
            {
                TotalOrders = orders.Count,
                TotalRevenue = orders.Sum(o => o.Total),
                TotalItems = items.Sum(i => i.Quantity),
                AverageOrderValue = orders.Count > 0 ? orders.Average(o => o.Total) : 0,
                CancelledOrders = orders.Count(o => o.Status == "Cancelled"),
                DeliveredOrders = orders.Count(o => o.Status == "Delivered"),
                PendingOrders = orders.Count(o => o.Status == "Pending" || o.Status == "Confirmed")
            };

            resp.IsSuccess = true;
            resp.Data = new SalesReportResult { Items = items, Summary = summary };
            return resp;
        }

        public async Task<IResponse> GetCategoryReport(ReportFilterRequest filter)
        {
            var query = db.Orders
                .Where(o => !o.IsDeleted)
                .Include(o => o.Items).ThenInclude(i => i.Variant).ThenInclude(v => v!.Product).ThenInclude(p => p!.Category)
                .AsQueryable();

            if (filter.FromDate.HasValue)
                query = query.Where(o => o.CreatedOn >= filter.FromDate.Value);
            if (filter.ToDate.HasValue)
                query = query.Where(o => o.CreatedOn <= filter.ToDate.Value);
            if (filter.CategoryId.HasValue)
                query = query.Where(o => o.Items.Any(i => i.Variant!.Product!.CategoryId == filter.CategoryId.Value));
            if (filter.ProductId.HasValue)
                query = query.Where(o => o.Items.Any(i => i.Variant!.ProductId == filter.ProductId.Value));

            var orders = await query.ToListAsync();

            var categoryData = orders
                .SelectMany(o => o.Items)
                .GroupBy(i => i.Variant?.Product?.Category?.Name ?? "Uncategorized")
                .Select(g => new ReportCategoryRow
                {
                    Category = g.Key,
                    TotalQuantity = g.Sum(i => i.Quantity),
                    TotalRevenue = g.Sum(i => i.Total),
                    OrderCount = g.Select(i => i.OrderId).Distinct().Count()
                })
                .OrderByDescending(c => c.TotalRevenue)
                .ToList();

            resp.IsSuccess = true;
            resp.Data = new CategoryReportResult { Items = categoryData };
            return resp;
        }

        public async Task<IResponse> GetProductReport(ReportFilterRequest filter)
        {
            var query = db.Orders
                .Where(o => !o.IsDeleted)
                .Include(o => o.Items).ThenInclude(i => i.Variant).ThenInclude(v => v!.Product).ThenInclude(p => p!.Category)
                .AsQueryable();

            if (filter.FromDate.HasValue)
                query = query.Where(o => o.CreatedOn >= filter.FromDate.Value);
            if (filter.ToDate.HasValue)
                query = query.Where(o => o.CreatedOn <= filter.ToDate.Value);
            if (filter.ProductId.HasValue)
                query = query.Where(o => o.Items.Any(i => i.Variant!.ProductId == filter.ProductId.Value));
            if (filter.CategoryId.HasValue)
                query = query.Where(o => o.Items.Any(i => i.Variant!.Product!.CategoryId == filter.CategoryId.Value));

            var orders = await query.ToListAsync();

            var productData = orders
                .SelectMany(o => o.Items)
                .GroupBy(i => i.Variant?.Product?.Name ?? "Unknown")
                .Select(g => new ReportProductRow
                {
                    ProductName = g.Key,
                    TotalQuantity = g.Sum(i => i.Quantity),
                    TotalRevenue = g.Sum(i => i.Total),
                    AveragePrice = g.Average(i => i.UnitPrice),
                    OrderCount = g.Select(i => i.OrderId).Distinct().Count()
                })
                .OrderByDescending(p => p.TotalRevenue)
                .ToList();

            resp.IsSuccess = true;
            resp.Data = new ProductReportResult { Items = productData };
            return resp;
        }
    }
}
