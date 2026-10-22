using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Data;
using SmartAttendance.Models;
using SmartAttendance.Services;
using SmartAttendance.ViewModels.Attendance;

namespace SmartAttendance.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly IReportService _reportService;
        private readonly IAttendanceService _attendanceService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportsController(
            IReportService reportService,
            IAttendanceService attendanceService,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _reportService = reportService;
            _attendanceService = attendanceService;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            if (User.IsInRole("Student"))
            {
                return RedirectToAction(nameof(StudentWise));
            }
            return RedirectToAction(nameof(Daily));
        }

        [Authorize(Roles = "Admin,Faculty")]
        public async Task<IActionResult> Daily(DateTime? date, int? classSectionId)
        {
            var selectedDate = date ?? DateTime.Today;
            ViewBag.ClassSections = new SelectList(await _context.ClassSections.OrderBy(s => s.Name).ToListAsync(), "Id", "Name", classSectionId);
            ViewBag.SelectedDate = selectedDate;
            ViewBag.SelectedSectionId = classSectionId;

            var model = await _reportService.GetDailyReportAsync(selectedDate, classSectionId);
            return View(model);
        }

        [Authorize(Roles = "Admin,Faculty")]
        public async Task<IActionResult> SubjectWise(int? subjectId, int? classSectionId, DateTime? startDate, DateTime? endDate)
        {
            var subjects = await _context.Subjects.OrderBy(s => s.Code).ToListAsync();
            var sections = await _context.ClassSections.OrderBy(s => s.Name).ToListAsync();

            ViewBag.Subjects = new SelectList(subjects, "Id", "Name", subjectId);
            ViewBag.ClassSections = new SelectList(sections, "Id", "Name", classSectionId);
            ViewBag.SubjectId = subjectId;
            ViewBag.ClassSectionId = classSectionId;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            var model = await _reportService.GetSubjectWiseReportAsync(subjectId, classSectionId, startDate, endDate);
            return View(model);
        }

        public async Task<IActionResult> StudentWise(int? studentId, DateTime? startDate, DateTime? endDate)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);

            int targetStudentId;
            if (roles.Contains("Student"))
            {
                // Strict authorization: Student can only view their own attendance record!
                var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);
                if (student == null) return NotFound("Student record not linked.");
                targetStudentId = student.Id;
            }
            else
            {
                // Admin or Faculty can select a student
                if (!studentId.HasValue || studentId <= 0)
                {
                    var firstStudent = await _context.Students.OrderBy(s => s.RollNumber).FirstOrDefaultAsync();
                    if (firstStudent == null) return View("NoData");
                    targetStudentId = firstStudent.Id;
                }
                else
                {
                    targetStudentId = studentId.Value;
                }

                ViewBag.Students = new SelectList(
                    await _context.Students.OrderBy(s => s.RollNumber).Select(s => new { s.Id, Display = s.RollNumber + " - " + s.FirstName + " " + s.LastName }).ToListAsync(),
                    "Id", "Display", targetStudentId);
            }

            ViewBag.StudentId = targetStudentId;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            var model = await _reportService.GetStudentWiseReportAsync(targetStudentId, startDate, endDate);
            return View(model);
        }

        [Authorize(Roles = "Admin,Faculty")]
        public async Task<IActionResult> LowAttendance(double? threshold, int? classSectionId, int? subjectId)
        {
            var limit = threshold ?? 75.0;
            ViewBag.ClassSections = new SelectList(await _context.ClassSections.OrderBy(s => s.Name).ToListAsync(), "Id", "Name", classSectionId);
            ViewBag.Subjects = new SelectList(await _context.Subjects.OrderBy(s => s.Name).ToListAsync(), "Id", "Name", subjectId);
            ViewBag.Threshold = limit;
            ViewBag.ClassSectionId = classSectionId;
            ViewBag.SubjectId = subjectId;

            var model = await _reportService.GetLowAttendanceReportAsync(limit, classSectionId, subjectId);
            return View(model);
        }

        [Authorize(Roles = "Admin,Faculty")]
        public async Task<IActionResult> ExportAttendanceCsv(int? classSectionId, int? subjectId, DateTime? startDate, DateTime? endDate)
        {
            var filter = new AttendanceHistoryViewModel
            {
                ClassSectionId = classSectionId,
                SubjectId = subjectId,
                StartDate = startDate,
                EndDate = endDate,
                PageSize = 5000 // Export up to 5000 rows
            };

            var data = await _attendanceService.GetAttendanceHistoryAsync(filter);
            var csvBytes = _reportService.ExportAttendanceCsv(data.Records);

            var fileName = $"Attendance_Report_{DateTime.Now:yyyyMMdd_HHmm}.csv";
            return File(csvBytes, "text/csv", fileName);
        }

        [Authorize(Roles = "Admin,Faculty")]
        public async Task<IActionResult> ExportLowAttendanceCsv(double? threshold, int? classSectionId, int? subjectId)
        {
            var limit = threshold ?? 75.0;
            var report = await _reportService.GetLowAttendanceReportAsync(limit, classSectionId, subjectId);
            var csvBytes = _reportService.ExportLowAttendanceCsv(report);

            var fileName = $"Defaulters_Below_{limit}Pct_{DateTime.Now:yyyyMMdd}.csv";
            return File(csvBytes, "text/csv", fileName);
        }
    }
}
