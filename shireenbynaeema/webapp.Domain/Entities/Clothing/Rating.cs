namespace Domain
{
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SharedServices;

[Table("Rating")]
[EntityDisplayName("Rating")]
public class Rating : Auditable, IIdentifiable
{
    public Rating()
    {
        Comment = string.Empty;
    }

    [Key]
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; set; }

    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    public int Score { get; set; }

    public string Comment { get; set; }
}
}