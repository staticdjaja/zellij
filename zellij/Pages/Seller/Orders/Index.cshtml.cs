using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using zellij.Data;
using zellij.Models;

namespace zellij.Pages.Seller.Orders
{
    [Authorize(Roles = "Admin,Seller")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Order> Orders { get; set; } = new();
        public string? CurrentFilter { get; set; }
        public OrderStatus? StatusFilter { get; set; }
        public string? SearchString { get; set; }

        [BindProperty]
        public int OrderId { get; set; }

        [BindProperty]
        public OrderStatus NewStatus { get; set; }

        public async Task OnGetAsync(string? searchString, string? status)
        {
            SearchString = searchString;
            
            if (Enum.TryParse<OrderStatus>(status, out var statusEnum))
            {
                StatusFilter = statusEnum;
                CurrentFilter = status;
            }

            var ordersQuery = _context.Orders
                .Include(o => o.User)
                .Include(o => o.ShippingAddress)
                .Include(o => o.OrderItems)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                ordersQuery = ordersQuery.Where(o => 
                    o.OrderNumber.Contains(searchString) ||
                    o.User.UserName!.Contains(searchString) ||
                    o.User.Email!.Contains(searchString));
            }

            if (StatusFilter.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.Status == StatusFilter.Value);
            }

            Orders = await ordersQuery
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync()
        {
            var order = await _context.Orders.FindAsync(OrderId);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToPage();
            }

            var oldStatus = order.Status;
            order.Status = NewStatus;

            // Update timestamps based on status
            switch (NewStatus)
            {
                case OrderStatus.Shipped when !order.ShippedDate.HasValue:
                    order.ShippedDate = DateTime.Now;
                    break;
                case OrderStatus.Delivered when !order.DeliveredDate.HasValue:
                    order.DeliveredDate = DateTime.Now;
                    break;
            }

            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = $"Order #{order.OrderNumber} status updated from {oldStatus} to {NewStatus}.";
            return RedirectToPage();
        }
    }
}