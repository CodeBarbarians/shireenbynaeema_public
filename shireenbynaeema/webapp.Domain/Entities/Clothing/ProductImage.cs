namespace Domain
{
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SharedServices;

[Table("ProductImage")]
[EntityDisplayName("Product Image")]
public class ProductImage : Auditable, IIdentifiable
{
    public ProductImage()
    {
        ImageUrl = string.Empty;
        AltText = string.Empty;
    }

    [Key]
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; set; }

    public string ImageUrl { get; set; }

    public string AltText { get; set; }

    public int SortOrder { get; set; }

    public bool IsPrimary { get; set; }
}
}