using Microsoft.EntityFrameworkCore;
using zellij.Data;
using zellij.Models;

namespace zellij.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CartService> _logger;

        public CartService(ApplicationDbContext context, ILogger<CartService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<CartItem>> GetCartItemsAsync(string userId)
        {
            return await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.UserId == userId)
                .OrderBy(ci => ci.AddedDate)
                .ToListAsync();
        }

        public async Task<CartItem?> GetCartItemAsync(string userId, int productId)
        {
            return await _context.CartItems
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);
        }

        public async Task<bool> AddToCartAsync(string userId, int productId, int quantity = 1)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null || !product.InStock || product.StockQuantity < quantity)
                {
                    return false;
                }

                var existingCartItem = await GetCartItemAsync(userId, productId);

                if (existingCartItem != null)
                {
                    // Update existing cart item
                    var newQuantity = existingCartItem.Quantity + quantity;
                    if (newQuantity > product.StockQuantity)
                    {
                        return false;
                    }

                    existingCartItem.Quantity = newQuantity;
                    existingCartItem.AddedDate = DateTime.Now;
                }
                else
                {
                    // Add new cart item
                    var cartItem = new CartItem
                    {
                        UserId = userId,
                        ProductId = productId,
                        Quantity = quantity,
                        PriceAtTimeOfAdd = product.Price,
                        AddedDate = DateTime.Now
                    };

                    _context.CartItems.Add(cartItem);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Added {Quantity} of product {ProductId} to cart for user {UserId}", quantity, productId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product {ProductId} to cart for user {UserId}", productId, userId);
                return false;
            }
        }

        public async Task<bool> UpdateCartItemAsync(string userId, int productId, int quantity)
        {
            try
            {
                var cartItem = await GetCartItemAsync(userId, productId);
                if (cartItem == null)
                {
                    return false;
                }

                if (quantity <= 0)
                {
                    return await RemoveFromCartAsync(userId, productId);
                }

                // Check stock availability
                if (quantity > cartItem.Product.StockQuantity)
                {
                    return false;
                }

                cartItem.Quantity = quantity;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated cart item quantity to {Quantity} for product {ProductId} and user {UserId}", quantity, productId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item for product {ProductId} and user {UserId}", productId, userId);
                return false;
            }
        }

        public async Task<bool> RemoveFromCartAsync(string userId, int productId)
        {
            try
            {
                var cartItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);

                if (cartItem == null)
                {
                    return false;
                }

                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Removed product {ProductId} from cart for user {UserId}", productId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing product {ProductId} from cart for user {UserId}", productId, userId);
                return false;
            }
        }

        public async Task<bool> ClearCartAsync(string userId)
        {
            try
            {
                var cartItems = await _context.CartItems
                    .Where(ci => ci.UserId == userId)
                    .ToListAsync();

                _context.CartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Cleared cart for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for user {UserId}", userId);
                return false;
            }
        }

        public async Task<int> GetCartItemCountAsync(string userId)
        {
            return await _context.CartItems
                .Where(ci => ci.UserId == userId)
                .SumAsync(ci => ci.Quantity);
        }

        public async Task<decimal> GetCartTotalAsync(string userId)
        {
            return await _context.CartItems
                .Where(ci => ci.UserId == userId)
                .SumAsync(ci => ci.PriceAtTimeOfAdd * ci.Quantity);
        }

        public async Task<CartSummary> GetCartSummaryAsync(string userId)
        {
            var cartItems = await GetCartItemsAsync(userId);
            var subTotal = cartItems.Sum(ci => ci.Total);

            // Calculate tax (example: 10% tax rate)
            var tax = subTotal * 0.10m;

            // Calculate shipping (example: free shipping over $500, otherwise $25)
            var shippingCost = subTotal >= 500 ? 0 : 25;

            var total = subTotal + tax + shippingCost;

            return new CartSummary
            {
                Items = cartItems,
                TotalItems = cartItems.Sum(ci => ci.Quantity),
                SubTotal = subTotal,
                Tax = tax,
                ShippingCost = shippingCost,
                Total = total
            };
        }
    }
}