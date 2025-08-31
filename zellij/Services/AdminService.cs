using Microsoft.AspNetCore.Identity;
using zellij.Models;
using zellij.Repositories;

namespace zellij.Services
{
    public class AdminService : IAdminService
    {
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminService(
            IProductRepository productRepository,
            IOrderRepository orderRepository,
            UserManager<IdentityUser> userManager)
        {
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _userManager = userManager;
        }

        public async Task<AdminDashboardModel> GetDashboardDataAsync()
        {
            var productCount = await _productRepository.CountAsync();
            var userCount = _userManager.Users.Count();
            var inStockProducts = await _productRepository.CountAsync(p => p.InStock && p.StockQuantity > 0);
            var outOfStockProducts = await _productRepository.CountAsync(p => !p.InStock || p.StockQuantity == 0);
            var totalOrders = await _orderRepository.CountAsync();
            var totalRevenue = await _orderRepository.GetTotalRevenueAsync();

            return new AdminDashboardModel
            {
                ProductCount = productCount,
                UserCount = userCount,
                InStockProducts = inStockProducts,
                OutOfStockProducts = outOfStockProducts,
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue
            };
        }

        public async Task<SellerDashboardModel> GetSellerDashboardDataAsync()
        {
            var totalOrders = await _orderRepository.CountAsync();
            var pendingOrders = await _orderRepository.GetOrderCountByStatusAsync(OrderStatus.Pending);
            var processingOrders = await _orderRepository.GetOrderCountByStatusAsync(OrderStatus.Processing);
            var shippedOrders = await _orderRepository.GetOrderCountByStatusAsync(OrderStatus.Shipped);
            var deliveredOrders = await _orderRepository.GetOrderCountByStatusAsync(OrderStatus.Delivered);
            var totalRevenue = await _orderRepository.GetRevenueByStatusAsync(OrderStatus.Delivered);
            var recentOrders = await _orderRepository.GetRecentOrdersAsync(10);

            return new SellerDashboardModel
            {
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                ProcessingOrders = processingOrders,
                ShippedOrders = shippedOrders,
                DeliveredOrders = deliveredOrders,
                TotalRevenue = totalRevenue,
                RecentOrders = recentOrders.ToList()
            };
        }

        public int GetUserCount()
        {
            return _userManager.Users.Count();
        }
    }
}