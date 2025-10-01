using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
          
            ModelState.Remove("Customer.Store");
            ModelState.Remove("Customer.Address");
            ModelState.Remove("Customer.Payments");
            ModelState.Remove("Customer.Rentals");

            if (Customer.StoreId < 1)
                ModelState.AddModelError("Customer.StoreId", "Välj en butik.");

            if (Customer.AddressId < 1)
                ModelState.AddModelError("Customer.AddressId", "Välj en adress.");

            if (!ModelState.IsValid)
            {
                await LoadDropdowns();

                TempData["Error"] = FirstModelError(ModelState);
                return Page();
            }

            if (Customer.CreateDate == default) Customer.CreateDate = DateTime.UtcNow;
            Customer.Active = true;

            try
            {
                await _repo.AddAsync(Customer);
                await _repo.SaveAsync();
                TempData["Message"] = $"Kunden {Customer.FirstName} {Customer.LastName} skapades!";
                return RedirectToPage("Index");
            }
            catch (DbUpdateException ex)
            {
                TempData["Error"] = $"DbUpdateException: {ex.GetBaseException().Message}";
                await LoadDropdowns();
                return Page();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Exception: {ex.GetBaseException().Message}";
                await LoadDropdowns();
                return Page();
            }
        }
        private static string FirstModelError(ModelStateDictionary ms)
        {
            foreach (var kv in ms)
                foreach (var err in kv.Value!.Errors)
                    return $"Fält: {kv.Key}, Fel: {err.ErrorMessage}";
            return "Okänt valideringsfel.";
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
