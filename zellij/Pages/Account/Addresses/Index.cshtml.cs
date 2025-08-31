using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using zellij.Data;
using zellij.Models;

namespace zellij.Pages.Account.Addresses
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<UserAddress> Addresses { get; set; } = new();
        public bool HasDefaultAddress { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            Addresses = await _context.UserAddresses
                .Where(ua => ua.UserId == userId)
                .OrderByDescending(ua => ua.IsDefault)
                .ThenByDescending(ua => ua.CreatedDate)
                .ToListAsync();

            HasDefaultAddress = Addresses.Any(a => a.IsDefault);

            return Page();
        }

        public async Task<IActionResult> OnPostSetDefaultAsync(int addressId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // Set IsDefault = false for all addresses of the user (single DB call)
            await _context.UserAddresses
                .Where(ua => ua.UserId == userId && ua.IsDefault)
                .ExecuteUpdateAsync(setters => setters.SetProperty(ua => ua.IsDefault, false));

            // Set IsDefault = true for the selected address (single DB call)
            await _context.UserAddresses
                .Where(ua => ua.UserId == userId && ua.Id == addressId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(ua => ua.IsDefault, true));

            TempData["SuccessMessage"] = "Default address updated successfully.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int addressId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            var address = await _context.UserAddresses
                .FirstOrDefaultAsync(ua => ua.Id == addressId && ua.UserId == userId);

            if (address == null)
            {
                TempData["ErrorMessage"] = "Address not found.";
                return RedirectToPage();
            }

            // Check if this address is used in any orders
            var hasOrders = await _context.Orders
                .AnyAsync(o => o.ShippingAddressId == addressId || o.BillingAddressId == addressId);

            if (hasOrders)
            {
                TempData["ErrorMessage"] = "Cannot delete address as it's associated with existing orders.";
                return RedirectToPage();
            }

            _context.UserAddresses.Remove(address);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Address deleted successfully.";
            return RedirectToPage();
        }
    }
}