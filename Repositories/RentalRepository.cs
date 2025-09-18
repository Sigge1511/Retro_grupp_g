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
        
        
//*********************** RETURER ****************************************************************
        //GET

        public async Task<(IReadOnlyList<(int InventoryId, int FilmId, 
                                        string Title)> Films,
                           IReadOnlyList<(int CustomerId, string FullName, 
                                        string Email)> Customers)>OnGetReturnAsync()
        {
            var filmsRaw = await _db.Inventories
                .AsNoTracking()
                .Where(i => i.Rentals.Any(r => r.ReturnDate == null))   // ⬅️ bara uthyrda just nu
                .Select(i => new
                {
                    InventoryId = (int)i.InventoryId, // uint -> int för ViewData/Bind
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

        //------
        public async Task<(bool Found, string FilmTitle, DateOnly RentalDate, DateOnly DueDate,
                   bool IsLate, int DaysLate, int RentalDurationDays)>OnGetReturnPreviewAsync
                   (int inventoryId, int customerId)
        {
            var inv = (uint)inventoryId;
            var cid = (ushort)customerId;

            // Ladda inventory + film (för att få titel & RentalDuration även i simulerat läge)
            var inventory = await _db.Inventories
                .AsNoTracking()
                .Include(i => i.Film)
                .FirstOrDefaultAsync(i => i.InventoryId == inv);

            if (inventory is null || inventory.Film is null)
                return (false, "", default, default, false, 0, 0);

            var film = inventory.Film;
            var duration = Convert.ToInt32(film.RentalDuration);

            var realRental = await _db.Rentals
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.InventoryId == inv &&
                                          r.CustomerId == cid &&
                                          r.ReturnDate == null);

            if (realRental is not null)
            {
                var rentDt = realRental.RentalDate;
                if (rentDt.Kind == DateTimeKind.Unspecified)
                    rentDt = DateTime.SpecifyKind(rentDt, DateTimeKind.Utc);

                var rentalDay = DateOnly.FromDateTime(rentDt.ToLocalTime());
                var dueDay = rentalDay.AddDays(duration);
                var today = DateOnly.FromDateTime(DateTime.Now);
                var isLate = today > dueDay;
                var daysLate = isLate ? today.DayNumber - dueDay.DayNumber : 0;

                return (true, film.Title, rentalDay, dueDay, isLate, daysLate, duration);
            }

            // --- SIMULERAT LÄGE (ingen öppen rental för kombon) ---
            var simRentalDay = DateOnly.FromDateTime(DateTime.Now);        // “hyrd idag”
            var simDue = simRentalDay.AddDays(duration);
            return (true, film.Title, simRentalDay, simDue, false, 0, duration);
        }

        //************************************** RETURER POST
        public async Task ReturnNormalAsync(int rentalId)
        {
            var rental = await _db.Rentals
                .FirstOrDefaultAsync(r => r.RentalId == rentalId && r.ReturnDate == null);
            if (rental is null) return; // redan returnerad eller fel id

            rental.ReturnDate = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        public Task ReturnLateAsync(int rentalId) => Task.CompletedTask;
        public Task ReturnDamagedAsync(int rentalId) => Task.CompletedTask;
    }
}