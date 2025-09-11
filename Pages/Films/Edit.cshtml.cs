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
        private readonly Retro_grupp_g.Data.SakilaDbContext _context;

        public EditModel(Retro_grupp_g.Data.SakilaDbContext context)
        {
            _context = context;
        }

        [BindProperty] public List<int> SelectedCategoryIds { get; set; } = new();
        [BindProperty] public List<ushort> SelectedActorIds { get; set; } = new();

        [BindProperty] public Film Film { get; set; } = default!;

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

            Film = await _context.Films
                .Include(f => f.FilmCategories)
                .Include(f => f.FilmActors)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.FilmId == id);

            // Förifyll valda ID:n från join-tabellerna
            SelectedCategoryIds = Film.FilmCategories.Select(fc => (int)fc.CategoryId).ToList();
            SelectedActorIds = Film.FilmActors.Select(fa => fa.ActorId).ToList();

            await LoadSelectDataAsync();   // språk, rating, listor för checkboxar
            return Page();
        }

        private async Task LoadSelectDataAsync()
        {
            // Språk
            var languages = await _context.Languages
                .AsNoTracking()
                .Select(l => new { l.LanguageId, l.Name })
                .ToListAsync();
            ViewData["LanguageId"] = new SelectList(languages, "LanguageId", "Name", Film.LanguageId);
            ViewData["OriginalLanguageId"] = new SelectList(languages, "LanguageId", "Name", Film.OriginalLanguageId);

            // Rating (unika värden)
            var ratings = await _context.Films
                .AsNoTracking()
                .Select(f => f.Rating)
                .Where(r => r != null && r != "")
                .Distinct()
                .OrderBy(r => r)
                .ToListAsync();
            ViewData["Rating"] = new SelectList(ratings, Film.Rating);

            // Kategorier (för checkboxar)
            ViewData["Category"] = (await _context.Categories
                .AsNoTracking()
                .Select(c => new { c.CategoryId, c.Name })
                .ToListAsync())
                .Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.Name })
                .ToList();

            // Skådisar (för checkboxar)
            ViewData["Actors"] = (await _context.Actors
                .AsNoTracking()
                .Select(a => new { a.ActorId, FullName = a.FirstName + " " + a.LastName })
                .OrderBy(a => a.FullName)
                .ToListAsync())
                .Select(a => new SelectListItem { Value = a.ActorId.ToString(), Text = a.FullName })
                .ToList();            
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync(int id)
        {
            var film = await _context.Films
                .Include(f => f.FilmCategories)
                .Include(f => f.FilmActors)
                .FirstOrDefaultAsync(f => f.FilmId == id);

            if (film == null) return NotFound();
            if (!ModelState.IsValid)
            {
                Film = film; // så vi kan visa om med befintligt
                await LoadSelectDataAsync();
                return Page();
            }

            // Uppdatera enkla fält
            film.Title = Film.Title;
            film.Description = Film.Description;
            film.ReleaseYear = Film.ReleaseYear;
            film.LanguageId = Film.LanguageId;
            film.OriginalLanguageId = Film.OriginalLanguageId;
            film.RentalDuration = Film.RentalDuration;
            film.RentalRate = Film.RentalRate;
            film.Length = Film.Length;
            film.ReplacementCost = Film.ReplacementCost;
            film.Rating = Film.Rating;
            film.SpecialFeatures = Film.SpecialFeatures;

            // LastUpdate – serverstyrt
            film.LastUpdate = DateTime.UtcNow;

            // --- Synka kategorier ---
            var existingCatIds = film.FilmCategories.Select(fc => (int)fc.CategoryId).ToList();
            var toRemoveCats = film.FilmCategories.Where(fc => !SelectedCategoryIds.Contains((int)fc.CategoryId)).ToList();
            foreach (var fc in toRemoveCats) _context.FilmCategories.Remove(fc);

            var toAddCatIds = SelectedCategoryIds.Except(existingCatIds);
            foreach (var catId in toAddCatIds)
                _context.FilmCategories.Add(new FilmCategory { FilmId = film.FilmId, CategoryId = (byte)catId });

            // --- Synka skådisar ---
            var existingActorIds = film.FilmActors.Select(fa => fa.ActorId).ToList();
            var toRemoveActors = film.FilmActors.Where(fa => !SelectedActorIds.Contains(fa.ActorId)).ToList();
            foreach (var fa in toRemoveActors) _context.FilmActors.Remove(fa);

            var toAddActorIds = SelectedActorIds.Except(existingActorIds);
            foreach (var actorId in toAddActorIds)
                _context.FilmActors.Add(new FilmActor { FilmId = film.FilmId, ActorId = actorId });

            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }

        //private bool FilmExists(int id)
        //{
        //    return _context.Films.Any(e => e.FilmId == id);
        //}
    }
}
