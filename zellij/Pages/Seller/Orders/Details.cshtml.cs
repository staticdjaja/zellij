using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using zellij.Models;
using zellij.Services;

namespace zellij.Pages.Seller.Orders
{
    [Authorize(Roles = "Admin,Seller")]
    public class DetailsModel : PageModel
    {
        private readonly IOrderService _orderService;

        public DetailsModel(IOrderService orderService)
        {
            _orderService = orderService;
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

            var order = await _orderService.GetOrderByIdAsync(id.Value);

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
            var success = await _orderService.UpdateOrderStatusAsync(id, NewStatus, TrackingNumber);

            if (success)
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order != null)
                {
                    TempData["SuccessMessage"] = $"Order #{order.OrderNumber} status updated to {NewStatus}.";
                }
                else
                {
                    TempData["SuccessMessage"] = "Order status updated successfully.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Order not found or could not be updated.";
            }

            return RedirectToPage(new { id });
        }
    }
}