using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Retro_grupp_g.Data;
using Retro_grupp_g.Models;
using Retro_grupp_g.Repositories;

namespace Retro_grupp_g.Pages.Films
{
    public class IndexModel : PageModel
    {
        private readonly IFilmRepository _filmRepository;

        public IndexModel(IFilmRepository filmRepository)
        {
            _filmRepository = filmRepository;
        }

        public IList<Film> Film { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Film = await _filmRepository.GetAllWithLanguagesAsync();
        }
    }
}
