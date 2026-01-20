using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetClinic.Data;
using VetClinic.Models;

namespace VetClinic.Controllers
{
    [Authorize(Roles = "Doctor")]
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
            var userId = _userManager.GetUserId(User);

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.ApplicationUserId == userId);

            if (doctor == null)
            {
                return NotFound("Doctor profile not found.");
            }

            var appointments = await _context.Appointments
                .Include(a => a.Pet)
                    .ThenInclude(p => p!.Owner)
                .Where(a => a.DoctorId == doctor.Id)
                .Where(a => a.DateTime >= DateTime.Today)
                .OrderBy(a => a.DateTime)
                .ToListAsync();

            return View(appointments);
        }

        [HttpGet]
        public async Task<IActionResult> CreateConsultation(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null) return NotFound();

            ViewBag.Appointment = appointment;

            var model = new Consultation { AppointmentId = appointmentId };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateConsultation(Consultation consultation)
        {
            if (ModelState.IsValid)
            {
                _context.Consultations.Add(consultation);
                await _context.SaveChangesAsync();

                var bill = new Bill
                {
                    ConsultationId = consultation.Id,
                    TotalAmount = consultation.ServiceCost, // Cost from doctor
                    IsPaid = false
                };
                _context.Bills.Add(bill);

                var appointment = await _context.Appointments.FindAsync(consultation.AppointmentId);
                if (appointment != null)
                {
                    appointment.Status = AppointmentStatus.Completed;
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(consultation);
        }
    }
}