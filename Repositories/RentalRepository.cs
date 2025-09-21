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

        public async Task<(bool Found, string FilmTitle, DateOnly RentalDate, DateOnly DueDate,
                   bool IsLate, int DaysLate, int RentalDurationDays, int ActualCustomerId, 
            string ActualCustomerName, bool CustomerMatches)>OnGetReturnPreviewAsync(int inventoryId, int customerId)
        {
            var inv = (uint)inventoryId;
            var cid = (ushort)customerId;

            var row = await _db.Rentals
                .AsNoTracking()
                .Where(r => r.InventoryId == inv && r.ReturnDate == null)
                .Select(r => new
                {
                    r.RentalDate,
                    FilmTitle = r.Inventory.Film.Title,
                    RentalDuration = r.Inventory.Film.RentalDuration,
                    ActualCustomerId = r.CustomerId,
                    FirstName = r.Customer.FirstName,
                    LastName = r.Customer.LastName
                })
                .FirstOrDefaultAsync();

            if (row is null)
                return (false, "", default, default, false, 0, 0, 0, "", false);

            int duration = Convert.ToInt32(row.RentalDuration);
            var dt = row.RentalDate.Kind == DateTimeKind.Unspecified
                        ? DateTime.SpecifyKind(row.RentalDate, DateTimeKind.Utc)
                        : row.RentalDate;
            var rentalDay = DateOnly.FromDateTime(dt.ToLocalTime());
            var dueDay = rentalDay.AddDays(duration);
            var today = DateOnly.FromDateTime(DateTime.Now);
            var isLate = today > dueDay;
            var daysLate = isLate ? today.DayNumber - dueDay.DayNumber : 0;

            var actualName = $"{row.FirstName} {row.LastName}";
            var matches = row.ActualCustomerId == cid;

            return (true, row.FilmTitle, rentalDay, dueDay, isLate, daysLate, duration,
                    row.ActualCustomerId, actualName, matches);
        }
    //************************************** RETURER POST ***************
        public async Task<(bool Ok, string Message)> ReturnNormalRealAsync(int inventoryId, int customerId)
        {
            var inv = (uint)inventoryId;
            var cid = (ushort)customerId;

            var rental = await _db.Rentals
                .FirstOrDefaultAsync(r => r.InventoryId == inv &&
                                          r.CustomerId == cid &&
                                          r.ReturnDate == null);

            if (rental is null)
                return (false, "Ingen öppen uthyrning hittades för vald kund + film.");

            rental.ReturnDate = DateTime.UtcNow;
            rental.LastUpdate = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return (true, "Retur registrerad.");
        }

        //********************** SENA RETURER **********************************************************
        private static (int daysLate, ushort fee) CalcLateFee(DateOnly rentalDay, int rentalDurationDays)
        {
            var due = rentalDay.AddDays(rentalDurationDays);
            var today = DateOnly.FromDateTime(DateTime.Now);
            var daysLate = today > due ? today.DayNumber - due.DayNumber : 0;
            ushort fee = (ushort)(daysLate == 0 ? 0m : (daysLate <= 3 ? 5m : 15m));
            return (daysLate, fee);
        }
        //---
        public async Task<(bool Found, int RentalId, int CustomerId, string CustomerName, 
            string FilmTitle, DateOnly RentalDate, DateOnly DueDate, 
            int DaysLate, ushort FeeAmount)>GetLateFeePreviewByInventoryAsync(int inventoryId)
        {
            var inv = (uint)inventoryId;
            var row = await _db.Rentals
                .AsNoTracking()
                .Where(r => r.InventoryId == inv && r.ReturnDate == null)
                .Select(r => new
                {
                    r.RentalId,
                    r.RentalDate,
                    Duration = r.Inventory.Film.RentalDuration,
                    Title = r.Inventory.Film.Title,
                    r.CustomerId,
                    FirstName = r.Customer.FirstName,
                    LastName = r.Customer.LastName
                })
                .FirstOrDefaultAsync();

            if (row is null)
                return (false, 0, 0, "", "", default, default, 0, 0);

            var rentDt = row.RentalDate.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(row.RentalDate, DateTimeKind.Utc)
                : row.RentalDate;

            var rentalDay = DateOnly.FromDateTime(rentDt.ToLocalTime());
            var due = rentalDay.AddDays(Convert.ToInt32(row.Duration));
            var (daysLate, fee) = CalcLateFee(rentalDay, Convert.ToInt32(row.Duration));

            return (true, (int)row.RentalId, (int)row.CustomerId,
                    $"{row.FirstName} {row.LastName}", row.Title,
                    rentalDay, due, daysLate, fee);
        }
        //---
        public async Task<(bool Found, int RentalId, int CustomerId, int InventoryId, // Lägg till InventoryId här
            string CustomerName, string FilmTitle, DateOnly RentalDate,
            DateOnly DueDate, int DaysLate, ushort FeeAmount)> GetLateFeePreviewByRentalIdAsync(int rentalId)
        {
            var rid = (uint)rentalId;
            var row = await _db.Rentals
                .AsNoTracking()
                .Where(r => r.RentalId == rid && r.ReturnDate == null)
                .Select(r => new
                {
                    r.RentalId,
                    r.RentalDate,
                    Duration = r.Inventory.Film.RentalDuration,
                    Title = r.Inventory.Film.Title,
                    r.CustomerId,
                    r.InventoryId, // <-- Lägg till denna rad
                    FirstName = r.Customer.FirstName,
                    LastName = r.Customer.LastName
                })
                .FirstOrDefaultAsync();

            if (row is null)
                return (false, 0, 0, 0, "", "", default, default, 0, 0); // Uppdatera denna rad

            var rentDt = row.RentalDate.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(row.RentalDate, DateTimeKind.Utc)
                : row.RentalDate;

            var rentalDay = DateOnly.FromDateTime(rentDt.ToLocalTime());
            var due = rentalDay.AddDays(Convert.ToInt32(row.Duration));
            var (daysLate, fee) = CalcLateFee(rentalDay, Convert.ToInt32(row.Duration));

            return (true, (int)row.RentalId, (int)row.CustomerId, (int)row.InventoryId,
                    $"{row.FirstName} {row.LastName}", row.Title,
                    rentalDay, due, daysLate, fee);
        }

        //--- Bekräfta “sen retur”: markera retur + skapa payment
        public async Task<(bool Found, bool IsLate, int DaysLate, DateOnly DueDate, 
            string FilmTitle, ushort FeeAmount, bool IsReal, int? RentalId, 
            string ActualCustomerName)>ReturnLateRealAsync(int inventoryId, int customerId)
        {
            var inv = (uint)inventoryId;
            var cid = (ushort)customerId;

            var row = await _db.Rentals
                .AsNoTracking()
                .Where(r => r.InventoryId == inv && r.ReturnDate == null)
                .Select(r => new
                {
                    r.RentalId,
                    r.CustomerId,
                    r.RentalDate,
                    Title = r.Inventory.Film.Title,
                    Duration = r.Inventory.Film.RentalDuration,
                    First = r.Customer.FirstName,
                    Last = r.Customer.LastName
                })
                .FirstOrDefaultAsync();

            if (row is null)
                return (false, false, 0, default, "", 0, false, null, "");

            var rentDt = row.RentalDate.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(row.RentalDate, DateTimeKind.Utc)
                : row.RentalDate;

            var rentalDay = DateOnly.FromDateTime(rentDt.ToLocalTime());
            var duration = Convert.ToInt32(row.Duration);
            var due = rentalDay.AddDays(duration);
            var (daysLate, fee) = CalcLateFee(rentalDay, duration);

            var isLate = daysLate > 0;
            var isReal = row.CustomerId == cid;

            return (true, isLate, daysLate, due, row.Title, fee, isReal, (int)row.RentalId, $"{row.First} {row.Last}");
        }

//********************** SKADAD RETUR/BORTTAPPAD **********************************************************
        public Task ReturnDamagedAsync(int rentalId) => Task.CompletedTask;

//********************** HJÄLPMETODER **********************************************************
        public Task<Rental?> GetOpenRentalByInventoryAsync(int inventoryId)
        {
            var inv = (uint)inventoryId;
            return _db.Rentals
                .Include(r => r.Inventory).ThenInclude(i => i.Film)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.InventoryId == inv && r.ReturnDate == null);
        }
        public Task<Rental?> GetOpenRentalByIdAsync(int rentalId)
        {
            var rid = (uint)rentalId;
            return _db.Rentals
                .Include(r => r.Inventory).ThenInclude(i => i.Film)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.RentalId == rid && r.ReturnDate == null);
        }
        
    }
}