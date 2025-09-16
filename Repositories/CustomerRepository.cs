using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Retro_grupp_g.Data;
using Retro_grupp_g.Models;

namespace Retro_grupp_g.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly SakilaDbContext _db;
        public CustomerRepository(SakilaDbContext db) => _db = db;

        public Task AddAsync(Customer customer) =>
            _db.Customers.AddAsync(customer).AsTask();

        public async Task DeleteAsync(int id)
        {
            var c = await _db.Customers.FindAsync((ushort)id);
            if (c != null) _db.Customers.Remove(c);
        }

        public Task<List<Customer>> GetAllAsync() =>
            _db.Customers
               .OrderBy(c => c.LastName)
               .ThenBy(c => c.FirstName)
               .ToListAsync();

        public Task<Customer?> GetByIdAsync(int id) =>
            _db.Customers.FindAsync((ushort)id).AsTask();

        public Task SaveAsync() => _db.SaveChangesAsync();

        public Task UpdateAsync(Customer customer)
        {
            _db.Customers.Update(customer);
            return Task.CompletedTask;
        }
        public async Task<Customer?> GetDetailsAsync(ushort id)
        {
            return await _db.Customers
                .Include(c => c.Rentals)
                    .ThenInclude(r => r.Inventory)
                        .ThenInclude(i => i.Film)
                .FirstOrDefaultAsync(c => c.CustomerId == id);
        }
    }
}
