namespace Domain
{
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SharedServices;

[Table("OrderItem")]
[EntityDisplayName("Order Item")]
public class OrderItem : Auditable, IIdentifiable
{
    [Key]
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    [ForeignKey(nameof(OrderId))]
    public Order? Order { get; set; }

    public Guid ProductVariantId { get; set; }

    [ForeignKey(nameof(ProductVariantId))]
    public ProductVariant? Variant { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Total { get; set; }
}
}