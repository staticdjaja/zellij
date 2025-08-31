using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using zellij.Data;
using zellij.Models;

namespace zellij.Pages.Account.Addresses
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public UserAddress Address { get; set; } = new();

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            //if (!ModelState.IsValid)
            //{
            //    return Page();
            //}

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            Address.UserId = userId;
            Address.CreatedDate = DateTime.Now;

            // Check if this should be the default address (if no other addresses exist)
            var existingAddresses = _context.UserAddresses.Where(ua => ua.UserId == userId).Any();
            if (!existingAddresses)
            {
                Address.IsDefault = true;
            }

            _context.UserAddresses.Add(Address);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Address added successfully!";
            return RedirectToPage("./Index");
        }
    }
}