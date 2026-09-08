namespace Domain
{
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SharedServices;

[Table("ReturnItem")]
[EntityDisplayName("Return Item")]
public class ReturnItem : Auditable, IIdentifiable
{
    [Key]
    public Guid Id { get; set; }

    public Guid ReturnId { get; set; }

    [ForeignKey(nameof(ReturnId))]
    public Return? Return { get; set; }

    public Guid OrderItemId { get; set; }

    [ForeignKey(nameof(OrderItemId))]
    public OrderItem? OrderItem { get; set; }

    public int Quantity { get; set; }

    public string Reason { get; set; } = string.Empty;
}
}
