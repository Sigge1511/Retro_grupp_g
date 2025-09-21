using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Retro_grupp_g.Repositories;

namespace Retro_grupp_g.Pages.Rentals
{
    public class FeeModel : PageModel
    {
        private readonly IRentalRepository _rentalRepository;

        [BindProperty(SupportsGet = true)]
        public int RentalId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int InventoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CustomerId { get; set; }

        // Dina andra publika egenskaper
        public string FilmTitle { get; set; }
        public string ActualCustomerName { get; set; }
        public DateOnly RentalDate { get; set; }
        public DateOnly DueDate { get; set; }
        public int DaysLate { get; set; }
        public decimal FeeAmount { get; set; }
        public bool IsReal { get; set; }

        public FeeModel(IRentalRepository rentalRepository)
        {
            _rentalRepository = rentalRepository;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (RentalId <= 0)
            {
                TempData["Msg"] = "Ogiltig åtkomst till avgiftssidan.";
                return RedirectToPage("/Rentals/Return");
            }

            var rentalDetails = await _rentalRepository.GetLateFeePreviewByRentalIdAsync(RentalId);

            if (!rentalDetails.Found)
            {
                TempData["Msg"] = "Ingen uthyrning hittades för angivet ID.";
                return RedirectToPage("/Rentals/Return");
            }

            // Fyll de publika egenskaperna från repository-metoden
            FilmTitle = rentalDetails.FilmTitle;
            ActualCustomerName = rentalDetails.CustomerName;
            RentalDate = rentalDetails.RentalDate;
            DueDate = rentalDetails.DueDate;
            DaysLate = rentalDetails.DaysLate;
            FeeAmount = (decimal)rentalDetails.FeeAmount;

            // Fyll de nya egenskaperna från resultatet
            // De behövs för POST-formuläret
            CustomerId = rentalDetails.CustomerId;
            InventoryId = rentalDetails.InventoryId;

            // Denna logik ska finnas här, inte i repot
            IsReal = rentalDetails.CustomerId == CustomerId;

            if (DaysLate <= 0)
            {
                TempData["Msg"] = $"Returen är inte sen. Förfallodag: {DueDate:yyyy-MM-dd}.";
                return RedirectToPage("/Rentals/Return");
            }

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
