using zellij.Models;

namespace zellij.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IEnumerable<Order>> GetUserOrdersAsync(string userId);
        Task<Order?> GetOrderWithDetailsAsync(int orderId, string userId);
        Task<Order?> GetOrderWithDetailsAsync(int orderId);
        Task<IEnumerable<Order>> GetOrdersWithDetailsAsync();
        Task<IEnumerable<Order>> GetRecentOrdersAsync(int count);
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);
        Task<int> GetOrderCountByStatusAsync(OrderStatus status);
        Task<decimal> GetTotalRevenueAsync();
        Task<decimal> GetRevenueByStatusAsync(OrderStatus status);
        Task<IEnumerable<Order>> SearchOrdersAsync(string? searchString = null, OrderStatus? status = null);
    }
}