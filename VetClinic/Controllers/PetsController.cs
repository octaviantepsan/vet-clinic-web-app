using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetClinic.Data;
using VetClinic.Models;

namespace VetClinic.Controllers
{
    [Authorize]
    public class PetsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PetsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Pets
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var pets = await _context.Pets.Where(p => p.OwnerId == userId).ToListAsync();
            return View(pets);
        }

        // GET: Pets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            var pet = await _context.Pets
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Consultation)
                        .ThenInclude(c => c!.Bill) // <--- NEW LINE: Load the Bill info
                                                   // Load Doctor info safely
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Doctor)
                        .ThenInclude(d => d!.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (pet == null || pet.OwnerId != userId)
            {
                return NotFound();
            }

            return View(pet);
        }

        // GET: Pets/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Pets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Pet pet)
        {
            pet.OwnerId = _userManager.GetUserId(User)!;
            ModelState.Remove("Owner");
            ModelState.Remove("OwnerId");

            if (ModelState.IsValid)
            {
                // IMAGE UPLOAD LOGIC
                if (pet.ImageFile != null && pet.ImageFile.Length > 0)
                {
                    // 1. Create a unique filename (so "dog.jpg" doesn't overwrite another "dog.jpg")
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(pet.ImageFile.FileName);

                    // 2. Define the save path (wwwroot/images/pets)
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/pets", fileName);

                    // 3. Ensure the directory exists
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                    // 4. Save the file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await pet.ImageFile.CopyToAsync(stream);
                    }

                    // 5. Save the RELATIVE path to the database
                    pet.ImageUrl = "/images/pets/" + fileName;
                }

                _context.Add(pet);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(pet);
        }

        // GET: Pets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var pet = await _context.Pets.FindAsync(id);
            var userId = _userManager.GetUserId(User);

            if (pet == null || pet.OwnerId != userId) return NotFound();

            return View(pet);
        }

        // POST: Pets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Pet pet)
        {
            if (id != pet.Id) return NotFound();

            var originalPet = await _context.Pets.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (originalPet == null) return NotFound();

            pet.OwnerId = originalPet.OwnerId;

            if (pet.ImageFile == null)
            {
                pet.ImageUrl = originalPet.ImageUrl;
            }

            ModelState.Remove("Owner");
            ModelState.Remove("OwnerId");
            ModelState.Remove("Appointments");

            if (ModelState.IsValid)
            {

                if (pet.ImageFile != null && pet.ImageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(pet.ImageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/pets", fileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await pet.ImageFile.CopyToAsync(stream);
                    }

                    pet.ImageUrl = "/images/pets/" + fileName;
                }

                try
                {
                    _context.Update(pet);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Pets.Any(e => e.Id == pet.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(pet);
        }

        // GET: Pets/Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var pet = await _context.Pets
                .FirstOrDefaultAsync(m => m.Id == id);

            if (pet == null || pet.OwnerId != userId) return NotFound();

            return View(pet);
        }

        // POST: Pets/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pet = await _context.Pets.FindAsync(id);

            if (pet != null)
            {
                var userId = _userManager.GetUserId(User);
                if (pet.OwnerId != userId)
                {
                    return Forbid();
                }

                if (!string.IsNullOrEmpty(pet.ImageUrl))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", pet.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Pets.Remove(pet);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}