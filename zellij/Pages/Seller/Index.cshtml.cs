using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using zellij.Data;
using zellij.Models;

namespace zellij.Pages.Seller
{
    [Authorize(Roles = "Admin,Seller")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int ShippedOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<Order> RecentOrders { get; set; } = new();

        public async Task OnGetAsync()
        {
            var orders = await _context.Orders.Include(o => o.User).ToListAsync();

            TotalOrders = orders.Count;
            PendingOrders = orders.Count(o => o.Status == OrderStatus.Pending);
            ProcessingOrders = orders.Count(o => o.Status == OrderStatus.Processing);
            ShippedOrders = orders.Count(o => o.Status == OrderStatus.Shipped);
            DeliveredOrders = orders.Count(o => o.Status == OrderStatus.Delivered);
            TotalRevenue = orders.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.Total);

            RecentOrders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.ShippingAddress)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .ToListAsync();
        }
    }
}