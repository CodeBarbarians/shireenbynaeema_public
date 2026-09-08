namespace Domain
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using SharedServices;

    [Table("Coupon")]
    [EntityDisplayName("Coupon")]
    public class Coupon : SoftDeletableAuditable, IIdentifiable
    {
        [Key]
        public Guid Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string DiscountType { get; set; } = "Percentage";

        public decimal DiscountValue { get; set; }

        public decimal? MinOrderAmount { get; set; }

        public int? MaxUses { get; set; }

        public int UsedCount { get; set; }

        public DateTime? ExpiresOn { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<OrderCoupon> OrderCoupons { get; set; } = [];
    }

    [Table("OrderCoupon")]
    [EntityDisplayName("OrderCoupon")]
    public class OrderCoupon : Auditable, IIdentifiable
    {
        [Key]
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order? Order { get; set; }

        public Guid CouponId { get; set; }

        [ForeignKey(nameof(CouponId))]
        public Coupon? Coupon { get; set; }

        public decimal DiscountApplied { get; set; }
    }
}
