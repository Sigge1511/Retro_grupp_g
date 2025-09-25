using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Retro_grupp_g.Data;
using Retro_grupp_g.Models;

namespace Retro_grupp_g.Pages.Addresses
{
    public class IndexModel : PageModel
    {
        private readonly SakilaDbContext _db;
        public IndexModel(SakilaDbContext db) => _db = db;

        public List<Address> Addresses { get; private set; } = new();

        public async Task OnGetAsync()
        {
            Addresses = await _db.Addresses
                .Include(a => a.City).ThenInclude(c => c.Country)
                .OrderBy(a => a.Address1)
                .ToListAsync();
        }
    }
}
