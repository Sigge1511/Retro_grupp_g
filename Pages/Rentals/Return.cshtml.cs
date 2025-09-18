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
            try
            {
                var p = await _rentalRepository.OnGetReturnPreviewAsync(inventoryId, customerId);

                if (!p.Found)
                    return new JsonResult(new { found = false });

                return new JsonResult(new
                {
                    found = true,
                    filmTitle = p.FilmTitle,
                    rentalDate = p.RentalDate.ToString("yyyy-MM-dd"),
                    dueDate = p.DueDate.ToString("yyyy-MM-dd"),
                    isLate = p.IsLate,
                    daysLate = p.DaysLate,
                    duration = p.RentalDurationDays
                });
            }
            catch (Exception ex)
            {
                // Returnera JSON istället för 500 så vi kan se felet i Network ? Response
                return new JsonResult(new { found = false, error = ex.Message, stack = ex.StackTrace });
            }
        }

        //****************
        //Ta emot inventoryID och
        //rentalID från vyn
        //Ta emot customerID från vyn

        //Se till att rätt metod anropas i rätt repository

        //Task för att returnera en film och ropa på RentalRepository 

        //Task för att ta betalt för sen återlämning och ropa på RentalRepository

        //Task för att ta betalt för skadad film och ropa på RentalRepository

        //
        //erw4etwrt
        //****************
    }
}