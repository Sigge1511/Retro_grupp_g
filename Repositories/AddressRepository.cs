using Microsoft.EntityFrameworkCore;
using Retro_grupp_g.Data;
using Retro_grupp_g.Models;

namespace Retro_grupp_g.Repositories
{
    public class AddressRepository : IAddressRepository
    {

        private readonly SakilaDbContext _db;
        public AddressRepository(SakilaDbContext db) => _db = db;

        public async Task AddAsync(Address address)
        {
            await _db.Addresses.AddAsync(address);
        }

        public Task<bool> ExistsAsync(string address, string postalCode, string city, string district)
        {
            return _db.Addresses.AnyAsync(a => a.Address1 == address && a.PostalCode == postalCode && a.City.City1 == city && a.District == district);
        }

        public Task<List<Address>> GetAllAsync() =>
         _db.Addresses.OrderBy(a => a.Address1).ToListAsync();
        
        public Task<Address?> GetByIdAsync(int id) =>
            _db.Addresses.FindAsync(id).AsTask();


        public Task SaveAsync() => 
            _db.SaveChangesAsync();


        public Task UpdateAsync(Address address)
        {
            _db.Addresses.Update(address);
            return Task.CompletedTask;
        }
    }
}
