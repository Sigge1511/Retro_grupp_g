using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Retro_grupp_g.Data;
using Retro_grupp_g.Models;
using Retro_grupp_g.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Retro_grupp_g.Pages.Films
{
    public class CreateModel : PageModel
    {
        private readonly IFilmRepository _filmRepository;
        private readonly SakilaDbContext _context; //för dropdowns
        [BindProperty] public List<int> SelectedActorIds { get; set; } = new();
        [BindProperty] public List<int> SelectedCategoryIds { get; set; } = new();

        public CreateModel(IFilmRepository filmRepository, SakilaDbContext context)
        {
            _filmRepository = filmRepository;
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Språk (value = LanguageId, text = Name)
            var languages = await _context.Languages
                .AsNoTracking()
                .Select(l => new { l.LanguageId, l.Name })
                .ToListAsync();
            ViewData["LanguageId"] = new SelectList(languages, "LanguageId", "Name");
            ViewData["OriginalLanguageId"] = new SelectList(languages, "LanguageId", "Name");

            // Rating – unika strängar
            var ratings = await _context.Films
                .AsNoTracking()
                .Select(f => f.Rating)
                .Where(r => r != null && r != "")
                .Distinct()
                .OrderBy(r => r)
                .ToListAsync();
            ViewData["Rating"] = new SelectList(ratings);

            // Kategorier (many-to-many) → MultiSelectList (value = CategoryId)
            var categories = await _context.Categories
                .AsNoTracking()
                .Select(c => new { c.CategoryId, c.Name })
                .ToListAsync();
            ViewData["Category"] = new MultiSelectList(categories, "CategoryId", "Name");

            // Skådisar (many-to-many) → MultiSelectList (value = ActorId, text = "Förnamn Efternamn")
            var actors = await _context.Actors
                .AsNoTracking()
                .Select(a => new { a.ActorId, FullName = a.FirstName + " " + a.LastName })
                .OrderBy(a => a.FullName)
                .ToListAsync();
            ViewData["Actors"] = new MultiSelectList(actors, "ActorId", "FullName");
            //ViewData["Store"] = new SelectList(_context.Stores, "Store_Id", "Store_Id");


            return Page();
        }

        [BindProperty]
        public Film Film { get; set; } = default!;

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync(); // ladda listor igen om validering faller
                return Page();
            }

            // Spara filmen först
            await _filmRepository.AddAsync(Film);
            await _filmRepository.SaveAsync();

            // Lägg till alla valda skådisar
            if (SelectedActorIds?.Any() == true)
            {
                foreach (var actorId in SelectedActorIds)
                {
                    _context.FilmActors.Add(new FilmActor
                    {
                        FilmId = Film.FilmId,
                        ActorId = (ushort)actorId
                    });
                }
                await _context.SaveChangesAsync();
            }


            // film_category-join
            if (SelectedCategoryIds?.Any() == true)
            {
                foreach (var catId in SelectedCategoryIds)
                {
                    int filmId = Film.FilmId;
                    _context.FilmCategories.Add(new FilmCategory
                    {
                        FilmId = filmId,
                        CategoryId = (byte)catId // om CategoryId är byte; annars ta bort cast
                    });
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
