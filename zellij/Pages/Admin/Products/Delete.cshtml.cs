using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using zellij.Models;
using zellij.Services;

namespace zellij.Pages.Admin.Products
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly IProductService _productService;

        public DeleteModel(IProductService productService)
        {
            _productService = productService;
        }

        [BindProperty]
        public Product Product { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _productService.GetProductAsync(id.Value);

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

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _productService.GetProductAsync(id.Value);
            if (product != null)
            {
                Product = product;
                var success = await _productService.DeleteProductAsync(id.Value);
                if (success)
                {
                    TempData["SuccessMessage"] = $"Product '{Product.Name}' has been deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Unable to delete product.";
                }
            }

            return RedirectToPage("./Index");
        }
    }
}