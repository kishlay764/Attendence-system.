using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Data;
using SmartAttendance.Models;
using SmartAttendance.ViewModels.Management;

namespace SmartAttendance.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FacultyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FacultyController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? search, string? department)
        {
            var query = _context.Faculties
                .Include(f => f.Assignments)
                    .ThenInclude(a => a.Subject)
                .Include(f => f.Assignments)
                    .ThenInclude(a => a.ClassSection)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(f => f.FacultyCode.ToLower().Contains(term) ||
                                         f.FirstName.ToLower().Contains(term) ||
                                         f.LastName.ToLower().Contains(term) ||
                                         f.Email.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(department))
            {
                query = query.Where(f => f.Department == department);
            }

            var departments = await _context.Faculties
                .Select(f => f.Department)
                .Distinct()
                .ToListAsync();

            ViewBag.Departments = departments;
            ViewBag.SelectedDept = department;
            ViewBag.Search = search;

            var faculties = await query.OrderBy(f => f.FacultyCode).ToListAsync();
            return View(faculties);
        }

        public async Task<IActionResult> Details(int id)
        {
            var faculty = await _context.Faculties
                .Include(f => f.Assignments)
                    .ThenInclude(a => a.Subject)
                .Include(f => f.Assignments)
                    .ThenInclude(a => a.ClassSection)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (faculty == null) return NotFound();

            var recordedCount = await _context.AttendanceRecords
                .CountAsync(a => a.RecordedByFacultyId == id);

            ViewBag.TotalRecordedSessions = recordedCount;
            return View(faculty);
        }

        public IActionResult Create()
        {
            return View(new FacultyCreateEditViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FacultyCreateEditViewModel model)
        {
            if (await _context.Faculties.AnyAsync(f => f.FacultyCode == model.FacultyCode))
            {
                ModelState.AddModelError("FacultyCode", "This Faculty Code is already in use.");
            }

            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                ModelState.AddModelError("Email", "An account with this email already exists.");
            }

            if (string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("Password", "Password is required for faculty account creation.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = $"{model.FirstName} {model.LastName}".Trim(),
                PhoneNumber = model.Phone,
                UserRole = "Faculty",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = model.IsActive
            };

            var result = await _userManager.CreateAsync(user, model.Password!);
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, err.Description);
                }
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, "Faculty");

            var faculty = new Faculty
            {
                UserId = user.Id,
                FacultyCode = model.FacultyCode,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Phone = model.Phone,
                Department = model.Department,
                Designation = model.Designation,
                Qualification = model.Qualification,
                JoiningDate = model.JoiningDate,
                IsActive = model.IsActive
            };

            _context.Faculties.Add(faculty);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Faculty member {faculty.FullName} registered successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var faculty = await _context.Faculties.FindAsync(id);
            if (faculty == null) return NotFound();

            var model = new FacultyCreateEditViewModel
            {
                Id = faculty.Id,
                FacultyCode = faculty.FacultyCode,
                FirstName = faculty.FirstName,
                LastName = faculty.LastName,
                Email = faculty.Email,
                Phone = faculty.Phone,
                Department = faculty.Department,
                Designation = faculty.Designation,
                Qualification = faculty.Qualification,
                JoiningDate = faculty.JoiningDate,
                IsActive = faculty.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FacultyCreateEditViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (await _context.Faculties.AnyAsync(f => f.FacultyCode == model.FacultyCode && f.Id != id))
            {
                ModelState.AddModelError("FacultyCode", "This Faculty Code is already in use by another faculty.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var faculty = await _context.Faculties.FindAsync(id);
            if (faculty == null) return NotFound();

            faculty.FacultyCode = model.FacultyCode;
            faculty.FirstName = model.FirstName;
            faculty.LastName = model.LastName;
            faculty.Phone = model.Phone;
            faculty.Department = model.Department;
            faculty.Designation = model.Designation;
            faculty.Qualification = model.Qualification;
            faculty.JoiningDate = model.JoiningDate;
            faculty.IsActive = model.IsActive;

            var user = await _userManager.FindByIdAsync(faculty.UserId);
            if (user != null)
            {
                user.FullName = $"{model.FirstName} {model.LastName}".Trim();
                user.PhoneNumber = model.Phone;
                user.IsActive = model.IsActive;
                await _userManager.UpdateAsync(user);

                if (!string.IsNullOrEmpty(model.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    await _userManager.ResetPasswordAsync(user, token, model.Password);
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Faculty {faculty.FullName} updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var faculty = await _context.Faculties.FindAsync(id);
            if (faculty == null) return NotFound();

            var user = await _userManager.FindByIdAsync(faculty.UserId);

            // Remove assignments
            var assignments = _context.FacultyAssignments.Where(a => a.FacultyId == id);
            _context.FacultyAssignments.RemoveRange(assignments);

            _context.Faculties.Remove(faculty);
            await _context.SaveChangesAsync();

            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }

            TempData["SuccessMessage"] = "Faculty member removed successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
