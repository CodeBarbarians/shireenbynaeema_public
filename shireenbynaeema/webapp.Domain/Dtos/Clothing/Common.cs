namespace Domain
{
public class InvoiceDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Tax { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public long? CreatedOn { get; set; }
}

public class DeliveryDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Carrier { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ShippedOn { get; set; }
    public string? DeliveredOn { get; set; }
    public string? EstimatedDelivery { get; set; }
    public List<DeliveryItemDto> Items { get; set; } = [];
}

public class DeliveryItemDto
{
    public Guid Id { get; set; }
    public Guid OrderItemId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int OrderedQuantity { get; set; }
}

public class DeliveryCreateRequest
{
    public Guid OrderId { get; set; }
    public string Carrier { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public string? EstimatedDelivery { get; set; }
    public List<DeliveryItemRequest>? Items { get; set; }
}

public class DeliveryItemRequest
{
    public Guid OrderItemId { get; set; }
    public int Quantity { get; set; }
}

public class DeliveryUpdateRequest
{
    public string Status { get; set; } = string.Empty;
}

public class ReturnDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal? RefundAmount { get; set; }
    public string? RefundPaymentId { get; set; }
    public string BankAccount { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;
    public string Attachments { get; set; } = string.Empty;
    public string? RequestedOn { get; set; }
    public string? ProcessedOn { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<ReturnItemDto> Items { get; set; } = [];
}

public class ReturnItemDto
{
    public Guid Id { get; set; }
    public Guid OrderItemId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int OrderedQuantity { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class ReturnRequestDto
{
    public Guid OrderId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string BankAccount { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;
    public string? Attachments { get; set; }
    public List<ReturnItemRequest>? Items { get; set; }
}

public class ReturnItemRequest
{
    public Guid OrderItemId { get; set; }
    public int Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class ReturnProcessDto
{
    public string Status { get; set; } = string.Empty;
    public decimal? RefundAmount { get; set; }
    public string? Notes { get; set; }
}

public class CustomerDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public List<AddressDto> Addresses { get; set; } = [];
    public List<OrderDto> Orders { get; set; } = [];
    public List<InvoiceDto> Invoices { get; set; } = [];
    public int OrderCount { get; set; }
    public decimal TotalSpent { get; set; }
    public long? CreatedOn { get; set; }
}

public class ReviewDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Score { get; set; }
    public string Comment { get; set; } = string.Empty;
    public long CreatedOn { get; set; }
}

public class ReviewCreateRequest
{
    public Guid ProductId { get; set; }
    public int Score { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class DashboardStatsDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalProducts { get; set; }
}

public class SettingDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
}
}