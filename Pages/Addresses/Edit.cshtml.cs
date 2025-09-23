using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Retro_grupp_g.Data;
using Retro_grupp_g.Models;
using Retro_grupp_g.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Retro_grupp_g.Pages.Addresses
{
    public class EditModel : PageModel
    {
        private readonly IAddressRepository _repo;
        private readonly SakilaDbContext _db;

        public EditModel(IAddressRepository repo, SakilaDbContext db)
        {
            _repo = repo;
            _db = db;
        }

        [BindProperty]
        public Models.Address Address { get; set; } = new();

        public List<City> Cities { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var address = await _repo.GetByIdAsync(id);
            if (address == null)
                return NotFound();

            Address = address;

            Cities = await _db.Cities
                .Select(c => new City { CityId = c.CityId, City1 = c.City1 })
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            Address.LastUpdate = DateTime.UtcNow;

            await _repo.UpdateAsync(Address);
            await _repo.SaveAsync();

            return RedirectToPage("/Customers/Index");
        }
    }
}
