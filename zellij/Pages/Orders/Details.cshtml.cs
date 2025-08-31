using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using zellij.Models;
using zellij.Services;

namespace zellij.Pages.Orders
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IOrderService _orderService;

        public DetailsModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public Order Order { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var order = await _orderService.GetOrderAsync(id.Value, userId);

            if (order == null)
            {
                return NotFound();
            }

            Order = order;
            return Page();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var success = await _orderService.CancelOrderAsync(id, userId);

            if (success)
            {
                TempData["SuccessMessage"] = "Order cancelled successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Unable to cancel order. Only pending orders can be cancelled.";
            }

            return RedirectToPage(new { id });
        }
    }
}