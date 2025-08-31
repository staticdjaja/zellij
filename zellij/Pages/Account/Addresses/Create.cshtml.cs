using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using zellij.Models;
using zellij.Services;

namespace zellij.Pages.Account.Addresses
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly IUserAddressService _userAddressService;

        public CreateModel(IUserAddressService userAddressService)
        {
            _userAddressService = userAddressService;
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

            await _userAddressService.CreateAddressAsync(Address);

            TempData["SuccessMessage"] = "Address added successfully!";
            return RedirectToPage("./Index");
        }
    }
}