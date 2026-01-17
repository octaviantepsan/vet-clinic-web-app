using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetClinic.Data;
using VetClinic.Models;

namespace VetClinic.Areas.Admin.Controllers
{
    [Area("Admin")] // IMPORTANT: Tells MVC this belongs to the Admin area
    [Authorize(Roles = "Admin")] // Security: Only Admins can enter here
    public class DoctorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DoctorsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin/Doctors/Index
        public IActionResult Index()
        {
            // We use .Include() to fetch the User data (Name, Email) along with the Doctor profile
            var doctors = _context.Doctors
                .Include(d => d.ApplicationUser)
                .ToList();

            return View(doctors);
        }

        // GET: Admin/Doctors/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Doctors/Create
        [HttpPost]
        public async Task<IActionResult> Create(string firstName, string lastName, string email, string specialization, string bio)
        {
            // 1. Create the Login Account first
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true
            };

            // Note: We are setting a default temporary password. 
            // In a real app, you would email them a reset link.
            var result = await _userManager.CreateAsync(user, "Doctor123!");

            if (result.Succeeded)
            {
                // 2. Assign "Doctor" Role
                await _userManager.AddToRoleAsync(user, "Doctor");

                // 3. Create the Doctor Profile linked to that user
                var doctor = new Doctor
                {
                    ApplicationUserId = user.Id,
                    Specialization = specialization,
                    Bio = bio
                };

                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // If failure, show errors
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View();
        }
    }
}