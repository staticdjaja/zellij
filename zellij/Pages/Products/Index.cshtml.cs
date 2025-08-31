using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using zellij.Data;
using zellij.Models;

namespace zellij.Pages.Products
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
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

            var query = _context.Products.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) || 
                                        p.Description.Contains(searchTerm) || 
                                        p.MarbleType.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(origin))
            {
                query = query.Where(p => p.Origin == origin);
            }

            if (!string.IsNullOrEmpty(color))
            {
                query = query.Where(p => p.Color == color);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice);
            }

            Products = await query.Where(p => p.InStock).ToListAsync();
        }
    }
}