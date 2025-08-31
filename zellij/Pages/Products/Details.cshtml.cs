using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using zellij.Data;
using zellij.Models;
using zellij.Services;
using System.Security.Claims;

namespace zellij.Pages.Products
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ICartService _cartService;

        public DetailsModel(ApplicationDbContext context, ICartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        public Product Product { get; set; } = default!;

        [BindProperty]
        public int Quantity { get; set; } = 1;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            else
            {
                Product = product;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (!User.Identity?.IsAuthenticated == true)
            {
                return Challenge();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var success = await _cartService.AddToCartAsync(userId, id.Value, Quantity);

            if (success)
            {
                TempData["SuccessMessage"] = $"Added {Quantity} item(s) to your cart!";
                return RedirectToPage("/Cart/Index");
            }
            else
            {
                TempData["ErrorMessage"] = "Unable to add item to cart. Please check stock availability.";
                return RedirectToPage(new { id });
            }
        }
    }
}