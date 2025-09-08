using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using zellij.Models;
using zellij.Services;

namespace zellij.Pages.Seller.Orders
{
    [Authorize(Roles = "Admin,Seller")]
    public class IndexModel : PageModel
    {
        private readonly IOrderService _orderService;

        public IndexModel(IOrderService orderService)
        {
            _orderService = orderService;
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

            Orders = (await _orderService.SearchOrdersAsync(searchString, StatusFilter)).ToList();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync()
        {
            var success = await _orderService.UpdateOrderStatusAsync(OrderId, NewStatus);

            if (success)
            {
                var order = await _orderService.GetOrderByIdAsync(OrderId);
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

            return RedirectToPage();
        }
    }
}