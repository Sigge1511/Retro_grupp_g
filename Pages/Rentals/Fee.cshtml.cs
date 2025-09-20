using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Retro_grupp_g.Repositories;

namespace Retro_grupp_g.Pages.Rentals
{
    public class FeeModel : PageModel
    {
        private readonly IRentalRepository _rentalRepository;
        public FeeModel(IRentalRepository rentalRepository) => _rentalRepository = rentalRepository;

        [BindProperty(SupportsGet = true)] public int InventoryId { get; set; }
        [BindProperty(SupportsGet = true)] public int CustomerId { get; set; }

        [BindProperty] public int RentalId { get; set; }

        public string FilmTitle { get; private set; } = "";
        public string ActualCustomerName { get; private set; } = "";
        public DateOnly RentalDate { get; private set; }
        public DateOnly DueDate { get; private set; }
        public int DaysLate { get; private set; }
        public decimal FeeAmount { get; private set; }
        public bool IsReal { get; private set; }


        // GET LATE
        public async Task<IActionResult> OnGetFeeAsync()
        {
            if (InventoryId <= 0 || CustomerId <= 0)
            {
                TempData["Msg"] = "Ogiltig åtkomst till avgiftssidan.";
                return RedirectToPage("/Rentals/Return");
            }

            var rental = await _rentalRepository.GetOpenRentalByInventoryAsync(InventoryId);
            if (rental is null)
            {
                TempData["Msg"] = "Ingen öppen uthyrning hittades för vald film.";
                return RedirectToPage("/Rentals/Return");
            }

            FilmTitle = rental.Inventory?.Film?.Title ?? "(okänd)";
            ActualCustomerName = $"{rental.Customer.FirstName} {rental.Customer.LastName}";

            var rentDt = rental.RentalDate.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(rental.RentalDate, DateTimeKind.Utc)
                : rental.RentalDate;

            RentalDate = DateOnly.FromDateTime(rentDt.ToLocalTime());
            int duration = Convert.ToInt32(rental.Inventory?.Film?.RentalDuration ?? 0);
            DueDate = RentalDate.AddDays(duration);

            var today = DateOnly.FromDateTime(DateTime.Now);
            DaysLate = today > DueDate ? today.DayNumber - DueDate.DayNumber : 0;
            FeeAmount = DaysLate == 0 ? 0m : (DaysLate <= 3 ? 5m : 15m);

            if (DaysLate <= 0)
            {
                TempData["Msg"] = $"Returen är inte sen. Förfallodag: {DueDate:yyyy-MM-dd}.";
                return RedirectToPage("/Rentals/Return");
            }

            IsReal = rental.CustomerId == (ushort)CustomerId;
            RentalId = (int)rental.RentalId;

            return Page();
        }

        // POST LATE
        public async Task<IActionResult> OnPostReturnLateAsync()
        {
            if (RentalId <= 0 || InventoryId <= 0 || CustomerId <= 0)
            {
                TempData["Msg"] = "Ogiltiga data vid bekräftelse.";
                return RedirectToPage("/Rentals/Return");
            }
            
            var rental = await _rentalRepository.GetOpenRentalByIdAsync(RentalId)
                         ?? await _rentalRepository.GetOpenRentalByInventoryAsync(InventoryId);
            if (rental is null)
            {
                TempData["Msg"] = "Uthyrningen är redan hanterad eller saknas.";
                return RedirectToPage("/Rentals/Return");
            }
            var rentDt = rental.RentalDate.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(rental.RentalDate, DateTimeKind.Utc)
                : rental.RentalDate;
            var rentalDay = DateOnly.FromDateTime(rentDt.ToLocalTime());
            int duration = Convert.ToInt32(rental.Inventory?.Film?.RentalDuration ?? 0);
            var due = rentalDay.AddDays(duration);
            var today = DateOnly.FromDateTime(DateTime.Now);
            
            var daysLate = today > due ? today.DayNumber - due.DayNumber : 0;
            var fee = daysLate == 0 ? 0m : (daysLate <= 3 ? 5m : 15m);
            if (daysLate <= 0)
            {
                TempData["Msg"] = $"Returen är inte sen. Förfallodag: {due:yyyy-MM-dd}.";
                return RedirectToPage("/Rentals/Return");
            }
            var isReal = rental.CustomerId == (ushort)CustomerId;
            if (!isReal)
            {
                TempData["Msg"] =
                    $"Ej rätt kund som gör retur. Sen {daysLate} dag(ar). " +
                    $"Förfallodag: {due:yyyy-MM-dd}. Avgift: ${fee:0.00}. Ingen ändring sparad.";
                return RedirectToPage("/Rentals/Return");
            }
            await _rentalRepository.ReturnLateRealAsync((int)rental.RentalId, (int)fee);

            TempData["Msg"] = $"Sen retur registrerad. Avgift: ${fee:0.00}.";
            return RedirectToPage("/Rentals/Return");
        }
    }
}
