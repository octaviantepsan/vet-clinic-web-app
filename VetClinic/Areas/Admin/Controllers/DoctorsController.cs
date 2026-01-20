using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VetClinic.Data;
using VetClinic.Models;

namespace VetClinic.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DoctorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DoctorsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin/Doctors
        public async Task<IActionResult> Index()
        {
            var doctors = await _context.Doctors
                .Include(d => d.ApplicationUser)
                .ToListAsync();
            return View(doctors);
        }

        // GET: Admin/Doctors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Doctors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string firstName, string lastName, string email, string password, string specialization, string bio)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true, 
                IsDarkMode = false
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Doctor");

                var doctor = new Doctor
                {
                    ApplicationUserId = user.Id,
                    Specialization = specialization,
                    Bio = bio
                };

                _context.Add(doctor);
                await _context.SaveChangesAsync();

                TempData["AlertMessage"] = $"Dr. {lastName} created successfully!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View();
        }
        
        // GET: Admin/Doctors/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var doctor = await _context.Doctors.Include(d => d.ApplicationUser).FirstOrDefaultAsync(m => m.Id == id);
            if (doctor == null) return NotFound();

            return View(doctor);
        }

        // POST: Admin/Doctors/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Doctor doctor, string firstName, string lastName)
        {
            if (id != doctor.Id) return NotFound();

            var existingDoctor = await _context.Doctors
                .Include(d => d.ApplicationUser)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (existingDoctor == null) return NotFound();

            if (ModelState.IsValid)
            {
                existingDoctor.Specialization = doctor.Specialization;
                existingDoctor.Bio = doctor.Bio;

                if (existingDoctor.ApplicationUser != null)
                {
                    existingDoctor.ApplicationUser.FirstName = firstName;
                    existingDoctor.ApplicationUser.LastName = lastName;
                }

                _context.Update(existingDoctor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(doctor);
        }
    }
}