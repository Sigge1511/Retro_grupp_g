using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Retro_grupp_g.Data;
using Retro_grupp_g.ViewModels;

namespace Retro_grupp_g.Pages.Rentals
{
    public class IndexModel : PageModel
    {
        private readonly SakilaContext _context;
        public IndexModel(SakilaContext context)
        {
            _context = context;
        }

        [BindProperty]

        public string SearchTerm { get; set; }
        public List<FilmViewModel> Films { get; set; }
         public async Task OnGetAsync()
        {
            await LoadFilmsAsync();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            await LoadFilmsAsync(SearchTerm);
            return Page();
        }

        private async Task LoadFilmsAsync(string? search = null)
        {
            var query = _context.Films
                .Include(f => f.Language)
                .Include(f => f.FilmActors).ThenInclude(fa => fa.Actor)
                .Include(f => f.FilmCategories).ThenInclude(fc => fc.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(f =>
                f.Title.Contains(search) ||
                f.Description.Contains(search) ||
                f.FilmActors.Any(a => a.Actor.FirstName.Contains(search) || a.Actor.LastName.Contains(search)) ||
                f.FilmCategories.Any(c => c.Category.Name.Contains(search)));
            }
            Films = await query.Select(f => new FilmViewModel
            {
                FilmId = f.FilmId,
                Title = f.Title,
                Description = f.Description,
                Genres = (ICollection<Models.Category>)f.FilmCategories.Select(fc => fc.Category.Name).ToList(),
                ReleaseYear = (int?)f.ReleaseYear,
                Length = (int?)f.Length,
                Rating = f.Rating,
                Language = f.Language.Name,
                ActorSummary = string.Join(", ", f.FilmActors.Select(a => $"{a.Actor.FirstName} {a.Actor.LastName}"))
            }).ToListAsync();
        }

        }
    }

