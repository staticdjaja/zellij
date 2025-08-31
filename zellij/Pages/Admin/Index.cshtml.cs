using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using zellij.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace zellij.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public int ProductCount { get; set; }
        public int UserCount { get; set; }
        public int InStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }

        public async Task OnGetAsync()
        {
            ProductCount = await _context.Products.CountAsync();
            UserCount = _userManager.Users.Count();
            InStockProducts = await _context.Products.CountAsync(p => p.InStock && p.StockQuantity > 0);
            OutOfStockProducts = await _context.Products.CountAsync(p => !p.InStock || p.StockQuantity == 0);
        }
    }
}