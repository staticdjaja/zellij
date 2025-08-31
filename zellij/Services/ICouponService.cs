using zellij.Models;

namespace zellij.Services
{
    public interface ICouponService
    {
        Task<Coupon?> GetCouponByCodeAsync(string code);
        Task<bool> CanUserUseCouponAsync(string userId, string couponCode);
        Task<bool> HasUserUsedCouponAsync(string userId, int couponId);
        Task<decimal> CalculateDiscountAsync(Coupon coupon, decimal orderAmount);
        Task<bool> ApplyCouponToOrderAsync(string userId, string couponCode, int orderId, decimal discountAmount);
        Task<List<Coupon>> GetActiveCouponsAsync();
        Task<bool> IsUserEmailConfirmedAsync(string userId);
    }
}