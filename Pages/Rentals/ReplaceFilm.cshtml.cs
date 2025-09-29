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
        public decimal ReplacementCost { get; set; }
        public bool IsReal { get; set; }
        //-----------
        public ReplaceFilmModel(IRentalRepository rentalRepository, ICustomerRepository customerRepository)
        {
            _rentalRepository = rentalRepository;
            _customerRepository = customerRepository;
        }
        //-----------
        public async Task<IActionResult> OnGetAsync(int selectedInventoryId, int selectedCustomerId)
        {
            // LÄSER IN ID:N FRÅN URL:en OCH SÄTTER DE PUBLIKA EGENSKAPERNA
            SelectedInventoryId = selectedInventoryId;
            SelectedCustomerId = selectedCustomerId;

            // 1. Validera att vi har båda ID:na
            if (SelectedInventoryId <= 0 || SelectedCustomerId <= 0)
            {
                TempData["Msg"] = "Kritiska data saknas. Kontrollera kund- och inventarie-ID.";
                return RedirectToPage("/Rentals/Return"); 
            }

            // 2. Hämta preview-data från Repository (använder SelectedInventoryId)
            var (found, rentalId, customerId, customerName, filmTitle, feeAmount) =
                await _rentalRepository.GetReplaceFeePreviewByInventoryAsync(SelectedInventoryId);

            // 3. Fyll de publika egenskaperna
            Found = found;
            if (Found)
            {
                RentalId = rentalId;
                CustomerName = customerName;
                FilmTitle = filmTitle;
                ReplacementCost = (decimal)feeAmount;

                // Jämför DB:s kund-ID (customerId) mot det valda kund-ID:t (SelectedCustomerId)
                IsReal = customerId == SelectedCustomerId;
            }
            else
            {
                // VIKTIGT: Ingen omdirigering här, vi stannar på sidan för att visa meddelande
                TempData["Msg"] = "Ingen pågående uthyrning hittades för det angivna inventarie-ID:t.";
            }

            return Page();
        }
        //**********************  POST  REPLACE **************************************

        public async Task<IActionResult> OnPostConfirmReplaceAsync()
        {
            // 1. Initial validering av indata
            if (RentalId <= 0 || SelectedInventoryId <= 0 || SelectedCustomerId <= 0)
            {
                TempData["Msg"] = "Kritiska data saknas för att bekräfta ersättningen. Försök igen.";
                return RedirectToPage("/Rentals/Return");
            }

            // 2. Hämta data IGEN (Preview) för att få aktuellt kund-ID och avgift (ReplacementCost)
            // Denna anropar GetReplaceFeePreviewByInventoryAsync som hämtar ReplacementCost
            var (foundPreview, actualRentalId, actualCustomerId, actualCustomerName, filmTitle, feeAmount) =
                 await _rentalRepository.GetReplaceFeePreviewByInventoryAsync(SelectedInventoryId);

            if (!foundPreview)
            {
                TempData["Msg"] = "Kunde inte hitta uthyrning att ersätta under bekräftelse.";
                return RedirectToPage("/Rentals/Return");
            }

            // 3. AFFÄRSLOGIK: Kontrollera om det är rätt kund (likt FeeModel)
            // Jämför kund-ID från DB (actualCustomerId) med ID från formuläret (SelectedCustomerId)
            bool isReal = actualCustomerId == SelectedCustomerId;

            // 4. BARA ANROPA REPO OM ÄKTA
            if (isReal)
            {
                // Anropa Repot för att utföra transaktionen.
                // Repot är nu rent och utgår från att validering har skett
                var (success, finalFeeAmount) =
                    await _rentalRepository.ReplaceFilmRealAsync(SelectedInventoryId, SelectedCustomerId);

                if (success)
                {
                    TempData["Msg"] = $"Ersättning för film registrerad. Avgift: ${finalFeeAmount:0.00}. Filmen är nu borttagen från lagret.";
                }
                else
                {
                    TempData["Msg"] = "Ett oväntat fel uppstod under databasuppdateringen.";
                }
            }
            else
            {
                // 5. Felmeddelande om ej äkta (likt FeeModel)
                TempData["Msg"] = $"Ej rätt kund ({actualCustomerName}) som gör ersättningen. Avgiften är ${feeAmount:0.00}. Ingen ändring har sparats i databasen.";
            }

            return RedirectToPage("/Rentals/Return");
        }


    }
}
