using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Retro_grupp_g.Repositories;
using Retro_grupp_g.Models;

namespace Retro_grupp_g.Pages.Customers
{
    public class DetailsModel : PageModel
    {
        private readonly ICustomerRepository _repo;
        public DetailsModel(ICustomerRepository repo) => _repo = repo;
        public Customer Customer {  get; set; } = default!;
        public async Task<IActionResult> OnGetAsync(int id)
        {
            var customer = await _repo.GetDetailsAsync((ushort)id);
            if (customer == null) return NotFound();

            Customer = customer;
            return Page();
        }
    }
}
