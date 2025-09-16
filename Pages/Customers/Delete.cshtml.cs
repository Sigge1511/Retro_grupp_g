using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Retro_grupp_g.Repositories;
using Retro_grupp_g.Models;

namespace Retro_grupp_g.Pages.Customers
{
    public class DeleteModel : PageModel
    {
        private readonly ICustomerRepository _repo;
        public DeleteModel(ICustomerRepository repo) => _repo = repo;

        [BindProperty]
        public Customer Customer { get; set; } = new();
        public async Task<IActionResult> OnGetAsync(int id)
        {
            var c = await _repo.GetByIdAsync(id);
            if (c == null) return NotFound();
            Customer = c;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            await _repo.DeleteAsync(id);
            await _repo.SaveAsync();
            return RedirectToPage("Index");
        }
    }
}
