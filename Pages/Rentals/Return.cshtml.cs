using Microsoft.AspNetCore.Mvc.RazorPages;
using Retro_grupp_g.Repositories;

namespace Retro_grupp_g.Pages.Rentals
{
    public class ReturnModel : PageModel
    {
        private readonly IRentalRepository _rentalRepository;
        private readonly ICustomerRepository _customerRepository;

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