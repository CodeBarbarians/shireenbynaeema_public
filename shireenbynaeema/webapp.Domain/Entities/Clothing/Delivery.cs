namespace Domain
{
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SharedServices;

[Table("Delivery")]
[EntityDisplayName("Delivery")]
public class Delivery : SoftDeletableAuditable, IIdentifiable
{
    public Delivery()
    {
        Carrier = string.Empty;
        TrackingNumber = string.Empty;
    }

    [Key]
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    [ForeignKey(nameof(OrderId))]
    public Order? Order { get; set; }

    public string Carrier { get; set; }

    public string TrackingNumber { get; set; }

    public string Status { get; set; } = "Pending";

    public DateTime? ShippedOn { get; set; }

    public DateTime? DeliveredOn { get; set; }

    public DateTime? EstimatedDelivery { get; set; }

    public ICollection<DeliveryItem> Items { get; set; } = [];
}
}