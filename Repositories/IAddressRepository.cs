using Retro_grupp_g.Models;

namespace Retro_grupp_g.Repositories
{
    public interface IAddressRepository
    {
        Task<List<Address>> GetAllAsync();
        Task<Address?> GetByIdAsync(int id);
        Task AddAsync(Address address);
        Task UpdateAsync(Address address);
        Task SaveAsync();
        Task<bool> ExistsAsync(string address1, int cityId, string? postalCode, string? district);
        Task DeleteAsync(int id);
        Task<bool> CanDeleteAsync(int id);
    }
}

