using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Data;
using SmartAttendance.Models;
using SmartAttendance.Services;

namespace SmartAttendance.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public DashboardController(
            IDashboardService dashboardService,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _dashboardService = dashboardService;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin")) return RedirectToAction(nameof(Admin));
            if (roles.Contains("Faculty")) return RedirectToAction(nameof(Faculty));
            if (roles.Contains("Student")) return RedirectToAction(nameof(Student));

            return RedirectToAction("Login", "Account");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Admin()
        {
            var model = await _dashboardService.GetAdminDashboardStatsAsync();
            return View(model);
        }

        [Authorize(Roles = "Faculty")]
        public async Task<IActionResult> Faculty()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var faculty = await _context.Faculties.FirstOrDefaultAsync(f => f.UserId == user.Id);
            if (faculty == null)
            {
                TempData["ErrorMessage"] = "Faculty profile not found for this account.";
                return RedirectToAction("Profile", "Account");
            }

            var model = await _dashboardService.GetFacultyDashboardStatsAsync(faculty.Id);
            return View(model);
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Student()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);
            if (student == null)
            {
                TempData["ErrorMessage"] = "Student record not found for this account.";
                return RedirectToAction("Profile", "Account");
            }

            var model = await _dashboardService.GetStudentDashboardStatsAsync(student.Id);
            return View(model);
        }
    }
}
