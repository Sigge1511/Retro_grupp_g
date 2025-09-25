using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Retro_grupp_g.Data;
using Retro_grupp_g.Repositories;
using Retro_grupp_g.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Retro_grupp_g.Pages.Customers
{
    public class EditModel : PageModel
    {
        private readonly ICustomerRepository _repo;
        private readonly SakilaDbContext _db;   // dropdowns

        public EditModel(ICustomerRepository repo, SakilaDbContext db)
        {
            _repo = repo;
            _db = db;
        }

        [BindProperty]
        public Customer Customer { get; set; } = null!;

        public SelectList Stores { get; set; } = null!;
        public SelectList Addresses { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var cust = await _repo.GetByIdAsync(id);
            if (cust == null) return NotFound();

            Customer = cust;
            LoadDropdowns();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdowns(); 
                return Page();
            }

            var entity = await _repo.GetByIdAsync(id);
            if (entity is null) return NotFound();

            entity.FirstName = Customer.FirstName;
            entity.LastName = Customer.LastName;
            entity.Email = Customer.Email;
            entity.StoreId = Customer.StoreId;
            entity.AddressId = Customer.AddressId;
            entity.Active = Customer.Active;
            entity.LastUpdate = DateTime.UtcNow;

            await _repo.UpdateAsync(entity);
            await _repo.SaveAsync();

            return RedirectToPage("Index");
        }

        private void LoadDropdowns()
        {
            var stores = _db.Stores
                .OrderBy(s => s.StoreId)
                .Select(s => new { s.StoreId, Text = $"Store {s.StoreId}" })
                .ToList();
            Stores = new SelectList(stores, "StoreId", "Text", Customer?.StoreId);

            var addresses = _db.Addresses
                .OrderBy(a => a.AddressId)
                .Select(a => new { a.AddressId, Text = $"{a.AddressId} - {a.Address1}" })
                .ToList();
            Addresses = new SelectList(addresses, "AddressId", "Text", Customer?.AddressId);
        }
    }
}
