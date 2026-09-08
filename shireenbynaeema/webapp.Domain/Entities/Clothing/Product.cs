namespace Domain
{
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SharedServices;

[Table("Product")]
[EntityDisplayName("Product")]
public class Product : SoftDeletableAuditable, IIdentifiable
{
    public Product()
    {
        Name = string.Empty;
        Slug = string.Empty;
        Description = string.Empty;
        SKU = string.Empty;
        Brand = string.Empty;
    }

    [Key]
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Slug { get; set; }

    public string Description { get; set; }

    public string SKU { get; set; }

    public decimal BasePrice { get; set; }

    public decimal? SalePrice { get; set; }

    public Guid CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public Category? Category { get; set; }

    public string Brand { get; set; }

    public bool IsFeatured { get; set; }

    public bool IsActive { get; set; } = true;

    public decimal Rating { get; set; }

    public int ReviewCount { get; set; }

    public string SizeChartJson { get; set; } = "[]";

    public ICollection<ProductVariant> Variants { get; set; } = [];

    public ICollection<ProductImage> Images { get; set; } = [];

    public ICollection<Rating> Ratings { get; set; } = [];
}
}