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
               .AsNoTracking()
               .Include(r => r.Inventory)
               .Where(r => r.CustomerId == (ushort)customerId && r.ReturnDate == null)
               .OrderByDescending(r => r.RentalDate)
               .ToListAsync();

        public async Task<bool> RentAsync(int customerId, int filmId, int? staffId = null)
        {
            var availableInventoryId = await _db.Inventories
                .Where(i => i.FilmId == filmId)
                .Where(i => !_db.Rentals.Any(r => r.InventoryId == i.InventoryId && r.ReturnDate == null))
                .Select(i => (uint?)i.InventoryId)
                .FirstOrDefaultAsync();

            if (availableInventoryId is null) return false;

            byte chosenStaffId = staffId.HasValue
                ? (byte)staffId.Value
                : (byte)await _db.Staff.Select(s => s.StaffId).FirstAsync();

            var rental = new Rental
            {
                InventoryId = availableInventoryId.Value,
                CustomerId = (ushort)customerId,
                StaffId = chosenStaffId,
                RentalDate = DateTime.UtcNow,
                ReturnDate = null
            };

            await _db.Rentals.AddAsync(rental);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<(IReadOnlyList<(int InventoryId, int FilmId, 
                                        string Title)> Films,
                           IReadOnlyList<(int CustomerId, string FullName, 
                                        string Email)> Customers)>OnGetReturnAsync()
        {
            var filmsRaw = await _db.Inventories
                .AsNoTracking()
                .Select(i => new
                {
                    InventoryId = (int)i.InventoryId, // uint -> int för enkelhet i ViewData
                    i.FilmId,
                    Title = i.Film.Title
                })
                .OrderBy(x => x.Title)
                .ToListAsync();

            var customersRaw = await _db.Customers
                .AsNoTracking()
                .Select(c => new
                {
                    c.CustomerId,
                    FullName = c.FirstName + " " + c.LastName,
                    c.Email
                })
                .OrderBy(x => x.FullName)
                .ToListAsync();

            return (
                filmsRaw.Select(x => (x.InventoryId, x.FilmId, x.Title)).ToList(),
                customersRaw.Select(x => (x.CustomerId, x.FullName, x.Email)).ToList()
            );
        }

        public Task ReturnNormalAsync(int rentalId) => Task.CompletedTask; // implementeras senare
        public Task ReturnLateAsync(int rentalId) => Task.CompletedTask;
        public Task ReturnDamagedAsync(int rentalId) => Task.CompletedTask;
    }
}