namespace Domain
{
    public class Coupon_Listing
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DiscountType { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public int? MaxUses { get; set; }
        public int UsedCount { get; set; }
        public DateTime? ExpiresOn { get; set; }
        public bool IsActive { get; set; }
    }

    public class Coupon_AddEdit
    {
        public Guid? Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DiscountType { get; set; } = "Percentage";
        public decimal DiscountValue { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public int? MaxUses { get; set; }
        public DateTime? ExpiresOn { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CouponApplyRequest
    {
        public string Code { get; set; } = string.Empty;
        public decimal OrderTotal { get; set; }
    }

    public class CouponApplyToOrderRequest
    {
        public Guid OrderId { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal Discount { get; set; }
    }

    public class BlogPost_Listing
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;
        public string CoverImageUrl { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public long? PublishedOn { get; set; }
        public int ViewCount { get; set; }
    }

    public class BlogPost_AddEdit
    {
        public Guid? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string CoverImageUrl { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
    }
}
