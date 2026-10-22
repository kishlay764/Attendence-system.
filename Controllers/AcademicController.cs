using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Data;
using SmartAttendance.Models;
using SmartAttendance.ViewModels.Management;

namespace SmartAttendance.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AcademicController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AcademicController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= COURSES =================
        public async Task<IActionResult> Courses()
        {
            var courses = await _context.Courses
                .Include(c => c.Subjects)
                .Include(c => c.Students)
                .OrderBy(c => c.Code)
                .Select(c => new CourseViewModel
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name,
                    Department = c.Department,
                    DurationYears = c.DurationYears,
                    Description = c.Description,
                    TotalSubjects = c.Subjects.Count,
                    TotalStudents = c.Students.Count
                })
                .ToListAsync();

            return View(courses);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(CourseViewModel model)
        {
            if (await _context.Courses.AnyAsync(c => c.Code == model.Code))
            {
                TempData["ErrorMessage"] = "A course with this code already exists.";
                return RedirectToAction(nameof(Courses));
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid course data provided.";
                return RedirectToAction(nameof(Courses));
            }

            var course = new Course
            {
                Code = model.Code.ToUpper().Trim(),
                Name = model.Name.Trim(),
                Department = model.Department.Trim(),
                DurationYears = model.DurationYears,
                Description = model.Description
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Course {course.Code} added successfully.";
            return RedirectToAction(nameof(Courses));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Students)
                .Include(c => c.Subjects)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();

            if (course.Students.Any() || course.Subjects.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete course with associated students or subjects. Please reassign or delete them first.";
                return RedirectToAction(nameof(Courses));
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Course removed successfully.";
            return RedirectToAction(nameof(Courses));
        }

        // ================= SUBJECTS =================
        public async Task<IActionResult> Subjects(int? courseId)
        {
            var query = _context.Subjects.Include(s => s.Course).AsQueryable();

            if (courseId.HasValue && courseId > 0)
            {
                query = query.Where(s => s.CourseId == courseId.Value);
            }

            ViewBag.Courses = new SelectList(await _context.Courses.ToListAsync(), "Id", "Name", courseId);
            ViewBag.SelectedCourseId = courseId;

            var subjects = await query.OrderBy(s => s.SemesterNumber).ThenBy(s => s.Code).ToListAsync();
            return View(subjects);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSubject(SubjectViewModel model)
        {
            if (await _context.Subjects.AnyAsync(s => s.Code == model.Code))
            {
                TempData["ErrorMessage"] = "A subject with this code already exists.";
                return RedirectToAction(nameof(Subjects));
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill in all subject details properly.";
                return RedirectToAction(nameof(Subjects));
            }

            var subject = new Subject
            {
                Code = model.Code.ToUpper().Trim(),
                Name = model.Name.Trim(),
                Credits = model.Credits,
                SemesterNumber = model.SemesterNumber,
                CourseId = model.CourseId
            };

            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Subject {subject.Code} created successfully.";
            return RedirectToAction(nameof(Subjects));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSubject(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null) return NotFound();

            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Subject deleted successfully.";
            return RedirectToAction(nameof(Subjects));
        }

        // ================= CLASS SECTIONS =================
        public async Task<IActionResult> Sections()
        {
            var sections = await _context.ClassSections
                .Include(s => s.Course)
                .Include(s => s.AcademicSemester)
                .Include(s => s.Enrollments)
                .OrderBy(s => s.Name)
                .Select(s => new ClassSectionViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    CourseId = s.CourseId,
                    CourseName = s.Course!.Name,
                    AcademicSemesterId = s.AcademicSemesterId,
                    SemesterName = s.AcademicSemester!.Name,
                    RoomNumber = s.RoomNumber,
                    Capacity = s.Capacity,
                    EnrolledStudentsCount = s.Enrollments.Count(e => e.IsActive)
                })
                .ToListAsync();

            ViewBag.Courses = new SelectList(await _context.Courses.ToListAsync(), "Id", "Name");
            ViewBag.Semesters = new SelectList(await _context.AcademicSemesters.ToListAsync(), "Id", "Name");

            return View(sections);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSection(ClassSectionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please provide all required class section info.";
                return RedirectToAction(nameof(Sections));
            }

            var sec = new ClassSection
            {
                Name = model.Name.Trim(),
                CourseId = model.CourseId,
                AcademicSemesterId = model.AcademicSemesterId,
                RoomNumber = model.RoomNumber,
                Capacity = model.Capacity
            };

            _context.ClassSections.Add(sec);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Class Section {sec.Name} created.";
            return RedirectToAction(nameof(Sections));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSection(int id)
        {
            var sec = await _context.ClassSections.FindAsync(id);
            if (sec == null) return NotFound();

            _context.ClassSections.Remove(sec);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Class Section removed.";
            return RedirectToAction(nameof(Sections));
        }

        // ================= ENROLLMENTS =================
        public async Task<IActionResult> Enrollments(int id)
        {
            var section = await _context.ClassSections
                .Include(c => c.Course)
                .Include(c => c.AcademicSemester)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (section == null) return NotFound();

            var enrollments = await _context.StudentEnrollments
                .Include(e => e.Student)
                .Where(e => e.ClassSectionId == id && e.IsActive)
                .OrderBy(e => e.Student!.RollNumber)
                .Select(e => new EnrolledStudentRowViewModel
                {
                    EnrollmentId = e.Id,
                    StudentId = e.StudentId,
                    RollNumber = e.Student!.RollNumber,
                    FullName = e.Student.FullName,
                    Email = e.Student.Email,
                    EnrollmentDate = e.EnrollmentDate
                })
                .ToListAsync();

            var enrolledStudentIds = enrollments.Select(e => e.StudentId).ToList();

            var availableStudents = await _context.Students
                .Where(s => s.CourseId == section.CourseId && !enrolledStudentIds.Contains(s.Id) && s.IsActive)
                .OrderBy(s => s.RollNumber)
                .Select(s => new AvailableStudentOptionViewModel
                {
                    StudentId = s.Id,
                    RollNumber = s.RollNumber,
                    FullName = s.FullName,
                    Email = s.Email
                })
                .ToListAsync();

            var model = new EnrollmentManagementViewModel
            {
                ClassSectionId = section.Id,
                ClassSectionName = section.Name,
                CourseName = section.Course?.Name ?? "",
                SemesterName = section.AcademicSemester?.Name ?? "",
                EnrolledStudents = enrollments,
                AvailableStudents = availableStudents
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollStudent(int classSectionId, int studentId)
        {
            var section = await _context.ClassSections.FindAsync(classSectionId);
            if (section == null) return NotFound();

            var existing = await _context.StudentEnrollments
                .FirstOrDefaultAsync(e => e.ClassSectionId == classSectionId && e.StudentId == studentId);

            if (existing != null)
            {
                existing.IsActive = true;
                existing.EnrollmentDate = DateTime.Today;
            }
            else
            {
                _context.StudentEnrollments.Add(new StudentEnrollment
                {
                    ClassSectionId = classSectionId,
                    StudentId = studentId,
                    AcademicSemesterId = section.AcademicSemesterId,
                    EnrollmentDate = DateTime.Today,
                    IsActive = true
                });
            }

            var student = await _context.Students.FindAsync(studentId);
            if (student != null)
            {
                student.ClassSectionId = classSectionId;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Student enrolled successfully.";
            return RedirectToAction(nameof(Enrollments), new { id = classSectionId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveEnrollment(int classSectionId, int enrollmentId)
        {
            var enrollment = await _context.StudentEnrollments.FindAsync(enrollmentId);
            if (enrollment != null)
            {
                _context.StudentEnrollments.Remove(enrollment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Student removed from class section.";
            }

            return RedirectToAction(nameof(Enrollments), new { id = classSectionId });
        }

        // ================= FACULTY ASSIGNMENTS =================
        public async Task<IActionResult> FacultyAssignments()
        {
            var assignments = await _context.FacultyAssignments
                .Include(fa => fa.Faculty)
                .Include(fa => fa.Subject)
                .Include(fa => fa.ClassSection)
                .Include(fa => fa.AcademicSemester)
                .OrderBy(fa => fa.ClassSection!.Name)
                .ThenBy(fa => fa.Subject!.Code)
                .Select(fa => new FacultyAssignmentViewModel
                {
                    Id = fa.Id,
                    FacultyId = fa.FacultyId,
                    FacultyName = fa.Faculty!.FullName,
                    FacultyCode = fa.Faculty.FacultyCode,
                    SubjectId = fa.SubjectId,
                    SubjectName = fa.Subject!.Name,
                    SubjectCode = fa.Subject.Code,
                    ClassSectionId = fa.ClassSectionId,
                    ClassSectionName = fa.ClassSection!.Name,
                    AcademicSemesterId = fa.AcademicSemesterId,
                    SemesterName = fa.AcademicSemester!.Name,
                    AssignedDate = fa.AssignedDate,
                    IsActive = fa.IsActive
                })
                .ToListAsync();

            ViewBag.FacultyList = new SelectList(await _context.Faculties.Where(f => f.IsActive).OrderBy(f => f.FirstName).ToListAsync(), "Id", "FullName");
            ViewBag.SubjectList = new SelectList(await _context.Subjects.OrderBy(s => s.Code).ToListAsync(), "Id", "Name");
            ViewBag.SectionList = new SelectList(await _context.ClassSections.OrderBy(s => s.Name).ToListAsync(), "Id", "Name");
            ViewBag.SemesterList = new SelectList(await _context.AcademicSemesters.OrderBy(s => s.Name).ToListAsync(), "Id", "Name");

            return View(assignments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignFaculty(FacultyAssignmentViewModel model)
        {
            var exists = await _context.FacultyAssignments
                .AnyAsync(fa => fa.FacultyId == model.FacultyId &&
                                fa.SubjectId == model.SubjectId &&
                                fa.ClassSectionId == model.ClassSectionId &&
                                fa.IsActive);

            if (exists)
            {
                TempData["ErrorMessage"] = "This faculty member is already assigned to this subject and class.";
                return RedirectToAction(nameof(FacultyAssignments));
            }

            var assignment = new FacultyAssignment
            {
                FacultyId = model.FacultyId,
                SubjectId = model.SubjectId,
                ClassSectionId = model.ClassSectionId,
                AcademicSemesterId = model.AcademicSemesterId,
                AssignedDate = DateTime.Today,
                IsActive = true
            };

            _context.FacultyAssignments.Add(assignment);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Faculty assigned successfully.";
            return RedirectToAction(nameof(FacultyAssignments));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAssignment(int id)
        {
            var assignment = await _context.FacultyAssignments.FindAsync(id);
            if (assignment != null)
            {
                _context.FacultyAssignments.Remove(assignment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Faculty assignment removed.";
            }

            return RedirectToAction(nameof(FacultyAssignments));
        }

        // ================= ACADEMIC SEMESTERS =================
        public async Task<IActionResult> Semesters()
        {
            var semesters = await _context.AcademicSemesters
                .OrderByDescending(s => s.StartDate)
                .Select(s => new AcademicSemesterViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    AcademicYear = s.AcademicYear,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    IsCurrent = s.IsCurrent
                })
                .ToListAsync();

            return View(semesters);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSemester(AcademicSemesterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid semester data.";
                return RedirectToAction(nameof(Semesters));
            }

            if (model.IsCurrent)
            {
                var current = await _context.AcademicSemesters.Where(s => s.IsCurrent).ToListAsync();
                foreach (var c in current) c.IsCurrent = false;
            }

            var sem = new AcademicSemester
            {
                Name = model.Name,
                AcademicYear = model.AcademicYear,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                IsCurrent = model.IsCurrent
            };

            _context.AcademicSemesters.Add(sem);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Semester {sem.Name} created.";
            return RedirectToAction(nameof(Semesters));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetCurrentSemester(int id)
        {
            var all = await _context.AcademicSemesters.ToListAsync();
            foreach (var s in all)
            {
                s.IsCurrent = (s.Id == id);
            }
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Active semester updated.";
            return RedirectToAction(nameof(Semesters));
        }
    }
}
