using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using zellij.Services;

namespace zellij.Pages.Seller
{
    [Authorize(Roles = "Admin,Seller")]
    public class IndexModel : PageModel
    {
        private readonly IAdminService _adminService;

        public IndexModel(IAdminService adminService)
        {
            _adminService = adminService;
        }

        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int ShippedOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<zellij.Models.Order> RecentOrders { get; set; } = new();

        public async Task OnGetAsync()
        {
            var dashboardData = await _adminService.GetSellerDashboardDataAsync();

            TotalOrders = dashboardData.TotalOrders;
            PendingOrders = dashboardData.PendingOrders;
            ProcessingOrders = dashboardData.ProcessingOrders;
            ShippedOrders = dashboardData.ShippedOrders;
            DeliveredOrders = dashboardData.DeliveredOrders;
            TotalRevenue = dashboardData.TotalRevenue;
            RecentOrders = dashboardData.RecentOrders;
        }
    }
}