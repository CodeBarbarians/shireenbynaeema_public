namespace Domain
{
    using System.ComponentModel.DataAnnotations;
    using SharedServices;

    public class OrderPlaceRequest
    {
        public string? PaymentIntentId { get; set; }
        public string? Notes { get; set; }

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string Street { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        [Required]
        public string ZipCode { get; set; } = string.Empty;

        [Required]
        public string Country { get; set; } = string.Empty;

        public string? CouponCode { get; set; }
        public List<OrderItemRequest>? Items { get; set; }
    }

    public class OrderItemRequest
    {
        public Guid ProductVariantId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class OrderListRequest : ListRequest
    {
        public string? Search { get; set; }
        public string? Status { get; set; }
    }

    public class OrderStatusUpdateRequest
    {
        public string Status { get; set; } = string.Empty;
    }

    public class OrderDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Shipping { get; set; }
        public decimal Total { get; set; }
        public decimal Discount { get; set; }
        public string CouponCode { get; set; } = string.Empty;
        public AddressDto? ShippingAddress { get; set; }
        public string Notes { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = [];
        public long? CreatedOn { get; set; }
    }

    public class OrderItemDto
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
        public int DeliveredQuantity { get; set; }
        public int ReturnedQuantity { get; set; }
    }

    public class AddressDto
    {
        public Guid Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }
}
