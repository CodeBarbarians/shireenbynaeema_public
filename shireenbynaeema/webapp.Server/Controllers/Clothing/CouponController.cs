namespace Server
{
    using Application;
    using Domain;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using SharedServices;

    [Route("api/[controller]")]
    [ApiController]
    public class CouponController : ControllerBase
    {
        private readonly ICouponService couponService;

        public CouponController(ICouponService couponService) { this.couponService = couponService; }

        [HttpPost("Admin/List")]
        [Authorize]
        public async Task<ActionResult> List(ListRequest request)
        {
            var response = await couponService.ListCoupons(request);
            return Ok(response);
        }

        [HttpPost("Admin/Add")]
        [Authorize]
        public async Task<ActionResult> Add(Coupon_AddEdit request)
        {
            var coupon = new Coupon
            {
                Id = request.Id ?? Guid.NewGuid(),
                Code = request.Code.ToUpper(),
                Description = request.Description,
                DiscountType = request.DiscountType,
                DiscountValue = request.DiscountValue,
                MinOrderAmount = request.MinOrderAmount,
                MaxUses = request.MaxUses,
                ExpiresOn = request.ExpiresOn,
                IsActive = request.IsActive
            };

            if (request.Id.HasValue)
            {
                var response = await couponService.UpdateAsync(new SaveRequest<Coupon> { EntityId = request.Id, Entity = coupon });
                return Ok(response);
            }
            else
            {
                var response = await couponService.AddAsync(new SaveRequest<Coupon> { Entity = coupon });
                return Ok(response);
            }
        }

        [HttpPost("Validate")]
        public async Task<ActionResult> Validate(CouponApplyRequest request)
        {
            var response = await couponService.Validate(request.Code, request.OrderTotal);
            return Ok(response);
        }

        [HttpPost("Admin/Apply")]
        [Authorize]
        public async Task<ActionResult> Apply(CouponApplyToOrderRequest request)
        {
            var response = await couponService.Apply(request.OrderId, request.Code, request.Discount);
            return Ok(response);
        }

        [HttpDelete("Admin/Delete/{id}")]
        [Authorize]
        public async Task<ActionResult> Delete(Guid id)
        {
            var response = await couponService.DeleteAsync(id);
            return Ok(response);
        }
    }
}
