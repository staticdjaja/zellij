using zellij.Models;

namespace zellij.Services
{
    public interface IUserAddressService
    {
        Task<IEnumerable<UserAddress>> GetUserAddressesAsync(string userId);
        Task<UserAddress?> GetUserAddressAsync(string userId, int addressId);
        Task<UserAddress?> GetDefaultAddressAsync(string userId);
        Task<UserAddress> CreateAddressAsync(UserAddress address);
        Task<UserAddress> UpdateAddressAsync(UserAddress address);
        Task<bool> DeleteAddressAsync(string userId, int addressId);
        Task<bool> SetDefaultAddressAsync(string userId, int addressId);
        Task<bool> IsAddressUsedInOrdersAsync(int addressId);
        Task<bool> HasAddressesAsync(string userId);
    }
}