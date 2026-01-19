using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetClinic.Data;
using VetClinic.Models;

namespace VetClinic.Controllers
{
    [Authorize]
    public class BillingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BillingController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: My Bills
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var bills = await _context.Bills
                // The '!' tells the compiler: "Trust me, these tables are linked."
                .Include(b => b.Consultation)
                .Include(b => b.Consultation!.Appointment)
                .Include(b => b.Consultation!.Appointment!.Pet)
                .Include(b => b.Consultation!.Appointment!.Doctor)
                    .ThenInclude(d => d!.ApplicationUser)
                // Ensure we only fetch bills where the chain of data is complete and belongs to user
                .Where(b => b.Consultation != null
                         && b.Consultation.Appointment != null
                         && b.Consultation.Appointment.Pet != null
                         && b.Consultation.Appointment.Pet.OwnerId == userId)
                .OrderByDescending(b => b.Consultation!.Appointment!.DateTime)
                .ToListAsync();

            return View(bills);
        }
        public async Task<IActionResult> Print(int? id)
        {
            if (id == null) return NotFound();

            var bill = await _context.Bills
                .Include(b => b.Consultation)
                    .ThenInclude(c => c!.Appointment)
                        .ThenInclude(a => a!.Pet)
                            .ThenInclude(p => p!.Owner)
                .Include(b => b.Consultation)
                    .ThenInclude(c => c!.Appointment)
                        .ThenInclude(a => a!.Doctor)
                            .ThenInclude(d => d!.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (bill == null) return NotFound();

            // --- SAFETY FIX START ---
            var userId = _userManager.GetUserId(User);

            // Safe traversal using ?.
            // If Consultation is null, OR Appointment is null, OR Pet is null, this returns null instead of crashing.
            var petOwnerId = bill.Consultation?.Appointment?.Pet?.OwnerId;

            // Logic:
            // 1. Check if user is the Owner (and ownerId is not null)
            // 2. OR Check if user is Admin
            bool allowed = (petOwnerId != null && petOwnerId == userId) || User.IsInRole("Admin");

            if (!allowed)
            {
                return NotFound();
            }
            // --- SAFETY FIX END ---

            return View(bill);
        }

        // GET: Billing/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var bill = await _context.Bills
                .Include(b => b.Consultation)
                    .ThenInclude(c => c!.Appointment) // '!' used for Include chains, but we check nulls below
                        .ThenInclude(a => a!.Pet)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (bill == null) return NotFound();

            // --- SAFETY FIX START ---
            var userId = _userManager.GetUserId(User);

            // safely get the owner ID, defaulting to empty string if any link is broken
            var petOwnerId = bill.Consultation?.Appointment?.Pet?.OwnerId ?? "";

            var isOwner = (petOwnerId == userId);
            var isAdmin = User.IsInRole("Admin");

            // If the chain was broken (petOwnerId is empty) and user is NOT admin, block them.
            if (!isOwner && !isAdmin)
            {
                return NotFound();
            }
            // --- SAFETY FIX END ---

            return View(bill);
        }

        // POST: Billing/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // CRITICAL FIX: "IsPaid" must be in this list for the checkbox to work!
        public async Task<IActionResult> Edit(int id, [Bind("Id,TotalAmount,Date,ConsultationId,IsPaid")] Bill bill)
        {
            if (id != bill.Id)
            {
                return NotFound();
            }

            // Security: Only Admins can mark bills as paid
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bill);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Bills.Any(e => e.Id == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                // Redirect back to Admin Billing Dashboard
                return RedirectToAction("Index", "Billing", new { area = "Admin" });
            }
            return View(bill);
        }

        // GET: Billing/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var bill = await _context.Bills
                .Include(b => b.Consultation)
                    .ThenInclude(c => c!.Appointment)
                        .ThenInclude(a => a!.Pet)
                            .ThenInclude(p => p!.Owner)
                .Include(b => b.Consultation)
                    .ThenInclude(c => c!.Appointment)
                        .ThenInclude(a => a!.Doctor)
                            .ThenInclude(d => d!.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (bill == null) return NotFound();

            // SECURITY: Allow if User is Owner OR Admin
            var userId = _userManager.GetUserId(User);
            var ownerId = bill.Consultation?.Appointment?.Pet?.OwnerId;

            if (ownerId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(bill);
        }
    }
}