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

        // GET: Open the Medical Record form
        [HttpGet]
        public async Task<IActionResult> CreateConsultation(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null) return NotFound();

            // Pass the appointment info to the view so the doctor knows who they are treating
            ViewBag.Appointment = appointment;

            // Create a blank consultation linked to this appointment
            var model = new Consultation { AppointmentId = appointmentId };
            return View(model);
        }

        // POST: Save and Generate Bill
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateConsultation(Consultation consultation)
        {
            if (ModelState.IsValid)
            {
                // 1. Save Consultation
                _context.Consultations.Add(consultation);
                await _context.SaveChangesAsync();

                // 2. Automatically Create the Bill
                var bill = new Bill
                {
                    ConsultationId = consultation.Id,
                    TotalAmount = consultation.ServiceCost, // Cost from doctor
                    IsPaid = false
                };
                _context.Bills.Add(bill);

                // 3. Mark Appointment as "Completed" (Status 3)
                var appointment = await _context.Appointments.FindAsync(consultation.AppointmentId);
                if (appointment != null)
                {
                    appointment.Status = AppointmentStatus.Completed;
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index)); // Back to Schedule
            }
            return View(consultation);
        }
    }
}