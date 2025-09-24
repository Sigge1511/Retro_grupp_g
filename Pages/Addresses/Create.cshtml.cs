using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Retro_grupp_g.Data;
using Retro_grupp_g.Models;
using Retro_grupp_g.Repositories;
using AddressModel = Retro_grupp_g.Models.Address;

namespace Retro_grupp_g.Pages.Addresses
{
    public class CreateModel : PageModel
    {
        private readonly IAddressRepository _repo;
        private readonly SakilaDbContext _db;

        public CreateModel(IAddressRepository repo, SakilaDbContext db)
        {
            _repo = repo;
            _db = db;
        }

        [BindProperty]
        public AddressModel Address { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public IEnumerable<SelectListItem> Cities { get; private set; }
            = Enumerable.Empty<SelectListItem>();

        public async Task OnGetAsync()
        {
            await LoadDropdownsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Address.City");

            ModelState.Remove("Address.Address2");
            ModelState.Remove("Address.PostalCode");

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Address.Address1) ||
                string.IsNullOrWhiteSpace(Address.District) ||
                Address.CityId == 0 ||
                string.IsNullOrWhiteSpace(Address.Phone))
            {
                ModelState.AddModelError(string.Empty, "Fyll i alla obligatoriska fält.");
                await LoadDropdownsAsync();
                return Page();
            }

            var exists = await _repo.ExistsAsync(
                Address.Address1,
                Address.CityId,
                Address.PostalCode,
                Address.District
            );

            if (exists)
            {
                ModelState.AddModelError(string.Empty, "Adressen finns redan.");
                await LoadDropdownsAsync();
                return Page();
            }

            Address.LastUpdate = DateTime.UtcNow;

            await _repo.AddAsync(Address);
            await _repo.SaveAsync();

            return RedirectToPage("/Customers/Create");
        }

        private async Task LoadDropdownsAsync()
        {
            Cities = await _db.Cities
                .OrderBy(c => c.City1)
                .Select(c => new SelectListItem
                {
                    Value = c.CityId.ToString(),
                    Text = c.City1
                })
                .ToListAsync();
        }
    }
}
