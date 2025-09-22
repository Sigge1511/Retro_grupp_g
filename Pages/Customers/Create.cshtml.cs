using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Retro_grupp_g.Data;
using Retro_grupp_g.Models;
using Retro_grupp_g.Repositories;

namespace Retro_grupp_g.Pages.Customers
{
    public class CreateModel : PageModel
    {
        private readonly ICustomerRepository _repo;
        private readonly SakilaDbContext _db;     

        public CreateModel(ICustomerRepository repo, SakilaDbContext db)
        {
            _repo = repo;
            _db = db;
        }

        [BindProperty] public Customer Customer { get; set; } = new();

        public SelectList Stores { get; set; } = null!;
        public SelectList Addresses { get; set; } = null!;

        public async Task OnGetAsync()
        {
            await LoadDropdowns();

            Customer.CreateDate = DateTime.UtcNow;
            Customer.Active = true;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns(); 
                return Page();
            }

            if (Customer.CreateDate == default)
                Customer.CreateDate = DateTime.UtcNow;

            await _repo.AddAsync(Customer);
            await _repo.SaveAsync();
            return RedirectToPage("Index");
        }

        private async Task LoadDropdowns()
        {
           
            var stores = await _db.Stores
                .OrderBy(s => s.StoreId)
                .Select(s => new { s.StoreId, Text = $"Store {s.StoreId}" })
                .ToListAsync();
            Stores = new SelectList(stores, "StoreId", "Text", Customer.StoreId);

            
            var addresses = await _db.Addresses
                .OrderBy(a => a.AddressId)
                .Select(a => new
                {
                    a.AddressId,
                    Text = $"{a.AddressId} - {a.Address1}"
                })
                .ToListAsync();
            Addresses = new SelectList(addresses, "AddressId", "Text", Customer.AddressId);
        }
    }
}
