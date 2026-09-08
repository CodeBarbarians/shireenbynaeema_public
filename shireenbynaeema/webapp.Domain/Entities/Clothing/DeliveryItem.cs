namespace Domain
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using SharedServices;

    [Table("DeliveryItem")]
    [EntityDisplayName("Delivery Item")]
    public class DeliveryItem : Auditable, IIdentifiable
    {
        [Key]
        public Guid Id { get; set; }

        public Guid DeliveryId { get; set; }

        [ForeignKey(nameof(DeliveryId))]
        public Delivery? Delivery { get; set; }

        public Guid OrderItemId { get; set; }

        [ForeignKey(nameof(OrderItemId))]
        public OrderItem? OrderItem { get; set; }

        public int Quantity { get; set; }
    }
}
