using Microsoft.EntityFrameworkCore;
using zellij.Data;
using zellij.Models;

namespace zellij.Repositories
{
    public class UserAddressRepository : Repository<UserAddress>, IUserAddressRepository
    {
        public UserAddressRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<UserAddress>> GetUserAddressesAsync(string userId)
        {
            return await _dbSet.Where(ua => ua.UserId == userId)
                              .OrderByDescending(ua => ua.IsDefault)
                              .ThenByDescending(ua => ua.CreatedDate)
                              .ToListAsync();
        }

        public async Task<UserAddress?> GetDefaultAddressAsync(string userId)
        {
            return await _dbSet.FirstOrDefaultAsync(ua => ua.UserId == userId && ua.IsDefault);
        }

        public async Task<bool> SetDefaultAddressAsync(string userId, int addressId)
        {
            // Set IsDefault = false for all addresses of the user
            await _dbSet.Where(ua => ua.UserId == userId && ua.IsDefault)
                       .ExecuteUpdateAsync(setters => setters.SetProperty(ua => ua.IsDefault, false));

            // Set IsDefault = true for the selected address
            var result = await _dbSet.Where(ua => ua.UserId == userId && ua.Id == addressId)
                                   .ExecuteUpdateAsync(setters => setters.SetProperty(ua => ua.IsDefault, true));

            return result > 0;
        }

        public async Task<bool> IsAddressUsedInOrdersAsync(int addressId)
        {
            return await _context.Orders
                .AnyAsync(o => o.ShippingAddressId == addressId || o.BillingAddressId == addressId);
        }

        public async Task<UserAddress?> GetUserAddressAsync(string userId, int addressId)
        {
            return await _dbSet.FirstOrDefaultAsync(ua => ua.Id == addressId && ua.UserId == userId);
        }
    }
}