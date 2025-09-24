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
        }
    }
}
