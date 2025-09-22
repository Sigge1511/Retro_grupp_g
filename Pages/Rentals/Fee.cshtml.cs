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
        // GET LATE
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
            // Din logik för att kontrollera ID:n
            if (RentalId <= 0 || InventoryId <= 0 || CustomerId <= 0)
            {
                TempData["Msg"] = "Ogiltiga data vid bekräftelse.";
                return RedirectToPage("/Rentals/Return");
            }

            // Nu kommer dekonstruktionen att fungera
            var (found, isLate, daysLate, dueDate, filmTitle, feeAmount, isReal, rentalId, actualCustomerName) =
                await _rentalRepository.ReturnLateRealAsync(InventoryId, CustomerId);

            // Hantera meddelanden baserat på 'isReal'
            if (isReal)
            {
                TempData["Msg"] = $"Sen retur registrerad. Avgift: ${feeAmount:0.00}.";
            }
            else
            {
                TempData["Msg"] = $"Ej rätt kund som gör retur. Avgiften är ${feeAmount:0.00}. Ingen ändring har sparats i databasen.";
            }

            return RedirectToPage("/Rentals/Return");
        }
    }
}
