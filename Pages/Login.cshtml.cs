using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Retro_grupp_g.Data;
using Retro_grupp_g.Models;

namespace Retro_grupp_g.Pages
{
    public class LoginModel : PageModel
    {
        private readonly SakilaDbContext _context;

        public LoginModel (SakilaDbContext context)
        {
            _context = context;
        }

        [BindProperty] public int SelectedStaffId { get; set; }
        [BindProperty] public int SelectedStoreId { get; set; }

        public List<SelectListItem> StaffList { get; set; }
        public List <SelectListItem> StoreList { get; set; }

        public void OnGet()
        {
            StaffList = _context.Staff.Select(s => new SelectListItem 
            { Value = s.StaffId.ToString(), Text = $"{s.FirstName} {s.LastName}" }).ToList();

            StoreList = _context.Stores.Select(s => new SelectListItem 
            { Value = s.StoreId.ToString(), Text = $"Store {s.StoreId}" }).ToList();
        }

        public IActionResult OnPost()
        {
            HttpContext.Session.SetInt32("StaffId", SelectedStaffId);
            HttpContext.Session.SetInt32("StoreId", SelectedStoreId);

            return RedirectToPage("/Index");
        }
    }
}
