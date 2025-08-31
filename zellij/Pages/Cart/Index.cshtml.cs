using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using zellij.Services;

namespace zellij.Pages.Cart
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ICartService _cartService;
        private readonly ICouponService _couponService;

        public IndexModel(ICartService cartService, ICouponService couponService)
        {
            _cartService = cartService;
            _couponService = couponService;
        }

        public CartSummary CartSummary { get; set; } = new();

        [BindProperty]
        public string CouponCode { get; set; } = string.Empty;

        public string? CouponMessage { get; set; }
        public bool CouponApplied { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            CartSummary = await _cartService.GetCartSummaryAsync(userId);
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateQuantityAsync(int productId, int quantity)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (quantity <= 0)
            {
                await _cartService.RemoveFromCartAsync(userId, productId);
                TempData["SuccessMessage"] = "Item removed from cart.";
            }
            else
            {
                var success = await _cartService.UpdateCartItemAsync(userId, productId, quantity);
                if (success)
                {
                    TempData["SuccessMessage"] = "Cart updated successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Unable to update cart. Please check stock availability.";
                }
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveItemAsync(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var success = await _cartService.RemoveFromCartAsync(userId, productId);
            if (success)
            {
                TempData["SuccessMessage"] = "Item removed from cart.";
            }
            else
            {
                TempData["ErrorMessage"] = "Unable to remove item from cart.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostApplyCouponAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (string.IsNullOrWhiteSpace(CouponCode))
            {
                TempData["ErrorMessage"] = "Please enter a coupon code.";
                return RedirectToPage();
            }

            var canUse = await _couponService.CanUserUseCouponAsync(userId, CouponCode);
            if (!canUse)
            {
                var coupon = await _couponService.GetCouponByCodeAsync(CouponCode);
                if (coupon == null)
                {
                    TempData["ErrorMessage"] = "Invalid coupon code.";
                }
                else if (!coupon.IsValid)
                {
                    TempData["ErrorMessage"] = "This coupon has expired or is no longer valid.";
                }
                else if (await _couponService.HasUserUsedCouponAsync(userId, coupon.Id))
                {
                    TempData["ErrorMessage"] = "You have already used this coupon.";
                }
                else if (coupon.RequireEmailConfirmation && !await _couponService.IsUserEmailConfirmedAsync(userId))
                {
                    TempData["ErrorMessage"] = "Please confirm your email address before using coupons.";
                }
                else
                {
                    TempData["ErrorMessage"] = "This coupon cannot be applied to your order.";
                }
            }
            else
            {
                // Store coupon in session for checkout
                HttpContext.Session.SetString("AppliedCoupon", CouponCode);
                TempData["SuccessMessage"] = $"Coupon '{CouponCode}' applied successfully!";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostClearCartAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var success = await _cartService.ClearCartAsync(userId);
            if (success)
            {
                TempData["SuccessMessage"] = "Cart cleared successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Unable to clear cart.";
            }

            return RedirectToPage();
        }
    }
}