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
        public async Task OnGet()
        {
            Customers = await _repo.GetAllAsync();
            Customers = Customers.OrderBy(c => c.FirstName).ThenBy(c => c.LastName).ToList();
        }
    }
}
