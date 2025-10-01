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
    public class DeleteModel : PageModel
    {
        private readonly IFilmRepository _filmRepository;

        public DeleteModel(IFilmRepository filmRepository)
        {
            _filmRepository = filmRepository;
        }

        [BindProperty]
        public Film Film { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var film = await _filmRepository.GetByIdAsync(id.Value);

            if (film == null)
            {
                return NotFound();
            }
            else
            {
                Film = film;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            await _filmRepository.DeleteAsync(id.Value);
            await _filmRepository.SaveAsync();

            return RedirectToPage("./Index");
        }
    }
}
