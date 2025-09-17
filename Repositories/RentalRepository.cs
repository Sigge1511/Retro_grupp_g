using Microsoft.EntityFrameworkCore;
using Retro_grupp_g.Data;
using Retro_grupp_g.Models;

namespace Retro_grupp_g.Repositories
{
    public class RentalRepository : IRentalRepository
    {
        private readonly SakilaDbContext _db;
        public RentalRepository(SakilaDbContext db) => _db = db;


        public Task<List<Rental>> GetOpenRentalsByCustomerAsync(int customerId) =>
            _db.Rentals
            .Include(r => r.Inventory)
            .ThenInclude(i => i.Film)
            .Where(r => r.CustomerId == customerId && r.ReturnDate == null)
            .OrderByDescending(r => r.RentalDate)
            .ToListAsync();


        public async Task<bool> RentAsync(int customerId, int filmId, int? staffId = null)
        {
            var availableInventoryId = await _db.Inventories
                .Where(i => i.FilmId == filmId)
                .Where(i => !_db.Rentals.Any(r => r.InventoryId == i.InventoryId && r.ReturnDate == null))
                .Select(i => (uint?)i.InventoryId)
                .FirstOrDefaultAsync();

            if (availableInventoryId is null)
                return false;

            byte chosenStaffId = staffId.HasValue
                ? (byte)staffId.Value
                : (byte)await _db.Staff.Select(s => s.StaffId).FirstAsync();

            var rental = new Rental
            {
                InventoryId = availableInventoryId.Value,
                CustomerId = customerId,                 
                StaffId = chosenStaffId,
                RentalDate = DateTime.UtcNow,
                ReturnDate = null
            };


            await _db.Rentals.AddAsync(rental);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task ReturnAsync(int rentalId)
        {
            var rental = await _db.Rentals.FindAsync(rentalId);
            if (rental == null) return; 

            if (rental.ReturnDate == null)
            {
                rental.ReturnDate = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }
    }
}
