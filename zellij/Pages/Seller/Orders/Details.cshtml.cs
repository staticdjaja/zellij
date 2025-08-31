using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using zellij.Data;
using zellij.Models;

namespace zellij.Pages.Seller.Orders
{
    [Authorize(Roles = "Admin,Seller")]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Order Order { get; set; } = default!;

        [BindProperty]
        public OrderStatus NewStatus { get; set; }

        [BindProperty]
        public string? TrackingNumber { get; set; }

        [BindProperty]
        public string? AdminNotes { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.ShippingAddress)
                .Include(o => o.BillingAddress)
                .Include(o => o.Coupon)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            Order = order;
            NewStatus = order.Status;
            TrackingNumber = order.TrackingNumber;
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            var oldStatus = order.Status;
            order.Status = NewStatus;

            // Update timestamps based on status
            switch (NewStatus)
            {
                case OrderStatus.Shipped when !order.ShippedDate.HasValue:
                    order.ShippedDate = DateTime.Now;
                    if (!string.IsNullOrEmpty(TrackingNumber))
                    {
                        order.TrackingNumber = TrackingNumber;
                    }
                    break;
                case OrderStatus.Delivered when !order.DeliveredDate.HasValue:
                    order.DeliveredDate = DateTime.Now;
                    break;
            }

            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = $"Order status updated from {oldStatus} to {NewStatus}.";
            return RedirectToPage(new { id });
        }
    }
}