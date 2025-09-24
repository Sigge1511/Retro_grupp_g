using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Retro_grupp_g.Data;
using Retro_grupp_g.ViewModels;

namespace Retro_grupp_g.Pages
{
    public class TopListsModel : PageModel
    {
        private readonly SakilaDbContext _context;
       
        public TopListsModel(SakilaDbContext context)
        {
            _context = context;
        }
        public List<TopFilmsPerCategoryViewModel> TopLists { get; set; } = new();

        public void OnGet()
        {
            TopLists = _context.Categories.Select(c => new TopFilmsPerCategoryViewModel
            {
                CategoryName = c.Name,
                TopFilms = c.FilmCategories.Select(fc => new FilmRentalCountViewModel
                {
                    Title = fc.Film.Title,
                    RentalCount = fc.Film.Inventories.SelectMany(i => i.Rentals).Count()
                })
                .OrderByDescending(f => f.RentalCount).Take(5).ToList()
            }).ToList();
        }
    }
}
