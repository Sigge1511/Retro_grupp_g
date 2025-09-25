using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Retro_grupp_g.Repositories;

namespace Retro_grupp_g.Pages.Rentals
{
    public class ReplaceFilmModel : PageModel
    {
        private readonly IRentalRepository _rentalRepository;
        private readonly ICustomerRepository _customerRepository;

        [BindProperty] public int SelectedInventoryId { get; set; }
        [BindProperty] public int SelectedCustomerId { get; set; }

        // Egenskaper som behövs för att visa preview-datan
        public bool Found { get; set; } = false;
        public int RentalId { get; set; }
        public string FilmTitle { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        // Använder den korrekta typen för ReplacementCost
        public decimal ReplacementCost { get; set; }                
        public ReplaceFilmModel(IRentalRepository rentalRepository, ICustomerRepository customerRepository)
        {
            _rentalRepository = rentalRepository;
            _customerRepository = customerRepository;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Validera att vi har ett InventoryId
            if (SelectedInventoryId <= 0)
            {
                TempData["Msg"] = "Något blev fel, försök igen.";
                return RedirectToPage("/Rentals/Return"); // Omdirigera till sök-sidan
            }

            // 1. Hämta preview-data från Repository
            var (found, rentalId, customerId, customerName, filmTitle, feeAmount) =
                await _rentalRepository.GetReplaceFeePreviewByInventoryAsync(SelectedInventoryId);
            // 2. Fyll de publika egenskaperna
            Found = found;
            if (Found)
            {
                RentalId = rentalId;
                // Vi sparar det faktiska kund-ID:t för att jämföra i POST-steget
                SelectedCustomerId = customerId;
                CustomerName = customerName;
                FilmTitle = filmTitle;
                // Använd filmens kostnad som avgiften
                ReplacementCost = (decimal)feeAmount;
            }
            else
            {
                TempData["Msg"] = "Kunde inte hitta någon pågående uthyrning för det angivna inventarie-ID:t.";
                return RedirectToPage("/Rentals/Return");
            }

            // Visa sidan med preview-datan
            return Page();
        }
    }
}
