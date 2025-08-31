using Microsoft.EntityFrameworkCore;
using zellij.Data;
using zellij.Models;

namespace zellij.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(string userId)
        {
            return await _dbSet.Include(o => o.OrderItems)
                              .Include(o => o.ShippingAddress)
                              .Include(o => o.Coupon)
                              .Where(o => o.UserId == userId)
                              .OrderByDescending(o => o.OrderDate)
                              .ToListAsync();
        }

        public async Task<Order?> GetOrderWithDetailsAsync(int orderId, string userId)
        {
            return await _dbSet.Include(o => o.OrderItems)
                                  .ThenInclude(oi => oi.Product)
                              .Include(o => o.ShippingAddress)
                              .Include(o => o.BillingAddress)
                              .Include(o => o.Coupon)
                              .Include(o => o.User)
                              .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
        }

        public async Task<Order?> GetOrderWithDetailsAsync(int orderId)
        {
            return await _dbSet.Include(o => o.OrderItems)
                                  .ThenInclude(oi => oi.Product)
                              .Include(o => o.ShippingAddress)
                              .Include(o => o.BillingAddress)
                              .Include(o => o.Coupon)
                              .Include(o => o.User)
                              .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<IEnumerable<Order>> GetOrdersWithDetailsAsync()
        {
            return await _dbSet.Include(o => o.User)
                              .Include(o => o.ShippingAddress)
                              .Include(o => o.OrderItems)
                              .OrderByDescending(o => o.OrderDate)
                              .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetRecentOrdersAsync(int count)
        {
            return await _dbSet.Include(o => o.User)
                              .Include(o => o.ShippingAddress)
                              .OrderByDescending(o => o.OrderDate)
                              .Take(count)
                              .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status)
        {
            return await _dbSet.Include(o => o.User)
                              .Include(o => o.ShippingAddress)
                              .Where(o => o.Status == status)
                              .OrderByDescending(o => o.OrderDate)
                              .ToListAsync();
        }

        public async Task<int> GetOrderCountByStatusAsync(OrderStatus status)
        {
            return await _dbSet.CountAsync(o => o.Status == status);
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _dbSet.Where(o => o.Status == OrderStatus.Delivered)
                              .SumAsync(o => o.Total);
        }

        public async Task<decimal> GetRevenueByStatusAsync(OrderStatus status)
        {
            return await _dbSet.Where(o => o.Status == status)
                              .SumAsync(o => o.Total);
        }

        public async Task<IEnumerable<Order>> SearchOrdersAsync(string? searchString = null, OrderStatus? status = null)
        {
            var query = _dbSet.Include(o => o.User)
                             .Include(o => o.ShippingAddress)
                             .Include(o => o.OrderItems)
                             .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(o =>
                    o.OrderNumber.Contains(searchString) ||
                    o.User.UserName!.Contains(searchString) ||
                    o.User.Email!.Contains(searchString));
            }

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            return await query.OrderByDescending(o => o.OrderDate).ToListAsync();
        }
    }
}