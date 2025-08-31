using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using zellij.Models;
using zellij.Services;

namespace zellij.Pages.Checkout
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ICartService _cartService;
        private readonly ICouponService _couponService;
        private readonly IOrderService _orderService;
        private readonly IUserAddressService _userAddressService;

        public IndexModel(
            ICartService cartService,
            ICouponService couponService,
            IOrderService orderService,
            IUserAddressService userAddressService)
        {
            _cartService = cartService;
            _couponService = couponService;
            _orderService = orderService;
            _userAddressService = userAddressService;
        }

        public CartSummary CartSummary { get; set; } = new();
        public List<UserAddress> UserAddresses { get; set; } = new();
        public UserAddress? DefaultAddress { get; set; }

        [BindProperty]
        public CheckoutForm Form { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // Get cart summary
            CartSummary = await _cartService.GetCartSummaryAsync(userId);

            if (!CartSummary.Items.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty. Please add items before checkout.";
                return RedirectToPage("/Cart/Index");
            }

            // Get user addresses
            UserAddresses = (await _userAddressService.GetUserAddressesAsync(userId)).ToList();

            if (!UserAddresses.Any())
            {
                TempData["ErrorMessage"] = "Please add a delivery address before checkout.";
                return RedirectToPage("/Account/Addresses/Create");
            }

            DefaultAddress = await _userAddressService.GetDefaultAddressAsync(userId);
            Form.ShippingAddressId = DefaultAddress?.Id ?? UserAddresses.First().Id;

            // Check for applied coupon in session
            var appliedCoupon = HttpContext.Session.GetString("AppliedCoupon");
            if (!string.IsNullOrEmpty(appliedCoupon))
            {
                var coupon = await _couponService.GetCouponByCodeAsync(appliedCoupon);
                if (coupon != null && await _couponService.CanUserUseCouponAsync(userId, appliedCoupon))
                {
                    var discount = await _couponService.CalculateDiscountAsync(coupon, CartSummary.SubTotal);
                    CartSummary.DiscountAmount = discount;
                    CartSummary.Total = CartSummary.SubTotal + CartSummary.Tax + CartSummary.ShippingCost - discount;
                    CartSummary.AppliedCoupon = coupon;
                    Form.CouponCode = appliedCoupon;
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (!ModelState.IsValid)
            {
                await LoadPageDataAsync(userId);
                return Page();
            }

            // Verify the selected address belongs to the user
            var selectedAddress = await _userAddressService.GetUserAddressAsync(userId, Form.ShippingAddressId);

            if (selectedAddress == null)
            {
                ModelState.AddModelError("", "Invalid shipping address selected.");
                await LoadPageDataAsync(userId);
                return Page();
            }

            // Create the order
            var order = await _orderService.CreateOrderAsync(
                userId,
                Form.ShippingAddressId,
                Form.BillingAddressId,
                Form.CouponCode,
                Form.OrderNotes);

            if (order == null)
            {
                ErrorMessage = "Unable to create order. Please check your cart and try again.";
                await LoadPageDataAsync(userId);
                return Page();
            }

            // Clear applied coupon from session
            HttpContext.Session.Remove("AppliedCoupon");

            // Redirect to order confirmation
            TempData["SuccessMessage"] = $"Order #{order.OrderNumber} placed successfully! Your order will be delivered with cash on delivery.";
            return RedirectToPage("/Orders/Details", new { id = order.Id });
        }

        private async Task LoadPageDataAsync(string userId)
        {
            CartSummary = await _cartService.GetCartSummaryAsync(userId);
            UserAddresses = (await _userAddressService.GetUserAddressesAsync(userId)).ToList();
        }
    }

    public class CheckoutForm
    {
        public int ShippingAddressId { get; set; }
        public int? BillingAddressId { get; set; }
        public string? CouponCode { get; set; }
        public string? OrderNotes { get; set; }
        public bool AgreeToTerms { get; set; }
    }
}