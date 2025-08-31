using Microsoft.AspNetCore.Mvc.RazorPages;
using zellij.Models;
using zellij.Services;

namespace zellij.Pages.Products
{
    public class IndexModel : PageModel
    {
        private readonly IProductService _productService;

        public IndexModel(IProductService productService)
        {
            _productService = productService;
        }

        public IList<Product> Products { get; set; } = default!;
        public string? SearchTerm { get; set; }
        public string? SelectedOrigin { get; set; }
        public string? SelectedColor { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public async Task OnGetAsync(string? searchTerm, string? origin, string? color, decimal? minPrice, decimal? maxPrice)
        {
            SearchTerm = searchTerm;
            SelectedOrigin = origin;
            SelectedColor = color;
            MinPrice = minPrice;
            MaxPrice = maxPrice;

            Products = (await _productService.SearchProductsAsync(searchTerm, origin, color, minPrice, maxPrice)).ToList();
        }
    }
}