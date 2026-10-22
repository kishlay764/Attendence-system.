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
using SmartAttendance.ViewModels.Management;

namespace SmartAttendance.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? search, int? courseId, int? classSectionId)
        {
            var query = _context.Students
                .Include(s => s.Course)
                .Include(s => s.ClassSection)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(s => s.RollNumber.ToLower().Contains(term) ||
                                         s.FirstName.ToLower().Contains(term) ||
                                         s.LastName.ToLower().Contains(term) ||
                                         s.Email.ToLower().Contains(term));
            }

            if (courseId.HasValue && courseId > 0)
                query = query.Where(s => s.CourseId == courseId.Value);

            if (classSectionId.HasValue && classSectionId > 0)
                query = query.Where(s => s.ClassSectionId == classSectionId.Value);

            ViewBag.Courses = new SelectList(await _context.Courses.ToListAsync(), "Id", "Name", courseId);
            ViewBag.ClassSections = new SelectList(await _context.ClassSections.ToListAsync(), "Id", "Name", classSectionId);
            ViewBag.Search = search;

            var students = await query.OrderBy(s => s.RollNumber).ToListAsync();
            return View(students);
        }

        public async Task<IActionResult> Details(int id)
        {
            var student = await _context.Students
                .Include(s => s.Course)
                .Include(s => s.ClassSection)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.AcademicSemester)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null) return NotFound();

            var records = await _context.AttendanceRecords
                .Where(a => a.StudentId == id)
                .ToListAsync();

            var total = records.Count;
            var present = records.Count(r => r.Status == AttendanceStatus.Present);
            var late = records.Count(r => r.Status == AttendanceStatus.Late);
            var absent = records.Count(r => r.Status == AttendanceStatus.Absent);
            double pct = total > 0 ? Math.Round(((present + (late * 0.5)) / total) * 100.0, 1) : 0;

            ViewBag.TotalClasses = total;
            ViewBag.PresentClasses = present;
            ViewBag.AbsentClasses = absent;
            ViewBag.LateClasses = late;
            ViewBag.AttendancePercentage = pct;

            return View(student);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Courses = new SelectList(await _context.Courses.ToListAsync(), "Id", "Name");
            ViewBag.ClassSections = new SelectList(await _context.ClassSections.ToListAsync(), "Id", "Name");
            return View(new StudentCreateEditViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentCreateEditViewModel model)
        {
            if (await _context.Students.AnyAsync(s => s.RollNumber == model.RollNumber))
            {
                ModelState.AddModelError("RollNumber", "This Roll Number is already assigned to another student.");
            }

            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                ModelState.AddModelError("Email", "An account with this email already exists.");
            }

            if (string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("Password", "Password is required for student account creation.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Courses = new SelectList(await _context.Courses.ToListAsync(), "Id", "Name", model.CourseId);
                ViewBag.ClassSections = new SelectList(await _context.ClassSections.ToListAsync(), "Id", "Name", model.ClassSectionId);
                return View(model);
            }

            // Create Identity user
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = $"{model.FirstName} {model.LastName}".Trim(),
                PhoneNumber = model.Phone,
                UserRole = "Student",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = model.IsActive
            };

            var userResult = await _userManager.CreateAsync(user, model.Password!);
            if (!userResult.Succeeded)
            {
                foreach (var err in userResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, err.Description);
                }
                ViewBag.Courses = new SelectList(await _context.Courses.ToListAsync(), "Id", "Name", model.CourseId);
                ViewBag.ClassSections = new SelectList(await _context.ClassSections.ToListAsync(), "Id", "Name", model.ClassSectionId);
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, "Student");

            // Create Student Profile
            var student = new Student
            {
                UserId = user.Id,
                RollNumber = model.RollNumber,
                RegistrationNumber = model.RegistrationNumber,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Phone = model.Phone,
                CourseId = model.CourseId,
                ClassSectionId = model.ClassSectionId,
                CurrentSemester = model.CurrentSemester,
                Gender = model.Gender,
                DateOfBirth = model.DateOfBirth,
                AdmissionDate = model.AdmissionDate,
                IsActive = model.IsActive
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            // Automatically create enrollment if section selected
            if (model.ClassSectionId.HasValue)
            {
                var sem = await _context.AcademicSemesters.FirstOrDefaultAsync(s => s.IsCurrent)
                          ?? await _context.AcademicSemesters.FirstOrDefaultAsync();

                if (sem != null)
                {
                    _context.StudentEnrollments.Add(new StudentEnrollment
                    {
                        StudentId = student.Id,
                        ClassSectionId = model.ClassSectionId.Value,
                        AcademicSemesterId = sem.Id,
                        EnrollmentDate = DateTime.Today
                    });
                    await _context.SaveChangesAsync();
                }
            }

            TempData["SuccessMessage"] = $"Student {student.FullName} ({student.RollNumber}) enrolled successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            var model = new StudentCreateEditViewModel
            {
                Id = student.Id,
                RollNumber = student.RollNumber,
                RegistrationNumber = student.RegistrationNumber,
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email,
                Phone = student.Phone,
                CourseId = student.CourseId,
                ClassSectionId = student.ClassSectionId,
                CurrentSemester = student.CurrentSemester,
                Gender = student.Gender ?? "Male",
                DateOfBirth = student.DateOfBirth,
                AdmissionDate = student.AdmissionDate,
                IsActive = student.IsActive
            };

            ViewBag.Courses = new SelectList(await _context.Courses.ToListAsync(), "Id", "Name", student.CourseId);
            ViewBag.ClassSections = new SelectList(await _context.ClassSections.ToListAsync(), "Id", "Name", student.ClassSectionId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StudentCreateEditViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (await _context.Students.AnyAsync(s => s.RollNumber == model.RollNumber && s.Id != id))
            {
                ModelState.AddModelError("RollNumber", "This Roll Number is already used by another student.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Courses = new SelectList(await _context.Courses.ToListAsync(), "Id", "Name", model.CourseId);
                ViewBag.ClassSections = new SelectList(await _context.ClassSections.ToListAsync(), "Id", "Name", model.ClassSectionId);
                return View(model);
            }

            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            student.RollNumber = model.RollNumber;
            student.RegistrationNumber = model.RegistrationNumber;
            student.FirstName = model.FirstName;
            student.LastName = model.LastName;
            student.Phone = model.Phone;
            student.CourseId = model.CourseId;
            student.ClassSectionId = model.ClassSectionId;
            student.CurrentSemester = model.CurrentSemester;
            student.Gender = model.Gender;
            student.DateOfBirth = model.DateOfBirth;
            student.IsActive = model.IsActive;

            // Also update ApplicationUser
            var user = await _userManager.FindByIdAsync(student.UserId);
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
            TempData["SuccessMessage"] = $"Student {student.FullName} updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            var user = await _userManager.FindByIdAsync(student.UserId);

            // Remove enrollments and attendances
            var enrollments = _context.StudentEnrollments.Where(e => e.StudentId == id);
            _context.StudentEnrollments.RemoveRange(enrollments);

            var attendances = _context.AttendanceRecords.Where(a => a.StudentId == id);
            _context.AttendanceRecords.RemoveRange(attendances);

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }

            TempData["SuccessMessage"] = "Student and associated account deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
