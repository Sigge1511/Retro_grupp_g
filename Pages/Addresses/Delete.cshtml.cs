using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Retro_grupp_g.Models;
using Retro_grupp_g.Repositories;


namespace Retro_grupp_g.Pages.Addresses
{
    public class DeleteModel : PageModel
    {
        private readonly IAddressRepository _repo;
        public DeleteModel(IAddressRepository repo) => _repo = repo;

        [BindProperty] public Address Address { get; set; } = new();
        public bool InUse { get; private set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var a = await _repo.GetByIdAsync(id);
            if (a == null) return NotFound();

            Address = a;
            InUse = !await _repo.CanDeleteAsync(id);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            InUse = !await _repo.CanDeleteAsync(id);
            if (InUse)
            {
                Address = await _repo.GetByIdAsync(id) ?? new Address();
                return Page();
            }

            await _repo.DeleteAsync(id);
            await _repo.SaveAsync();
            return RedirectToPage("Index");
        }
    }
}
