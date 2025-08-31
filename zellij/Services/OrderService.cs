using Microsoft.EntityFrameworkCore;
using zellij.Data;
using zellij.Models;

namespace zellij.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICartService _cartService;
        private readonly ICouponService _couponService;
        private readonly ILogger<OrderService> _logger;

        public OrderService(ApplicationDbContext context, ICartService cartService, ICouponService couponService, ILogger<OrderService> logger)
        {
            _context = context;
            _cartService = cartService;
            _couponService = couponService;
            _logger = logger;
        }

        public async Task<Order?> CreateOrderAsync(string userId, int shippingAddressId, int? billingAddressId = null, string? couponCode = null, string? notes = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Get cart items
                var cartItems = await _cartService.GetCartItemsAsync(userId);
                if (!cartItems.Any())
                {
                    _logger.LogWarning("Attempted to create order with empty cart for user {UserId}", userId);
                    return null;
                }

                // Verify addresses exist and belong to user
                var shippingAddress = await _context.UserAddresses
                    .FirstOrDefaultAsync(ua => ua.Id == shippingAddressId && ua.UserId == userId);
                
                if (shippingAddress == null)
                {
                    _logger.LogWarning("Invalid shipping address {AddressId} for user {UserId}", shippingAddressId, userId);
                    return null;
                }

                UserAddress? billingAddress = null;
                if (billingAddressId.HasValue)
                {
                    billingAddress = await _context.UserAddresses
                        .FirstOrDefaultAsync(ua => ua.Id == billingAddressId.Value && ua.UserId == userId);
                }

                // Calculate order totals
                var subTotal = cartItems.Sum(ci => ci.Total);
                var tax = subTotal * 0.10m; // 10% tax
                var shippingCost = subTotal >= 500 ? 0 : 25; // Free shipping over $500
                var discountAmount = 0m;
                
                Coupon? appliedCoupon = null;
                if (!string.IsNullOrEmpty(couponCode))
                {
                    appliedCoupon = await _couponService.GetCouponByCodeAsync(couponCode);
                    if (appliedCoupon != null && await _couponService.CanUserUseCouponAsync(userId, couponCode))
                    {
                        discountAmount = await _couponService.CalculateDiscountAsync(appliedCoupon, subTotal);
                    }
                    else
                    {
                        appliedCoupon = null; // Invalid coupon
                    }
                }

                var total = subTotal + tax + shippingCost - discountAmount;

                // Generate order number
                var orderNumber = await GenerateOrderNumberAsync();

                // Create order
                var order = new Order
                {
                    UserId = userId,
                    OrderNumber = orderNumber,
                    Status = OrderStatus.Pending,
                    SubTotal = subTotal,
                    DiscountAmount = discountAmount,
                    ShippingCost = shippingCost,
                    Tax = tax,
                    Total = total,
                    CouponId = appliedCoupon?.Id,
                    ShippingAddressId = shippingAddressId,
                    BillingAddressId = billingAddressId,
                    Notes = notes,
                    OrderDate = DateTime.Now
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Create order items
                foreach (var cartItem in cartItems)
                {
                    // Verify stock availability
                    var product = await _context.Products.FindAsync(cartItem.ProductId);
                    if (product == null || product.StockQuantity < cartItem.Quantity)
                    {
                        _logger.LogWarning("Insufficient stock for product {ProductId} when creating order", cartItem.ProductId);
                        await transaction.RollbackAsync();
                        return null;
                    }

                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = cartItem.ProductId,
                        ProductName = cartItem.Product.Name,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.PriceAtTimeOfAdd,
                        Total = cartItem.Total,
                        ProductImageUrl = cartItem.Product.ImageUrl,
                        ProductDescription = cartItem.Product.Description
                    };

                    _context.OrderItems.Add(orderItem);

                    // Update product stock
                    product.StockQuantity -= cartItem.Quantity;
                }

                // Apply coupon if valid
                if (appliedCoupon != null && discountAmount > 0)
                {
                    await _couponService.ApplyCouponToOrderAsync(userId, couponCode!, order.Id, discountAmount);
                }

                // Clear the cart
                await _cartService.ClearCartAsync(userId);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Created order {OrderNumber} for user {UserId}", orderNumber, userId);
                return order;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating order for user {UserId}", userId);
                return null;
            }
        }

        public async Task<Order?> GetOrderAsync(int orderId, string userId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.ShippingAddress)
                .Include(o => o.BillingAddress)
                .Include(o => o.Coupon)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
        }

        public async Task<List<Order>> GetUserOrdersAsync(string userId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    return false;
                }

                order.Status = status;

                if (status == OrderStatus.Shipped && !order.ShippedDate.HasValue)
                {
                    order.ShippedDate = DateTime.Now;
                }
                else if (status == OrderStatus.Delivered && !order.DeliveredDate.HasValue)
                {
                    order.DeliveredDate = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated order {OrderId} status to {Status}", orderId, status);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderId} status", orderId);
                return false;
            }
        }

        public async Task<bool> CancelOrderAsync(int orderId, string userId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

                if (order == null || order.Status != OrderStatus.Pending)
                {
                    return false;
                }

                // Restore product stock
                foreach (var orderItem in order.OrderItems)
                {
                    var product = await _context.Products.FindAsync(orderItem.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += orderItem.Quantity;
                    }
                }

                order.Status = OrderStatus.Cancelled;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Cancelled order {OrderId} for user {UserId}", orderId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId} for user {UserId}", orderId, userId);
                return false;
            }
        }

        public async Task<string> GenerateOrderNumberAsync()
        {
            var date = DateTime.Now.ToString("yyyyMMdd");
            var lastOrder = await _context.Orders
                .Where(o => o.OrderNumber.StartsWith($"ZMM{date}"))
                .OrderByDescending(o => o.OrderNumber)
                .FirstOrDefaultAsync();

            var sequence = 1;
            if (lastOrder != null)
            {
                var lastSequence = lastOrder.OrderNumber.Substring(11); // ZMM + 8 digits date + sequence
                if (int.TryParse(lastSequence, out var parsed))
                {
                    sequence = parsed + 1;
                }
            }

            return $"ZMM{date}{sequence:D4}";
        }
    }
}