using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Retro_grupp_g.Data;

namespace Retro_grupp_g.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly SakilaDbContext _context;
        public string StaffName { get; set; } = "";

        public IndexModel(ILogger<IndexModel> logger, SakilaDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public void OnGet()
        {
            var staffID = HttpContext.Session.GetInt32("StaffId");

            if (staffID == null)
            {
                Response.Redirect("/Login");
                return;
            }
            
            var staff = _context.Staff.FirstOrDefault(s => s.StaffId == staffID);
            StaffName = $"{staff.FirstName} {staff.LastName}";
            
        }

       
    }
}
