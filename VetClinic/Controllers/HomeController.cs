using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using VetClinic.Data;
using VetClinic.Models;
using VetClinic.Models.ViewModels;

namespace VetClinic.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return View("PublicWelcome"); 
            }

            var userId = _userManager.GetUserId(User);

            var unpaidTotal = await _context.Bills
                .Where(b => b.Consultation!.Appointment!.Pet!.OwnerId == userId && !b.IsPaid)
                .SumAsync(b => b.TotalAmount);

            var petCount = await _context.Pets
                .CountAsync(p => p.OwnerId == userId);

            var nextAppt = await _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d!.ApplicationUser)
                .Where(a => a.Pet!.OwnerId == userId && a.DateTime > DateTime.Now)
                .OrderBy(a => a.DateTime)
                .FirstOrDefaultAsync();

                var history = await _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Consultation)
                .Where(a => a.Pet!.OwnerId == userId && a.Consultation != null) 
                .OrderByDescending(a => a.DateTime)
                .Take(3)
                .ToListAsync();

            var viewModel = new DashboardViewModel
            {
                TotalUnpaidBills = unpaidTotal,
                ActivePetCount = petCount,
                NextAppointment = nextAppt,
                RecentHistory = history
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}