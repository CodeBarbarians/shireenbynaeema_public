namespace Domain
{
    public class SalesReportResult
    {
        public List<ReportSalesRow> Items { get; set; } = [];
        public ReportSummary Summary { get; set; } = new();
    }

    public class CategoryReportResult
    {
        public List<ReportCategoryRow> Items { get; set; } = [];
    }

    public class ProductReportResult
    {
        public List<ReportProductRow> Items { get; set; } = [];
    }

    public class ReportFilterRequest
    {
        public long? FromDate { get; set; }
        public long? ToDate { get; set; }
        public Guid? ProductId { get; set; }
        public Guid? CategoryId { get; set; }
    }

    public class ReportSalesRow
    {
        public string OrderNumber { get; set; } = string.Empty;
        public string OrderDate { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ReportCategoryRow
    {
        public string Category { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class ReportProductRow
    {
        public string ProductName { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AveragePrice { get; set; }
        public int OrderCount { get; set; }
    }

    public class ReportSummary
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalItems { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int CancelledOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int PendingOrders { get; set; }
    }
}
