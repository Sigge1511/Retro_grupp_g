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
    public class IndexModel : PageModel
    {
        private readonly Retro_grupp_g.Data.SakilaDbContext _context;

        public IndexModel(Retro_grupp_g.Data.SakilaDbContext context)
        {
            _context = context;
        }

        public IList<Film> Film { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Film = await _context.Films
                .Include(f => f.Language)
                .Include(f => f.OriginalLanguage).ToListAsync();
        }
    }
}
