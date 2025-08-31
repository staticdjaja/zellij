using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using zellij.Services;

namespace zellij.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IAdminService _adminService;

        public IndexModel(IAdminService adminService)
        {
            _adminService = adminService;
        }

        public int ProductCount { get; set; }
        public int UserCount { get; set; }
        public int InStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }

        public async Task OnGetAsync()
        {
            var dashboardData = await _adminService.GetDashboardDataAsync();

            ProductCount = dashboardData.ProductCount;
            UserCount = dashboardData.UserCount;
            InStockProducts = dashboardData.InStockProducts;
            OutOfStockProducts = dashboardData.OutOfStockProducts;
        }
    }
}