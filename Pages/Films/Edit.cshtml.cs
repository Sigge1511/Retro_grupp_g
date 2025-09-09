using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Retro_grupp_g.Data;
using Retro_grupp_g.Models;

namespace Retro_grupp_g.Pages.Films
{
    public class EditModel : PageModel
    {
        private readonly Retro_grupp_g.Data.SakilaContext _context;

        public EditModel(Retro_grupp_g.Data.SakilaContext context)
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

            var film =  await _context.Films.FirstOrDefaultAsync(m => m.FilmId == id);
            if (film == null)
            {
                return NotFound();
            }
            Film = film;
           ViewData["LanguageId"] = new SelectList(_context.Languages, "LanguageId", "LanguageId");
           ViewData["OriginalLanguageId"] = new SelectList(_context.Languages, "LanguageId", "LanguageId");
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Film).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FilmExists(Film.FilmId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool FilmExists(int id)
        {
            return _context.Films.Any(e => e.FilmId == id);
        }
    }
}
