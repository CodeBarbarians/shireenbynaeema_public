namespace Domain
{
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SharedServices;

[Table("CartItem")]
[EntityDisplayName("Cart Item")]
public class CartItem : Auditable, IIdentifiable
{
    [Key]
    public Guid Id { get; set; }

    public Guid CartId { get; set; }

    [ForeignKey(nameof(CartId))]
    public Cart? Cart { get; set; }

    public Guid ProductVariantId { get; set; }

    [ForeignKey(nameof(ProductVariantId))]
    public ProductVariant? Variant { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}
}