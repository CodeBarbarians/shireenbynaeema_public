namespace Domain
{
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SharedServices;

[Table("ProductVariant")]
[EntityDisplayName("Product Variant")]
public class ProductVariant : Auditable, IIdentifiable
{
    public ProductVariant()
    {
        Size = string.Empty;
        Color = string.Empty;
        ColorHex = string.Empty;
        SKU = string.Empty;
    }

    [Key]
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; set; }

    public string Size { get; set; }

    public string Color { get; set; }

    public string ColorHex { get; set; }

    public int Stock { get; set; }

    public int BookedStock { get; set; }

    public decimal Price { get; set; }

    public string SKU { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<CartItem> CartItems { get; set; } = [];

    public ICollection<OrderItem> OrderItems { get; set; } = [];
}
}