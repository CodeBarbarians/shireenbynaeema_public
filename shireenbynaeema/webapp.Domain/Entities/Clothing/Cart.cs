namespace Domain
{
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SharedServices;

[Table("Cart")]
[EntityDisplayName("Cart")]
public class Cart : Auditable, IIdentifiable
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    public decimal Subtotal { get; set; }

    public ICollection<CartItem> Items { get; set; } = [];
}
}