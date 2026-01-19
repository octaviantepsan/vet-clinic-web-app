using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VetClinic.Data;
using VetClinic.Models;

namespace VetClinic.Controllers
{
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Appointments (List my appointments)
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d!.ApplicationUser) // <--- Add the '!' here
                .Include(a => a.Pet)
                .Where(a => a.Pet != null && a.Pet.OwnerId == userId)
                .OrderBy(a => a.DateTime)
                .ToListAsync();

            return View(appointments);
        }

        // GET: Appointments/Create
        public async Task<IActionResult> Create()
        {
            var userId = _userManager.GetUserId(User);

            // 1. Get Logged-in User's Pets for the dropdown
            var myPets = await _context.Pets
                .Where(p => p.OwnerId == userId)
                .ToListAsync();

            if (!myPets.Any())
            {
                TempData["AlertMessage"] = "Please register a pet before booking an appointment!";

                return RedirectToAction("Create", "Pets"); // Force them to add a pet first
            }

            // 2. Get All Doctors for the dropdown
            var doctors = await _context.Doctors
                .Include(d => d.ApplicationUser)
                .ToListAsync();

            // 3. Stuff them into ViewBag (Simple way to pass data to dropdowns)
            ViewBag.PetId = new SelectList(myPets, "Id", "Name");
            ViewBag.DoctorId = new SelectList(doctors, "Id", "ApplicationUser.FullName");

            return View();
        }

        // POST: Appointments/Create
        [HttpPost]
        public async Task<IActionResult> Create(Appointment appointment)
        {
            // Clean up validation
            ModelState.Remove("Doctor");
            ModelState.Remove("Pet");

            if (ModelState.IsValid)
            {
                // Ensure status is Pending
                appointment.Status = AppointmentStatus.Pending;

                _context.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If validation fails, reload the dropdowns
            var userId = _userManager.GetUserId(User);
            ViewBag.PetId = new SelectList(_context.Pets.Where(p => p.OwnerId == userId), "Id", "Name");
            ViewBag.DoctorId = new SelectList(_context.Doctors.Include(d => d.ApplicationUser), "Id", "ApplicationUser.FullName");

            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmReschedule(int id, bool accept)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Pet)
                .FirstOrDefaultAsync(a => a.Id == id);

            // Security Check: Ensure the logged-in user owns this pet
            var userId = _userManager.GetUserId(User);
            if (appointment == null || appointment.Pet?.OwnerId != userId)
            {
                return NotFound();
            }

            if (accept)
            {
                appointment.Status = AppointmentStatus.Accepted;
                TempData["AlertMessage"] = "Appointment updated and confirmed!";
            }
            else
            {
                // If rejected, we mark it as Refused (Closed) so they can book a fresh one
                appointment.Status = AppointmentStatus.Refused;
                TempData["AlertMessage"] = "Appointment cancelled. Please book a new time that suits you.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // (Accessible only to Admin in this app context)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();

            // SECURITY: Only allow if User is Admin
            if (!User.IsInRole("Admin"))
            {
                return Forbid(); // or NotFound()
            }

            // Load Lists for Dropdowns
            ViewData["DoctorId"] = new SelectList(_context.Doctors.Include(d => d.ApplicationUser), "Id", "ApplicationUser.LastName", appointment.DoctorId);
            ViewData["PetId"] = new SelectList(_context.Pets, "Id", "Name", appointment.PetId);

            return View(appointment);
        }

        // POST: Appointments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DateTime,PetId,DoctorId,Status")] Appointment appointment)
        {
            if (id != appointment.Id) return NotFound();

            // SECURITY: Only allow if User is Admin
            if (!User.IsInRole("Admin")) return Forbid();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(appointment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Appointments.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
                // Redirect back to the Admin Dashboard if the user is an Admin
                return RedirectToAction("Index", "Appointments", new { area = "Admin" });
            }

            ViewData["DoctorId"] = new SelectList(_context.Doctors.Include(d => d.ApplicationUser), "Id", "ApplicationUser.LastName", appointment.DoctorId);
            ViewData["PetId"] = new SelectList(_context.Pets, "Id", "Name", appointment.PetId);
            return View(appointment);
        }

        // GET: Appointments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null) return NotFound();

            // SECURITY: Only allow if User is Admin
            if (!User.IsInRole("Admin")) return Forbid();

            return View(appointment);
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
            }
            // Redirect back to Admin Dashboard
            return RedirectToAction("Index", "Appointments", new { area = "Admin" });
        }

        // GET: Appointments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Pet)
                    .ThenInclude(p => p!.Owner) // Include Owner to show email
                .Include(a => a.Doctor)
                    .ThenInclude(d => d!.ApplicationUser)
                .Include(a => a.Consultation) // Include consultation to see if it's finished
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null) return NotFound();

            // SECURITY: Allow if User is Owner OR Admin
            var userId = _userManager.GetUserId(User);

            // Use safe navigation (?.) just in case data is missing
            var ownerId = appointment.Pet?.OwnerId;

            if (ownerId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(appointment);
        }
    }
}