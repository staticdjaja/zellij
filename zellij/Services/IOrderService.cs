using zellij.Models;

namespace zellij.Services
{
    public interface IOrderService
    {
        Task<Order?> CreateOrderAsync(string userId, int shippingAddressId, int? billingAddressId = null, string? couponCode = null, string? notes = null);
        Task<Order?> GetOrderAsync(int orderId, string userId);
        Task<List<Order>> GetUserOrdersAsync(string userId);
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status);
        Task<bool> CancelOrderAsync(int orderId, string userId);
        Task<string> GenerateOrderNumberAsync();
    }
}