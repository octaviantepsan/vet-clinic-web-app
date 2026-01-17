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
    }
}