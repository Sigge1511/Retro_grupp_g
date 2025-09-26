using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        private readonly string[] StaticCategories = {"Action", "Comedy", "Children", "Drama", "Horror", "New"};
        public List<TopFilmsPerCategoryViewModel> StaticTopLists { get; set; } = new();
        public TopFilmsPerCategoryViewModel? SelectedCategoryTopList { get; set; }

        public List<SelectListItem> OtherCategories { get; set; } = new();
        [BindProperty(SupportsGet = true)]
        public int? SelectedCategoryId { get; set; }

        public void OnGet()
        {
            //De statiska kategorierna som alltid listas
            StaticTopLists = _context.Categories.Where(c => StaticCategories.Contains(c.Name))
                .Select(c => new TopFilmsPerCategoryViewModel
            {
                CategoryName = c.Name,
                TopFilms = c.FilmCategories.Select(fc => new FilmRentalCountViewModel
                {
                    Title = fc.Film.Title,
                    RentalCount = fc.Film.Inventories.SelectMany(i => i.Rentals).Count()
                })
                .OrderByDescending(f => f.RentalCount).Take(5).ToList()
            }).ToList();

            //Dropdown för alla andra kategorier
            OtherCategories = _context.Categories.Where(c => !StaticCategories.Contains(c.Name))
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.Name
                }).ToList();

            //Om personalen valt kategori från dropdownlistan
            if (SelectedCategoryId.HasValue)
            {
                SelectedCategoryTopList = _context.Categories.Where(c => c.CategoryId == SelectedCategoryId.Value)
                    .Select(c => new TopFilmsPerCategoryViewModel
                    {
                        CategoryName = c.Name,
                        TopFilms = c.FilmCategories.Select(fc => new FilmRentalCountViewModel
                        {
                            Title = fc.Film.Title,
                            RentalCount = fc.Film.Inventories.SelectMany(i => i.Rentals).Count()
                        })
                        .OrderByDescending(f => f.RentalCount).Take(5).ToList()
                    }).FirstOrDefault();
            }
        }
    }
}
