using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetClinic.Data;
using VetClinic.Models;

namespace VetClinic.Controllers
{
    [Authorize] // Requires login, but works for ANY logged-in user (Clients)
    public class PetsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PetsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Pets
        public async Task<IActionResult> Index()
        {
            // 1. Find out WHO is logged in right now
            var userId = _userManager.GetUserId(User);

            // 2. Fetch ONLY pets belonging to this user
            var myPets = await _context.Pets
                .Where(p => p.OwnerId == userId)
                .ToListAsync();

            return View(myPets);
        }

        // GET: /Pets/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Pets/Create
        [HttpPost]
        public async Task<IActionResult> Create(Pet pet)
        {
            // Remove "Owner" from validation because we set it manually below
            ModelState.Remove("Owner");
            ModelState.Remove("OwnerId");

            if (ModelState.IsValid)
            {
                // 1. Set the Owner to the currently logged-in user
                var userId = _userManager.GetUserId(User);

                if (userId == null)
                {
                    return NotFound("User not recognized.");
                }

                pet.OwnerId = userId;

                // 2. Save to DB
                _context.Pets.Add(pet);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(pet);
        }
    }
}