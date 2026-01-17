using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetClinic.Data;
using VetClinic.Models;

namespace VetClinic.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Only Admins can touch this
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Appointments
        public async Task<IActionResult> Index()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Pet)
                    .ThenInclude(p => p!.Owner) // Get Client Name
                .Include(a => a.Doctor)
                    .ThenInclude(d => d!.ApplicationUser) // Get Doctor Name
                .OrderByDescending(a => a.DateTime) // Newest first
                .ToListAsync();

            return View(appointments);
        }

        // POST: Admin/Appointments/UpdateStatus
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, int status) // Change AppointmentStatus to int
        {
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment == null)
            {
                return NotFound();
            }

            // Cast the integer back to the Enum manually
            appointment.Status = (AppointmentStatus)status;

            _context.Update(appointment); // Explicitly mark as modified
            await _context.SaveChangesAsync();

            TempData["AlertMessage"] = $"Appointment updated successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}