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

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var bills = await _context.Bills
                .Include(b => b.Consultation)
                .Include(b => b.Consultation!.Appointment)
                .Include(b => b.Consultation!.Appointment!.Pet)
                .Include(b => b.Consultation!.Appointment!.Doctor)
                    .ThenInclude(d => d!.ApplicationUser)
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

            var userId = _userManager.GetUserId(User);

             var petOwnerId = bill.Consultation?.Appointment?.Pet?.OwnerId;

            bool allowed = (petOwnerId != null && petOwnerId == userId) || User.IsInRole("Admin");

            if (!allowed)
            {
                return NotFound();
            }

            return View(bill);
        }

        // GET: Billing/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var bill = await _context.Bills
                .Include(b => b.Consultation)
                    .ThenInclude(c => c!.Appointment)
                        .ThenInclude(a => a!.Pet)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (bill == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            var petOwnerId = bill.Consultation?.Appointment?.Pet?.OwnerId ?? "";

            var isOwner = (petOwnerId == userId);
            var isAdmin = User.IsInRole("Admin");

            if (!isOwner && !isAdmin)
            {
                return NotFound();
            }

            return View(bill);
        }

        // POST: Billing/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TotalAmount,Date,ConsultationId,IsPaid")] Bill bill)
        {
            if (id != bill.Id)
            {
                return NotFound();
            }

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
                return RedirectToAction("Index", "Billing", new { area = "Admin" });
            }
            return View(bill);
        }

        // GET: Billing/Details
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