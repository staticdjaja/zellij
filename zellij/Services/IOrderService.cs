using zellij.Models;

namespace zellij.Services
{
    public interface IOrderService
    {
        Task<Order?> CreateOrderAsync(string userId, int shippingAddressId, int? billingAddressId = null, string? couponCode = null, string? notes = null);
        Task<Order?> GetOrderAsync(int orderId, string userId);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<List<Order>> GetUserOrdersAsync(string userId);
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status);
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status, string? trackingNumber = null);
        Task<bool> CancelOrderAsync(int orderId, string userId);
        Task<string> GenerateOrderNumberAsync();
        Task<IEnumerable<Order>> GetOrdersWithDetailsAsync();
        Task<IEnumerable<Order>> GetRecentOrdersAsync(int count);
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);
        Task<int> GetOrderCountByStatusAsync(OrderStatus status);
        Task<decimal> GetTotalRevenueAsync();
        Task<decimal> GetRevenueByStatusAsync(OrderStatus status);
        Task<IEnumerable<Order>> SearchOrdersAsync(string? searchString = null, OrderStatus? status = null);
    }
}