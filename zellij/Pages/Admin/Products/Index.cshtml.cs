using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using zellij.Data;
using zellij.Models;
using Microsoft.AspNetCore.Authorization;

namespace zellij.Pages.Admin.Products
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Product> Products { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }

        public async Task OnGetAsync()
        {
            IQueryable<Product> productsQuery = _context.Products;

            if (!string.IsNullOrEmpty(SearchString))
            {
                productsQuery = productsQuery.Where(p => 
                    p.Name.Contains(SearchString) || 
                    p.MarbleType.Contains(SearchString) || 
                    p.Origin.Contains(SearchString) ||
                    p.Color.Contains(SearchString));
            }

            Products = await productsQuery
                .OrderBy(p => p.Name)
                .ToListAsync();
        }
    }
}