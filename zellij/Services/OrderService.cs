using zellij.Models;
using zellij.Repositories;

namespace zellij.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserAddressRepository _userAddressRepository;
        private readonly ICartService _cartService;
        private readonly ICouponService _couponService;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IUserAddressRepository userAddressRepository,
            ICartService cartService,
            ICouponService couponService,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _userAddressRepository = userAddressRepository;
            _cartService = cartService;
            _couponService = couponService;
            _logger = logger;
        }

        public async Task<Order?> CreateOrderAsync(string userId, int shippingAddressId, int? billingAddressId = null, string? couponCode = null, string? notes = null)
        {
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
                var shippingAddress = await _userAddressRepository.GetUserAddressAsync(userId, shippingAddressId);

                if (shippingAddress == null)
                {
                    _logger.LogWarning("Invalid shipping address {AddressId} for user {UserId}", shippingAddressId, userId);
                    return null;
                }

                UserAddress? billingAddress = null;
                if (billingAddressId.HasValue)
                {
                    billingAddress = await _userAddressRepository.GetUserAddressAsync(userId, billingAddressId.Value);
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
                    OrderDate = DateTime.UtcNow,
                    OrderItems = new List<OrderItem>()
                };

                // Create order items and verify stock
                foreach (var cartItem in cartItems)
                {
                    var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
                    if (product == null || product.StockQuantity < cartItem.Quantity)
                    {
                        _logger.LogWarning("Insufficient stock for product {ProductId} when creating order", cartItem.ProductId);
                        return null;
                    }

                    var orderItem = new OrderItem
                    {
                        ProductId = cartItem.ProductId,
                        ProductName = cartItem.Product.Name,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.PriceAtTimeOfAdd,
                        Total = cartItem.Total,
                        ProductImageUrl = cartItem.Product.ImageUrl,
                        ProductDescription = cartItem.Product.Description
                    };

                    order.OrderItems.Add(orderItem);

                    // Update product stock
                    product.StockQuantity -= cartItem.Quantity;
                    await _productRepository.UpdateAsync(product);
                }

                // Add order to repository
                var createdOrder = await _orderRepository.AddAsync(order);

                // Apply coupon if valid
                if (appliedCoupon != null && discountAmount > 0)
                {
                    await _couponService.ApplyCouponToOrderAsync(userId, couponCode!, createdOrder.Id, discountAmount);
                }

                // Clear the cart
                await _cartService.ClearCartAsync(userId);

                _logger.LogInformation("Created order {OrderNumber} for user {UserId}", orderNumber, userId);
                return createdOrder;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order for user {UserId}", userId);
                return null;
            }
        }

        public async Task<Order?> GetOrderAsync(int orderId, string userId)
        {
            return await _orderRepository.GetOrderWithDetailsAsync(orderId, userId);
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _orderRepository.GetOrderWithDetailsAsync(orderId);
        }

        public async Task<List<Order>> GetUserOrdersAsync(string userId)
        {
            return (await _orderRepository.GetUserOrdersAsync(userId)).ToList();
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status, string? trackingNumber = null)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    return false;
                }

                order.Status = status;

                if (status == OrderStatus.Shipped && !order.ShippedDate.HasValue)
                {
                    order.ShippedDate = DateTime.Now;
                    if (!string.IsNullOrEmpty(trackingNumber))
                    {
                        order.TrackingNumber = trackingNumber;
                    }
                }
                else if (status == OrderStatus.Delivered && !order.DeliveredDate.HasValue)
                {
                    order.DeliveredDate = DateTime.Now;
                }

                await _orderRepository.UpdateAsync(order);
                _logger.LogInformation("Updated order {OrderId} status to {Status}", orderId, status);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderId} status", orderId);
                return false;
            }
        }
        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            return await UpdateOrderStatusAsync(orderId, status, null);
        }

        public async Task<bool> CancelOrderAsync(int orderId, string userId)
        {
            try
            {
                var order = await _orderRepository.GetOrderWithDetailsAsync(orderId, userId);

                if (order == null || order.Status != OrderStatus.Pending)
                {
                    return false;
                }

                // Restore product stock
                foreach (var orderItem in order.OrderItems)
                {
                    var product = await _productRepository.GetByIdAsync(orderItem.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += orderItem.Quantity;
                        await _productRepository.UpdateAsync(product);
                    }
                }

                order.Status = OrderStatus.Cancelled;
                await _orderRepository.UpdateAsync(order);

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
            var orderNumberPrefix = $"ZMM{date}";

            var existingOrders = await _orderRepository.FindAsync(o => o.OrderNumber.StartsWith(orderNumberPrefix));
            var lastOrder = existingOrders.OrderByDescending(o => o.OrderNumber).FirstOrDefault();

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

        // New methods from IOrderService interface
        public async Task<IEnumerable<Order>> GetOrdersWithDetailsAsync()
        {
            return await _orderRepository.GetOrdersWithDetailsAsync();
        }

        public async Task<IEnumerable<Order>> GetRecentOrdersAsync(int count)
        {
            return await _orderRepository.GetRecentOrdersAsync(count);
        }

        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status)
        {
            return await _orderRepository.GetOrdersByStatusAsync(status);
        }

        public async Task<int> GetOrderCountByStatusAsync(OrderStatus status)
        {
            return await _orderRepository.GetOrderCountByStatusAsync(status);
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _orderRepository.GetTotalRevenueAsync();
        }

        public async Task<decimal> GetRevenueByStatusAsync(OrderStatus status)
        {
            return await _orderRepository.GetRevenueByStatusAsync(status);
        }

        public async Task<IEnumerable<Order>> SearchOrdersAsync(string? searchString = null, OrderStatus? status = null)
        {
            return await _orderRepository.SearchOrdersAsync(searchString, status);
        }
    }
}