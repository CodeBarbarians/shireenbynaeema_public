namespace Domain
{
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SharedServices;

[Table("Order")]
[EntityDisplayName("Order")]
public class Order : SoftDeletableAuditable, IIdentifiable
{
    public Order()
    {
        OrderNumber = string.Empty;
        Notes = string.Empty;
    }

    [Key]
    public Guid Id { get; set; }

    public string OrderNumber { get; set; }

    public Guid CustomerId { get; set; }

    [ForeignKey(nameof(CustomerId))]
    public Customer? Customer { get; set; }

    public string Status { get; set; } = "Pending";

    public decimal Subtotal { get; set; }

    public decimal Tax { get; set; }

    public decimal Shipping { get; set; }

    public decimal Total { get; set; }

    public decimal Discount { get; set; }

    public string? CouponCode { get; set; }

    public Guid? ShippingAddressId { get; set; }

    [ForeignKey(nameof(ShippingAddressId))]
    public Address? ShippingAddress { get; set; }

    public Guid? BillingAddressId { get; set; }

    [ForeignKey(nameof(BillingAddressId))]
    public Address? BillingAddress { get; set; }

    public string Notes { get; set; }

    public string? PaymentIntentId { get; set; }

    public ICollection<OrderItem> Items { get; set; } = [];

    public Invoice? Invoice { get; set; }

    public ICollection<Delivery> Deliveries { get; set; } = [];

    public ICollection<Return> Returns { get; set; } = [];

    public ICollection<OrderCoupon> OrderCoupons { get; set; } = [];
}
}