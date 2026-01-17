using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetClinic.Data;
using VetClinic.Models;

namespace VetClinic.Controllers
{
    [Authorize(Roles = "Doctor")] // STRICTLY for Doctors
    public class DoctorPortalController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DoctorPortalController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Get the Logged-in User ID
            var userId = _userManager.GetUserId(User);

            // 2. Find the Doctor profile linked to this User
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.ApplicationUserId == userId);

            if (doctor == null)
            {
                return NotFound("Doctor profile not found.");
            }

            // 3. Get appointments assigned to THIS doctor
            var appointments = await _context.Appointments
                .Include(a => a.Pet)
                    .ThenInclude(p => p!.Owner) // To show Client Name
                .Where(a => a.DoctorId == doctor.Id)
                // Filter: Only show Today and Future (ignore history)
                .Where(a => a.DateTime >= DateTime.Today) 
                .OrderBy(a => a.DateTime)
                .ToListAsync();

            return View(appointments);
        }
    }
}