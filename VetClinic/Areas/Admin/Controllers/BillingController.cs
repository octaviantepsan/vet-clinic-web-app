using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetClinic.Data;
using VetClinic.Models;

namespace VetClinic.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BillingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BillingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var bills = await _context.Bills
                // Step-by-step Include to avoid warnings
                .Include(b => b.Consultation)
                    .ThenInclude(c => c!.Appointment) // Use ! to suppress null warning here
                        .ThenInclude(a => a!.Pet)
                            .ThenInclude(p => p!.Owner)
                .OrderByDescending(b => b.Id)
                .ToListAsync();

            return View(bills);
        }

        [HttpPost]
        public async Task<IActionResult> MarkPaid(int id)
        {
            var bill = await _context.Bills.FindAsync(id);
            if (bill != null)
            {
                bill.IsPaid = true;
                bill.PaymentDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Print(int id)
        {
            var bill = await _context.Bills
                .Include(b => b.Consultation)
                    .ThenInclude(c => c!.Appointment)
                        .ThenInclude(a => a!.Pet)
                            .ThenInclude(p => p!.Owner)
                // Separate chain for Doctor to be safe
                .Include(b => b.Consultation)
                    .ThenInclude(c => c!.Appointment)
                        .ThenInclude(a => a!.Doctor)
                            .ThenInclude(d => d!.ApplicationUser)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bill == null) return NotFound();
            
            return View(bill);
        }
    }
}