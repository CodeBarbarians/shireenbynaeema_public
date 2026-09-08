namespace Domain
{
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SharedServices;

[Table("SizeChart")]
[EntityDisplayName("Size Chart")]
public class SizeChart : Auditable, IIdentifiable
{
    [Key]
    public Guid Id { get; set; }

    public Guid CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public Category? Category { get; set; }

    public string SizeName { get; set; } = string.Empty;

    public string MeasurementsJson { get; set; } = "{}";
}
}