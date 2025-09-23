using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Retro_grupp_g.Models;
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
            // Steg 1: Grundläggande validering och hämtning av session-ID.
            if (RentalId <= 0 || InventoryId <= 0 || CustomerId <= 0)
            {
                TempData["Msg"] = "Ogiltiga data vid bekräftelse.";
                return Page();
            }

            var staffId = HttpContext.Session.GetInt32("StaffId");
            var storeId = HttpContext.Session.GetInt32("StoreId");

            if (!staffId.HasValue || !storeId.HasValue)
            {
                TempData["Msg"] = "Sessionen har gått ut. Vänligen logga in igen.";
                return RedirectToPage("/Login");
            }

            // Steg 2: Kolla om det är en mock-retur baserat på om IsReal-egenskapen är false.
            // Vi litar på att OnGet har satt denna egenskap korrekt.
            if (!IsReal)
            {
                // Om det är en mock-retur, sätt meddelandet du vill ha,
                // baserat på den information som redan finns i modellen.
                TempData["Msg"] = $"Ej rätt kund som gör retur. Avgiften är ${FeeAmount:0.00}. Ingen ändring har sparats i databasen.";
            }
            else
            {
                // Steg 3: Om det är en skarp retur, utför databasoperationen och sätt TempData["Msg"].
                var success = await _rentalRepository.ReturnLateRealAsync(InventoryId, CustomerId, staffId.Value, storeId.Value);

                if (success)
                {
                    TempData["Msg"] = $"Sen retur registrerad. Avgift: ${FeeAmount:0.00}.";
                }
                else
                {
                    TempData["Msg"] = "Ett fel uppstod vid registrering av returen.";
                }
            }

            // Vi anropar nu OnGetAsync för att hämta all information igen.
            // Detta garanterar att all data visas korrekt efter en POST-åtgärd.
            return RedirectToPage("/Rentals/Fee", new { RentalId = RentalId, CustomerId = CustomerId, InventoryId = InventoryId });
        }
    }
}
