using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using zellij.Data;
using zellij.Models;

namespace zellij.Services
{
    public class CouponService : ICouponService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<CouponService> _logger;

        public CouponService(ApplicationDbContext context, UserManager<IdentityUser> userManager, ILogger<CouponService> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<Coupon?> GetCouponByCodeAsync(string code)
        {
            return await _context.Coupons
                .FirstOrDefaultAsync(c => c.Code.ToUpper() == code.ToUpper() && c.IsActive);
        }

        public async Task<bool> CanUserUseCouponAsync(string userId, string couponCode)
        {
            var coupon = await GetCouponByCodeAsync(couponCode);
            if (coupon == null || !coupon.IsValid)
            {
                return false;
            }

            // Check if email confirmation is required
            if (coupon.RequireEmailConfirmation)
            {
                var isEmailConfirmed = await IsUserEmailConfirmedAsync(userId);
                if (!isEmailConfirmed)
                {
                    return false;
                }
            }

            // Check if user has already used this coupon
            var hasUsed = await HasUserUsedCouponAsync(userId, coupon.Id);
            return !hasUsed;
        }

        public async Task<bool> HasUserUsedCouponAsync(string userId, int couponId)
        {
            return await _context.CouponUsages
                .AnyAsync(cu => cu.UserId == userId && cu.CouponId == couponId);
        }

        public async Task<decimal> CalculateDiscountAsync(Coupon coupon, decimal orderAmount)
        {
            if (coupon.MinimumOrderAmount.HasValue && orderAmount < coupon.MinimumOrderAmount.Value)
            {
                return 0;
            }

            return Math.Round(orderAmount * (coupon.DiscountPercentage / 100), 2);
        }

        public async Task<bool> ApplyCouponToOrderAsync(string userId, string couponCode, int orderId, decimal discountAmount)
        {
            try
            {
                var coupon = await GetCouponByCodeAsync(couponCode);
                if (coupon == null)
                {
                    return false;
                }

                // Create coupon usage record
                var couponUsage = new CouponUsage
                {
                    CouponId = coupon.Id,
                    UserId = userId,
                    OrderId = orderId,
                    DiscountAmount = discountAmount,
                    UsedDate = DateTime.Now
                };

                _context.CouponUsages.Add(couponUsage);

                // Update coupon usage count
                coupon.TimesUsed++;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Applied coupon {CouponCode} to order {OrderId} for user {UserId}", couponCode, orderId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying coupon {CouponCode} to order {OrderId} for user {UserId}", couponCode, orderId, userId);
                return false;
            }
        }

        public async Task<List<Coupon>> GetActiveCouponsAsync()
        {
            return await _context.Coupons
                .Where(c => c.IsActive && c.ValidFrom <= DateTime.Now && c.ValidUntil >= DateTime.Now)
                .OrderBy(c => c.ValidUntil)
                .ToListAsync();
        }

        public async Task<bool> IsUserEmailConfirmedAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user?.EmailConfirmed ?? false;
        }
    }
}