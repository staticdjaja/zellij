using zellij.Models;

namespace zellij.Repositories
{
    public interface IUserAddressRepository : IRepository<UserAddress>
    {
        Task<IEnumerable<UserAddress>> GetUserAddressesAsync(string userId);
        Task<UserAddress?> GetDefaultAddressAsync(string userId);
        Task<bool> SetDefaultAddressAsync(string userId, int addressId);
        Task<bool> IsAddressUsedInOrdersAsync(int addressId);
        Task<UserAddress?> GetUserAddressAsync(string userId, int addressId);
    }
}