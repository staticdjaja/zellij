using zellij.Models;

namespace zellij.Services
{
    public interface ICartService
    {
        Task<List<CartItem>> GetCartItemsAsync(string userId);
        Task<CartItem?> GetCartItemAsync(string userId, int productId);
        Task<bool> AddToCartAsync(string userId, int productId, int quantity = 1);
        Task<bool> UpdateCartItemAsync(string userId, int productId, int quantity);
        Task<bool> RemoveFromCartAsync(string userId, int productId);
        Task<bool> ClearCartAsync(string userId);
        Task<int> GetCartItemCountAsync(string userId);
        Task<decimal> GetCartTotalAsync(string userId);
        Task<CartSummary> GetCartSummaryAsync(string userId);
    }

    public class CartSummary
    {
        public List<CartItem> Items { get; set; } = new();
        public int TotalItems { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Tax { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Total { get; set; }
        public Coupon? AppliedCoupon { get; set; }
    }
}