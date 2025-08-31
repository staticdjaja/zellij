using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using zellij.Models;
using zellij.Services;

namespace zellij.Pages.Admin.Products
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IProductService _productService;

        public IndexModel(IProductService productService)
        {
            _productService = productService;
        }

        public IList<Product> Products { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrEmpty(SearchString))
            {
                Products = (await _productService.SearchProductsAsync(SearchString, null, null, null, null)).ToList();
            }
            else
            {
                Products = (await _productService.GetAllProductsAsync()).OrderBy(p => p.Name).ToList();
            }
        }
    }
}