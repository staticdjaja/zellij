using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using zellij.Models;
using zellij.Services;

namespace zellij.Pages.Account.Addresses
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IUserAddressService _userAddressService;

        public IndexModel(IUserAddressService userAddressService)
        {
            _userAddressService = userAddressService;
        }

        public List<UserAddress> Addresses { get; set; } = new();
        public bool HasDefaultAddress { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            Addresses = (await _userAddressService.GetUserAddressesAsync(userId)).ToList();
            HasDefaultAddress = Addresses.Any(a => a.IsDefault);

            return Page();
        }

        public async Task<IActionResult> OnPostSetDefaultAsync(int addressId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var success = await _userAddressService.SetDefaultAddressAsync(userId, addressId);

            if (success)
            {
                TempData["SuccessMessage"] = "Default address updated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Unable to update default address.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int addressId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // Check if this address is used in any orders
            if (await _userAddressService.IsAddressUsedInOrdersAsync(addressId))
            {
                TempData["ErrorMessage"] = "Cannot delete address as it's associated with existing orders.";
                return RedirectToPage();
            }

            var success = await _userAddressService.DeleteAddressAsync(userId, addressId);
            if (success)
            {
                TempData["SuccessMessage"] = "Address deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Address not found.";
            }

            return RedirectToPage();
        }
    }
}