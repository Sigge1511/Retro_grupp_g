using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Retro_grupp_g.Data;
using Retro_grupp_g.Models;

namespace Retro_grupp_g.Pages.Films
{
    public class DeleteModel : PageModel
    {
        private readonly Retro_grupp_g.Data.SakilaDbContext _context;

        public DeleteModel(Retro_grupp_g.Data.SakilaDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Film Film { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var film = await _context.Films.FirstOrDefaultAsync(m => m.FilmId == id);

            if (film == null)
            {
                return NotFound();
            }
            else
            {
                Film = film;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var film = await _context.Films.FindAsync(id);
            if (film == null)
            {
                return NotFound();
            }

            // 1. Hämta alla Inventory-poster för denna film
            var inventoryItems = await _context.Inventories
                .Where(i => i.FilmId == id)
                .ToListAsync();
            var inventoryIds = inventoryItems.Select(i => i.InventoryId).ToList();


            // *** KONTROLLERA PÅGÅENDE UTHYRNING ***
            if (inventoryIds.Any())
            {
                var activeRental = await _context.Rentals
                    // Sök efter uthyrningar kopplade till dessa inventoryIds OCH där ReturnDate är NULL
                    .Where(r => inventoryIds.Contains(r.InventoryId) && r.ReturnDate == null)
                    .FirstOrDefaultAsync();

                if (activeRental != null)
                {
                    // Ladda filmen till BindProperty för att visa felmeddelandet på sidan
                    Film = film;

                    // Lägg till felmeddelande i ModelState
                    ModelState.AddModelError(string.Empty, "Filmen kan inte raderas eftersom den för närvarande är uthyrd. Vänta tills alla exemplar är återlämnade.");

                    // Återgå till sidan utan att radera
                    return Page();
                }
            }
            
            // *** SLUT PÅ KONTROLL ***

            // --- FORTSÄTT MED KASKRADERING (Endast om inga aktiva uthyrningar hittades) ---

            // 2. Rensa Payments (beroende av Rental) och Rentals
            if (inventoryIds.Any())
            {
                var rentalItems = await _context.Rentals
                    .Where(r => inventoryIds.Contains(r.InventoryId))
                    .ToListAsync();

                if (rentalItems.Any())
                {
                    var rentalIds = rentalItems.Select(r => r.RentalId).ToList();
                    var paymentItems = await _context.Payments
                        .Where(p => rentalIds.Contains((int)p.RentalId))
                        .ToListAsync();

                    _context.Payments.RemoveRange(paymentItems);
                    _context.Rentals.RemoveRange(rentalItems);
                }
            }

            // 3. Rensa Inventory-posterna
            _context.Inventories.RemoveRange(inventoryItems);

            // 4. Rensa FilmCategory-poster
            var filmCategories = await _context.FilmCategories
                .Where(fc => fc.FilmId == id)
                .ToListAsync();
            _context.FilmCategories.RemoveRange(filmCategories);

            // 5. Rensa FilmActor-poster
            var filmActors = await _context.FilmActors
                .Where(fa => fa.FilmId == id)
                .ToListAsync();
            _context.FilmActors.RemoveRange(filmActors);

            // 6. Radera själva filmen
            _context.Films.Remove(film);

            // 7. Spara alla ändringar
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
