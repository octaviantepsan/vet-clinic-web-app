using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetClinic.Data;
using VetClinic.Models;

namespace VetClinic.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
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
                .Include(a => a.Pet).ThenInclude(p => p!.Owner)
                .Include(a => a.Doctor).ThenInclude(d => d!.ApplicationUser)
                .Include(a => a.Consultation)
                .OrderByDescending(a => a.DateTime)
                .ToListAsync();

            return View(appointments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.Status = AppointmentStatus.Accepted;

            _context.Update(appointment);
            await _context.SaveChangesAsync();

            TempData["AlertMessage"] = "Appointment Confirmed.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Appointments/Deny
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deny(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.Status = AppointmentStatus.Refused;

            _context.Update(appointment);
            await _context.SaveChangesAsync();

            TempData["AlertMessage"] = "Appointment Denied.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Appointments/Reschedule
        [HttpGet]
        public async Task<IActionResult> Reschedule(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();
            return View(appointment);
        }

        // POST: Admin/Appointments/Reschedule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reschedule(int id, DateTime newDate)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.DateTime = newDate;

            appointment.Status = AppointmentStatus.RescheduleProposed;

            await _context.SaveChangesAsync();

            TempData["AlertMessage"] = "Proposal sent to client. Waiting for their approval.";
            return RedirectToAction(nameof(Index));
        }
    }
}