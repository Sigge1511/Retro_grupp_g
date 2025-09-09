using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Retro_grupp_g.Data;
using Retro_grupp_g.Models;

namespace Retro_grupp_g.Pages.Films
{
    public class CreateModel : PageModel
    {
        private readonly Retro_grupp_g.Data.SakilaContext _context;

        public CreateModel(Retro_grupp_g.Data.SakilaContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["LanguageId"] = new SelectList(_context.Languages, "LanguageId", "LanguageId");
        ViewData["OriginalLanguageId"] = new SelectList(_context.Languages, "LanguageId", "LanguageId");
            return Page();
        }

        [BindProperty]
        public Film Film { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Films.Add(Film);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
