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

        //****************
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

        //Task för att ta betalt för sen återlämning och ropa på RentalRepository

        //Task för att ta betalt för skadad film och ropa på RentalRepository

        //
        //erw4etwrt
        //****************
    }
}