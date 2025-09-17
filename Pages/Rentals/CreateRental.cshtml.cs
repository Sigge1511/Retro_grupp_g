using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Retro_grupp_g.Data;
using Retro_grupp_g.Models;
using Retro_grupp_g.ViewModels;
namespace Retro_grupp_g.Pages
{
    public class CreateRentalModel : PageModel
    {
        private readonly SakilaDbContext _context;

        public CreateRentalModel(SakilaDbContext context)
        {
            _context = context;
        }

        //Sökfält

        [BindProperty(SupportsGet = true)]
        public string SearchTitle { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchActor { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchDirector { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchGenre { get; set; }

        public List<FilmViewModel> Filmer { get; set; } = new();
        public List<CustomerViewModel> Customers { get; set; } = new();

        public List<StaffViewModel> StaffMembers { get; set; } = new();

        //Post hyra film

        [BindProperty]
        public int FilmId { get; set; }
        [BindProperty]
        public int SelectedCustomerId { get; set; }
        [BindProperty]
        public int SelectedStaffId { get; set; }

        public async Task OnGetAsync()
        {
            //Ladda kunder och personal
            Customers = await _context.Customers.
                Select(c => new CustomerViewModel
                {
                    CustomerId = c.CustomerId,
                    Name = c.FirstName + " " + c.LastName
                }).ToListAsync();

            StaffMembers = await _context.Staff
              .Select(s => new StaffViewModel
              {
                  StaffId = s.StaffId,
                  Name = s.FirstName + " " + s.LastName
              }).ToListAsync();

            // Kontrollera om någon sökning gjorts
            bool hasSearch = !string.IsNullOrEmpty(SearchTitle)
                          || !string.IsNullOrEmpty(SearchActor)
                          || !string.IsNullOrEmpty(SearchGenre);

            if (!hasSearch)
            {
                // Visa ingen film om ingen sökning gjorts (för att undvika lång laddning)
                Filmer = new List<FilmViewModel>();
                return;
            }


            //Ladda filmer för sökning
            var query = _context.Films
                .Include(f => f.FilmActors).ThenInclude(fa => fa.Actor)
                .Include(f => f.FilmCategories).ThenInclude(fc => fc.Category)
                .Include(f => f.Language)//Tillagt
                .AsQueryable();

            if (!string.IsNullOrEmpty(SearchTitle))
            {
                query = query.Where(f => f.Title.Contains(SearchTitle));
            }

            if (!string.IsNullOrEmpty(SearchActor))
            {
                query = query.Where(f => f.FilmActors.Any(a =>
                   a.Actor.FirstName.Contains(SearchActor) ||
                   a.Actor.LastName.Contains(SearchActor)));
            }

            if (!string.IsNullOrEmpty(SearchGenre))
            {
                query = query.Where(f => f.FilmCategories.Any(g =>
                    g.Category.Name.Contains(SearchGenre)));
            }

            var filmer = await query.Take(50).ToListAsync();//Tillagt


            //Lägg till regissörsök

            

            Filmer = filmer.Select(f => new FilmViewModel
            {
                FilmId = f.FilmId,
                Title = f.Title,
                Description = f.Description,
                Genres = f.FilmCategories.Select(fc => fc.Category.Name).ToList(),
                ReleaseYear = (int?)f.ReleaseYear,
                Length = (int?)f.Length,
                Rating = f.Rating,
                Language = f.Language?.Name ?? "",
                ActorSummary = string.Join(", ", f.FilmActors.Select(a => a.Actor.FirstName + " " + a.Actor.LastName))
            }).ToList();


        }

        //Post: Hyra Film
        public async Task<IActionResult> OnPostRentAsync()
        {
            //Kontrollera skuld
            var HasDebt = await _context.Payments
                .Where(p => p.CustomerId == SelectedCustomerId)
                .SumAsync(p => (decimal?)p.Amount) < 0;

            if (HasDebt)
            {
                {
                    ModelState.AddModelError(string.Empty, "Kunden har en skuld och kan inte hyra film.");
                    return RedirectToPage();
                }
            }

            //Skapa ny uthyrning
            var inventoryItem = await _context.Inventories
                .FirstOrDefaultAsync(i => i.FilmId == FilmId);

            if (inventoryItem == null)
            {
                ModelState.AddModelError(string.Empty, "Filmen finns inte i lager");
                return RedirectToPage();
            }
            var rental = new Rental
            {
                InventoryId = inventoryItem.InventoryId,
                CustomerId = (ushort)SelectedCustomerId,
                StaffId = (byte)SelectedStaffId,
                RentalDate = DateTime.Now
            };

            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();

            return RedirectToPage();

        }
        // POST: Returnera film
        public async Task<IActionResult> OnPostReturnAsync()
        {
            var rental = await _context.Rentals
                .Where(r => r.Inventory.FilmId == FilmId && r.ReturnDate == null)
                .FirstOrDefaultAsync();

            if (rental != null)
            {
                rental.ReturnDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        // Kontroll om filmen är tillgänglig
        public bool IsAvailable(int filmId)
        {
            var inventoryItem = _context.Inventories.FirstOrDefault(i => i.FilmId == filmId);

            if (inventoryItem == null)
                return false;

            var isRented = _context.Rentals.Any(r =>
                r.InventoryId == inventoryItem.InventoryId &&
                r.ReturnDate == null);

            return !isRented;
        }

        // ViewModels för dropdowns
        public class CustomerViewModel
        {
            public int CustomerId { get; set; }
            public string Name { get; set; } = "";
        }

        public class StaffViewModel
        {
            public int StaffId { get; set; }
            public string Name { get; set; } = "";
        }


    }
}