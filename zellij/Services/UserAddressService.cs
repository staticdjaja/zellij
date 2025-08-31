using zellij.Models;
using zellij.Repositories;

namespace zellij.Services
{
    public class UserAddressService : IUserAddressService
    {
        private readonly IUserAddressRepository _userAddressRepository;

        public UserAddressService(IUserAddressRepository userAddressRepository)
        {
            _userAddressRepository = userAddressRepository;
        }

        public async Task<IEnumerable<UserAddress>> GetUserAddressesAsync(string userId)
        {
            return await _userAddressRepository.GetUserAddressesAsync(userId);
        }

        public async Task<UserAddress?> GetUserAddressAsync(string userId, int addressId)
        {
            return await _userAddressRepository.GetUserAddressAsync(userId, addressId);
        }

        public async Task<UserAddress?> GetDefaultAddressAsync(string userId)
        {
            return await _userAddressRepository.GetDefaultAddressAsync(userId);
        }

        public async Task<UserAddress> CreateAddressAsync(UserAddress address)
        {
            address.CreatedDate = DateTime.Now;

            // Check if this should be the default address (if no other addresses exist)
            var existingAddresses = await _userAddressRepository.GetUserAddressesAsync(address.UserId);
            if (!existingAddresses.Any())
            {
                address.IsDefault = true;
            }

            return await _userAddressRepository.AddAsync(address);
        }

        public async Task<UserAddress> UpdateAddressAsync(UserAddress address)
        {
            return await _userAddressRepository.UpdateAsync(address);
        }

        public async Task<bool> DeleteAddressAsync(string userId, int addressId)
        {
            var address = await _userAddressRepository.GetUserAddressAsync(userId, addressId);
            if (address == null) return false;

            // Check if this address is used in any orders
            if (await _userAddressRepository.IsAddressUsedInOrdersAsync(addressId))
            {
                return false; // Cannot delete address used in orders
            }

            return await _userAddressRepository.DeleteAsync(address);
        }

        public async Task<bool> SetDefaultAddressAsync(string userId, int addressId)
        {
            return await _userAddressRepository.SetDefaultAddressAsync(userId, addressId);
        }

        public async Task<bool> IsAddressUsedInOrdersAsync(int addressId)
        {
            return await _userAddressRepository.IsAddressUsedInOrdersAsync(addressId);
        }

        public async Task<bool> HasAddressesAsync(string userId)
        {
            var addresses = await _userAddressRepository.GetUserAddressesAsync(userId);
            return addresses.Any();
        }
    }
}