using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Retro_grupp_g.Repositories;

namespace Retro_grupp_g.Pages.Rentals
{
    public class ReturnModel : PageModel
    {
        private readonly IRentalRepository _rentalRepository;
        private readonly ICustomerRepository _customerRepository;

        [BindProperty] public int SelectedInventoryId { get; set; }
        [BindProperty] public int SelectedCustomerId { get; set; }

        public ReturnModel(IRentalRepository rentalRepository, ICustomerRepository customerRepository)
        {
            _rentalRepository = rentalRepository;
            _customerRepository = customerRepository;
        }
        
        //**************** GET RETUR **********************************************************
        public async Task OnGetAsync()
        {
            var (films, customers) = await _rentalRepository.OnGetReturnAsync();
            ViewData["Films"] = films;         // List<(int InventoryId, int FilmId, string Title)>
            ViewData["Customers"] = customers; // List<(int CustomerId, string FullName, string Email)>
        }

        public async Task<IActionResult> OnGetReturnPreviewAsync(int inventoryId, int customerId)
        {
            var p = await _rentalRepository.OnGetReturnPreviewAsync(inventoryId, customerId);
            if (!p.Found) return new JsonResult(new { found = false });

            return new JsonResult(new
            {
                found = true,
                filmTitle = p.FilmTitle,
                rentalDate = p.RentalDate.ToString("yyyy-MM-dd"),
                dueDate = p.DueDate.ToString("yyyy-MM-dd"),
                isLate = p.IsLate,
                daysLate = p.DaysLate,
                duration = p.RentalDurationDays,
                rentedById = p.ActualCustomerId,
                rentedBy = p.ActualCustomerName,
                customerMatches = p.CustomerMatches
            });
        }

        //*************** POST NORMAL RETUR **********************************************************
        public async Task<IActionResult> OnPostReturnNormalAsync()
        {
            // 1) Grundvalidering
            if (SelectedInventoryId <= 0 || SelectedCustomerId <= 0)
            {
                await OnGetAsync();
                return Page();
            }

            // 2) Hämta preview (beräknar due, senhet och om kund+inventory matchar)
            var p = await _rentalRepository.OnGetReturnPreviewAsync(SelectedInventoryId, SelectedCustomerId);

            if (!p.Found)
            {
                // Inventory är inte uthyrd ? inget att göra (mock-info)
                TempData["Msg"] = "Filmen är inte uthyrd just nu – ingen retur att registrera.";
                await OnGetAsync();
                return Page();
            }

            // 3) Om kund+inventory matchar ? SKARPT retur via repo
            if (p.CustomerMatches)
            {
                var (ok, msg) = await _rentalRepository.ReturnNormalRealAsync(SelectedInventoryId, SelectedCustomerId);

                var status = ok ? "Ok" : "Nej";
                var timing = p.IsLate ? $"Försenad {p.DaysLate} dag(ar)." : "I tid.";
                TempData["Msg"] = $"{status} {msg} Förfallodag: {p.DueDate:yyyy-MM-dd}. {timing}";

                // Ladda om listor (filmen ska försvinna ur “öppet uthyrda”)
                await OnGetAsync();
                return Page();
            }

            // 4) Annars ingen DB-ändring & visa tydligt i UI
            {
                var timing = p.IsLate ? $"Försenad {p.DaysLate} dag(ar)." : "I tid.";
                TempData["Msg"] = $"Ej rätt kund som gör retur. (Ingen ändring sparad.)";

                // Ladda om listor ändå (sidan kan ha ändrats av annan användare)
                await OnGetAsync();
                return Page();
            }
        }

//*************** POST SEN RETUR *************************
        public async Task<IActionResult> OnPostReturnLateAsync()
        {
            if (SelectedInventoryId <= 0 || SelectedCustomerId <= 0)
            {
                TempData["Msg"] = "Välj kund och film.";
                await OnGetAsync();
                return Page();
            }

            // Vi anropar denna metod för att hämta info om en ev. sen retur
            var preview = await _rentalRepository.GetLateFeePreviewByInventoryAsync(SelectedInventoryId);

            if (!preview.Found)
            {
                TempData["Msg"] = "Ingen öppen uthyrning hittades för vald film.";
                await OnGetAsync();
                return Page();
            }

            if (preview.DaysLate <= 0)
            {
                TempData["Msg"] = $"Returen är inte sen. Förfallodag: {preview.DueDate:yyyy-MM-dd}.";
                await OnGetAsync();
                return Page();
            }

            // VIKTIGT: Vi omdirigerar nu till Fee-sidan och skickar med RentalId i URL:en
            return RedirectToPage("/Rentals/Fee", new { rentalId = preview.RentalId });
        }
//**************     POST REPLACE **************
       
        public async Task<IActionResult> OnPostReplaceFilmAsync()
        {
            if (SelectedInventoryId <= 0 || SelectedCustomerId <= 0)
            {
                TempData["Msg"] = "Välj kund och film.";
                await OnGetAsync();
                return Page();
            }

            // Vi anropar preview-metoden ENBART för att se om uthyrningen finns,
            // men vi behöver inte använda resultatet i omdirigeringen.
            var preview = await _rentalRepository.GetReplaceFeePreviewByInventoryAsync(SelectedInventoryId);

            if (!preview.Found)
            {
                
                if (!preview.Found)
                {
                    TempData["Msg"] = "Ingen öppen uthyrning hittades för vald film.";
                    await OnGetAsync();
                    return Page();
                }
            }
            return RedirectToPage("/Rentals/ReplaceFilm", new
            {
                selectedInventoryId = SelectedInventoryId,
                selectedCustomerId = SelectedCustomerId
            });
            
        }

        //****************
        //Backup metod för att slippa krascha sidan vid POST utan handler
        public async Task<IActionResult> OnPostAsync()
        {
            // Denna metod fångar alla POSTs som inte matchar en specifik handler.
            // Vi ser till att data fylls på innan sidan renderas om för att undvika NullReferenceException.
            await OnGetAsync();
            return Page();
        }
    }
}