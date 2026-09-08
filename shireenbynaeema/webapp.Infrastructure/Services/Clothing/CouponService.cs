namespace Infrastructure
{
    using Application;
    using Domain;
    using Microsoft.EntityFrameworkCore;
    using SharedServices;

    public class CouponService : Service<Coupon>, ICouponService
    {
        private readonly DatabaseContext db;
        private readonly IResponse resp;

        public CouponService(IRepository<Coupon> repository, IResponse response, DatabaseContext db)
            : base(repository, response)
        {
            this.db = db;
            this.resp = response;
        }

        public async Task<IResponse> ListCoupons(ListRequest request)
        {
            var query = db.Coupons.AsNoTracking().AsQueryable();
            var totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(c => c.CreatedOn)
                .Skip(request.Skip).Take(request.Take)
                .Select(c => new Coupon_Listing
                {
                    Id = c.Id, Code = c.Code, Description = c.Description,
                    DiscountType = c.DiscountType, DiscountValue = c.DiscountValue,
                    MinOrderAmount = c.MinOrderAmount, MaxUses = c.MaxUses,
                    UsedCount = c.UsedCount, ExpiresOn = c.ExpiresOn,
                    IsActive = c.IsActive
                }).ToListAsync();

            resp.IsSuccess = true;
            resp.Data = items.ToListResponse(request, totalCount);
            return resp;
        }

        public async Task<IResponse> Validate(string code, decimal orderTotal)
        {
            var coupon = await db.Coupons.FirstOrDefaultAsync(c => c.Code == code && c.IsActive && !c.IsDeleted);
            if (coupon == null)
            {
                resp.IsSuccess = false;
                resp.Message = "Invalid coupon code";
                return resp;
            }

            if (coupon.ExpiresOn.HasValue && coupon.ExpiresOn.Value < DateTime.UtcNow)
            {
                resp.IsSuccess = false;
                resp.Message = "Coupon has expired";
                return resp;
            }

            if (coupon.MaxUses.HasValue && coupon.UsedCount >= coupon.MaxUses.Value)
            {
                resp.IsSuccess = false;
                resp.Message = "Coupon usage limit reached";
                return resp;
            }

            if (coupon.MinOrderAmount.HasValue && orderTotal < coupon.MinOrderAmount.Value)
            {
                resp.IsSuccess = false;
                resp.Message = $"Minimum order amount is Rs. {coupon.MinOrderAmount.Value}";
                return resp;
            }

            var discount = coupon.DiscountType == "Percentage"
                ? orderTotal * coupon.DiscountValue / 100
                : coupon.DiscountValue;

            resp.IsSuccess = true;
            resp.Data = new { coupon.Id, coupon.Code, coupon.DiscountType, coupon.DiscountValue, Discount = discount };
            return resp;
        }

        public async Task<IResponse> Apply(Guid orderId, string code, decimal discount)
        {
            var coupon = await db.Coupons.FirstOrDefaultAsync(c => c.Code == code && c.IsActive && !c.IsDeleted);
            if (coupon == null)
            {
                resp.IsSuccess = false;
                resp.Message = "Invalid coupon";
                return resp;
            }

            var orderCoupon = new OrderCoupon
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                CouponId = coupon.Id,
                DiscountApplied = discount
            };
            db.OrderCoupons.Add(orderCoupon);

            coupon.UsedCount++;
            await db.SaveChangesAsync();

            resp.IsSuccess = true;
            return resp;
        }
    }
}
