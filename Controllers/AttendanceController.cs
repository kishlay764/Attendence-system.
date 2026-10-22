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
    [Authorize(Roles = "Admin,Faculty")]
    public class AttendanceController : Controller
    {
        private readonly IAttendanceService _attendanceService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AttendanceController(
            IAttendanceService attendanceService,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _attendanceService = attendanceService;
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Mark(int? classSectionId, int? subjectId, DateTime? date)
        {
            await PopulateDropdownsAsync(classSectionId, subjectId);

            var selectedDate = date ?? DateTime.Today;

            // If class section and subject are specified, load the attendance sheet
            if (classSectionId.HasValue && subjectId.HasValue)
            {
                var sheet = await _attendanceService.GetAttendanceSheetAsync(classSectionId.Value, subjectId.Value, selectedDate);
                return View(sheet);
            }

            // Otherwise, load default or first available option
            var defaultModel = new MarkAttendanceViewModel
            {
                Date = selectedDate,
                ClassSectionId = classSectionId ?? 0,
                SubjectId = subjectId ?? 0
            };

            return View(defaultModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAttendance([FromBody] AttendanceBatchSubmissionModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data submitted." });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);
            int? facultyId = null;

            if (roles.Contains("Faculty"))
            {
                var faculty = await _context.Faculties.FirstOrDefaultAsync(f => f.UserId == user.Id);
                facultyId = faculty?.Id;

                // Validate that faculty is assigned to this subject and class
                var isAssigned = await _context.FacultyAssignments
                    .AnyAsync(fa => fa.FacultyId == facultyId && fa.SubjectId == model.SubjectId && fa.ClassSectionId == model.ClassSectionId && fa.IsActive);

                if (!isAssigned)
                {
                    return Json(new { success = false, message = "You are not authorized to record attendance for this class and subject." });
                }
            }

            var (success, message, count) = await _attendanceService.SaveAttendanceAsync(model, facultyId, user.Id);
            return Json(new { success, message, count });
        }

        [HttpGet]
        public async Task<IActionResult> GetSheet(int classSectionId, int subjectId, DateTime date)
        {
            var sheet = await _attendanceService.GetAttendanceSheetAsync(classSectionId, subjectId, date);
            return PartialView("_AttendanceSheetPartial", sheet);
        }

        [HttpGet]
        public async Task<IActionResult> History(AttendanceHistoryViewModel filter)
        {
            await PopulateDropdownsAsync(filter.ClassSectionId, filter.SubjectId);
            var result = await _attendanceService.GetAttendanceHistoryAsync(filter);
            return View(result);
        }

        private async Task PopulateDropdownsAsync(int? selectedSectionId = null, int? selectedSubjectId = null)
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = user != null ? await _userManager.GetRolesAsync(user) : new string[0];

            if (roles.Contains("Faculty") && user != null)
            {
                var faculty = await _context.Faculties.FirstOrDefaultAsync(f => f.UserId == user.Id);
                if (faculty != null)
                {
                    var assignments = await _context.FacultyAssignments
                        .Include(a => a.ClassSection)
                        .Include(a => a.Subject)
                        .Where(a => a.FacultyId == faculty.Id && a.IsActive)
                        .ToListAsync();

                    var sections = assignments.Select(a => a.ClassSection).Distinct().ToList();
                    var subjects = assignments.Select(a => a.Subject).Distinct().ToList();

                    ViewBag.ClassSections = new SelectList(sections, "Id", "Name", selectedSectionId);
                    ViewBag.Subjects = new SelectList(subjects, "Id", "Name", selectedSubjectId);
                    return;
                }
            }

            // Admin dropdowns
            var allSections = await _context.ClassSections.OrderBy(s => s.Name).ToListAsync();
            var allSubjects = await _context.Subjects.OrderBy(s => s.Name).ToListAsync();

            ViewBag.ClassSections = new SelectList(allSections, "Id", "Name", selectedSectionId);
            ViewBag.Subjects = new SelectList(allSubjects, "Id", "Name", selectedSubjectId);
        }
    }
}
