using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Retro_grupp_g.Repositories;
using Retro_grupp_g.Models;

namespace Retro_grupp_g.Pages.Customers
{
    public class IndexModel : PageModel
    {
        private readonly ICustomerRepository _repo;
        public List<Customer> Customers { get; set; } = new();

        public IndexModel(ICustomerRepository repo) => _repo = repo;
        //Tillagd för sökning av kunder
        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = string.Empty;
        public async Task OnGet()
        {
            Customers = await _repo.GetAllAsync();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                Customers = Customers.Where(c =>
                c.FirstName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                c.LastName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                c.Email.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }
            //Slut på det som lades till för sökning av kund
            Customers = Customers.OrderBy(c => c.FirstName).ThenBy(c => c.LastName).ToList();
        }
    }
}
