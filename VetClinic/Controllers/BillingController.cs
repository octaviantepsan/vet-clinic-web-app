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

        // GET: Print Invoice
        public async Task<IActionResult> Print(int id)
        {
            var userId = _userManager.GetUserId(User);

            var bill = await _context.Bills
                .Include(b => b.Consultation)
                    .ThenInclude(c => c!.Appointment)
                        .ThenInclude(a => a!.Pet)
                            .ThenInclude(p => p!.Owner)
                 .Include(b => b.Consultation!.Appointment!.Doctor)
                    .ThenInclude(d => d!.ApplicationUser)
                .FirstOrDefaultAsync(b => b.Id == id);

            // Safe navigation (?.) to prevent crashes if data is missing
            if (bill?.Consultation?.Appointment?.Pet?.OwnerId != userId)
            {
                return NotFound();
            }

            return View(bill);
        }
    }
}