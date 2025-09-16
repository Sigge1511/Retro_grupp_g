using Retro_grupp_g.Models;

namespace Retro_grupp_g.Repositories
{
    public interface ICustomerRepository
    {
        Task<List<Customer>> GetAllAsync();
        Task<Customer?> GetByIdAsync (int id);
        Task AddAsync (Customer customer);
        Task UpdateAsync (Customer customer);
        Task DeleteAsync (int id);
        Task SaveAsync();
        Task<Customer?> GetDetailsAsync(ushort id);


    }
}
