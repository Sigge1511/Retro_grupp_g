using Microsoft.EntityFrameworkCore;
using Retro_grupp_g.Data;
using Retro_grupp_g.Models;

namespace Retro_grupp_g.Repositories
{
    public class AddressRepository : IAddressRepository
    {
        private readonly SakilaDbContext _db;
        public AddressRepository(SakilaDbContext db) => _db = db;

        public Task<List<Address>> GetAllAsync() =>
            _db.Addresses.OrderBy(a => a.Address1).ToListAsync();

        public Task<Address?> GetByIdAsync(int id) =>
            _db.Addresses.FindAsync(id).AsTask();

        public async Task AddAsync(Address address)
        {
            await _db.Addresses.AddAsync(address);
        }

        public Task UpdateAsync(Address address)
        {
            _db.Addresses.Update(address);
            return Task.CompletedTask;
        }

        public Task SaveAsync() => _db.SaveChangesAsync();

        public Task<bool> ExistsAsync(string address1, int cityId, string? postalCode, string? district)
        {
            return _db.Addresses.AnyAsync(a =>
                a.Address1 == address1 &&
                a.CityId == cityId &&
                a.PostalCode == postalCode &&   
                a.District == district          
            );
        }

    }
}
