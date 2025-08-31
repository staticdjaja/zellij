using zellij.Models;

namespace zellij.Services
{
    public interface IAdminService
    {
        Task<AdminDashboardModel> GetDashboardDataAsync();
        Task<SellerDashboardModel> GetSellerDashboardDataAsync();
        int GetUserCount();
    }

    public class AdminDashboardModel
    {
        public int ProductCount { get; set; }
        public int UserCount { get; set; }
        public int InStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class SellerDashboardModel
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int ShippedOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<Order> RecentOrders { get; set; } = new();
    }
}