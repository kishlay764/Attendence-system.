using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Data;
using SmartAttendance.Models;
using SmartAttendance.ViewModels.Attendance;
using SmartAttendance.ViewModels.Dashboard;

namespace SmartAttendance.Services
{
    public class AttendanceService : IAttendanceService
    {
        private const double LateWeight = 0.5; // Policy: Late counts as 0.5 present session
        private readonly ApplicationDbContext _context;

        public AttendanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MarkAttendanceViewModel> GetAttendanceSheetAsync(int classSectionId, int subjectId, DateTime date)
        {
            var dateOnly = date.Date;

            var section = await _context.ClassSections
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.Id == classSectionId);

            var subject = await _context.Subjects
                .FirstOrDefaultAsync(s => s.Id == subjectId);

            // Fetch students enrolled in this section or assigned to this section
            var enrolledStudentIds = await _context.StudentEnrollments
                .Where(e => e.ClassSectionId == classSectionId && e.IsActive)
                .Select(e => e.StudentId)
                .ToListAsync();

            // Also check direct ClassSectionId on Student as fallback
            var directStudentIds = await _context.Students
                .Where(s => s.ClassSectionId == classSectionId && s.IsActive)
                .Select(s => s.Id)
                .ToListAsync();

            var allStudentIds = enrolledStudentIds.Union(directStudentIds).Distinct().ToList();

            var students = await _context.Students
                .Where(s => allStudentIds.Contains(s.Id) && s.IsActive)
                .OrderBy(s => s.RollNumber)
                .ToListAsync();

            // Fetch existing attendance records for this date, section, subject
            var existingRecords = await _context.AttendanceRecords
                .Include(a => a.RecordedByFaculty)
                .Where(a => a.ClassSectionId == classSectionId && a.SubjectId == subjectId && a.Date.Date == dateOnly)
                .ToDictionaryAsync(a => a.StudentId);

            var isAlreadyMarked = existingRecords.Any();
            var firstRecord = existingRecords.Values.FirstOrDefault();

            var studentEntries = new List<StudentAttendanceEntryViewModel>();
            foreach (var student in students)
            {
                var hasRecord = existingRecords.TryGetValue(student.Id, out var record);
                
                // Calculate student's overall attendance percentage up to now
                var studentHistory = await _context.AttendanceRecords
                    .Where(a => a.StudentId == student.Id)
                    .Select(a => a.Status)
                    .ToListAsync();

                double overallPct = 0;
                if (studentHistory.Any())
                {
                    var present = studentHistory.Count(s => s == AttendanceStatus.Present);
                    var late = studentHistory.Count(s => s == AttendanceStatus.Late);
                    overallPct = Math.Round(((present + (late * LateWeight)) / studentHistory.Count) * 100.0, 1);
                }

                studentEntries.Add(new StudentAttendanceEntryViewModel
                {
                    StudentId = student.Id,
                    RollNumber = student.RollNumber,
                    StudentName = student.FullName,
                    Status = hasRecord ? record!.Status : AttendanceStatus.Present, // Default to Present for quick convenience
                    Remarks = hasRecord ? record!.Remarks : null,
                    AttendanceRecordId = hasRecord ? record!.Id : null,
                    OverallAttendancePercentage = overallPct
                });
            }

            return new MarkAttendanceViewModel
            {
                ClassSectionId = classSectionId,
                ClassSectionName = section?.Name,
                SubjectId = subjectId,
                SubjectName = subject?.Name,
                SubjectCode = subject?.Code,
                Date = dateOnly,
                IsAlreadyMarked = isAlreadyMarked,
                IsEditMode = isAlreadyMarked,
                RecordedByName = firstRecord?.RecordedByFaculty?.FullName,
                LastModifiedAt = firstRecord?.ModifiedAt,
                Students = studentEntries
            };
        }

        public async Task<(bool Success, string Message, int RecordedCount)> SaveAttendanceAsync(
            AttendanceBatchSubmissionModel model, int? recordedByFacultyId, string userId)
        {
            if (model.Students == null || !model.Students.Any())
            {
                return (false, "No student attendance records were submitted.", 0);
            }

            var dateOnly = model.Date.Date;

            // Load existing records for this (Section, Subject, Date)
            var existingRecords = await _context.AttendanceRecords
                .Where(a => a.ClassSectionId == model.ClassSectionId && a.SubjectId == model.SubjectId && a.Date.Date == dateOnly)
                .ToDictionaryAsync(a => a.StudentId);

            int savedCount = 0;

            foreach (var item in model.Students)
            {
                if (existingRecords.TryGetValue(item.StudentId, out var existing))
                {
                    // Update existing record with audit information
                    existing.Status = item.Status;
                    existing.Remarks = item.Remarks;
                    existing.ModifiedAt = DateTime.UtcNow;
                    existing.ModifiedByUserId = userId;
                }
                else
                {
                    // Create new record
                    var newRecord = new AttendanceRecord
                    {
                        StudentId = item.StudentId,
                        ClassSectionId = model.ClassSectionId,
                        SubjectId = model.SubjectId,
                        Date = dateOnly,
                        Status = item.Status,
                        Remarks = item.Remarks,
                        RecordedByFacultyId = recordedByFacultyId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.AttendanceRecords.Add(newRecord);
                }
                savedCount++;
            }

            await _context.SaveChangesAsync();
            return (true, $"Attendance successfully saved for {savedCount} students.", savedCount);
        }

        public async Task<AttendanceHistoryViewModel> GetAttendanceHistoryAsync(AttendanceHistoryViewModel filter)
        {
            var query = _context.AttendanceRecords
                .Include(a => a.Student)
                .Include(a => a.Subject)
                .Include(a => a.ClassSection)
                .Include(a => a.RecordedByFaculty)
                .AsQueryable();

            if (filter.ClassSectionId.HasValue && filter.ClassSectionId > 0)
                query = query.Where(a => a.ClassSectionId == filter.ClassSectionId.Value);

            if (filter.SubjectId.HasValue && filter.SubjectId > 0)
                query = query.Where(a => a.SubjectId == filter.SubjectId.Value);

            if (filter.StudentId.HasValue && filter.StudentId > 0)
                query = query.Where(a => a.StudentId == filter.StudentId.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(a => a.Date.Date >= filter.StartDate.Value.Date);

            if (filter.EndDate.HasValue)
                query = query.Where(a => a.Date.Date <= filter.EndDate.Value.Date);

            if (filter.Status.HasValue)
                query = query.Where(a => a.Status == filter.Status.Value);

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();
                query = query.Where(a => a.Student!.RollNumber.ToLower().Contains(term) ||
                                         a.Student.FirstName.ToLower().Contains(term) ||
                                         a.Student.LastName.ToLower().Contains(term) ||
                                         a.Subject!.Name.ToLower().Contains(term) ||
                                         a.Subject.Code.ToLower().Contains(term));
            }

            filter.TotalRecords = await query.CountAsync();
            var pageSize = filter.PageSize > 0 ? filter.PageSize : 25;
            filter.TotalPages = (int)Math.Ceiling(filter.TotalRecords / (double)pageSize);
            if (filter.CurrentPage < 1) filter.CurrentPage = 1;

            var items = await query
                .OrderByDescending(a => a.Date)
                .ThenBy(a => a.Student!.RollNumber)
                .Skip((filter.CurrentPage - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AttendanceRecordItemViewModel
                {
                    Id = a.Id,
                    Date = a.Date,
                    StudentRollNumber = a.Student!.RollNumber,
                    StudentName = a.Student.FirstName + " " + a.Student.LastName,
                    SubjectCode = a.Subject!.Code,
                    SubjectName = a.Subject.Name,
                    ClassSectionName = a.ClassSection!.Name,
                    Status = a.Status,
                    Remarks = a.Remarks,
                    RecordedByFaculty = a.RecordedByFaculty != null ? a.RecordedByFaculty.FirstName + " " + a.RecordedByFaculty.LastName : "System/Admin",
                    CreatedAt = a.CreatedAt,
                    ModifiedAt = a.ModifiedAt
                })
                .ToListAsync();

            filter.Records = items;
            return filter;
        }

        public async Task<double> CalculateStudentOverallAttendanceAsync(int studentId, int? semesterId = null)
        {
            var records = await _context.AttendanceRecords
                .Where(a => a.StudentId == studentId)
                .Select(a => a.Status)
                .ToListAsync();

            if (!records.Any()) return 0.0;

            var present = records.Count(s => s == AttendanceStatus.Present);
            var late = records.Count(s => s == AttendanceStatus.Late);
            return Math.Round(((present + (late * LateWeight)) / records.Count) * 100.0, 1);
        }

        public async Task<List<StudentSubjectAttendanceItem>> GetStudentSubjectBreakdownsAsync(int studentId, int? semesterId = null)
        {
            var records = await _context.AttendanceRecords
                .Include(a => a.Subject)
                .Include(a => a.RecordedByFaculty)
                .Where(a => a.StudentId == studentId)
                .ToListAsync();

            var grouped = records.GroupBy(r => r.SubjectId);
            var results = new List<StudentSubjectAttendanceItem>();

            foreach (var group in grouped)
            {
                var sample = group.First();
                var total = group.Count();
                var present = group.Count(g => g.Status == AttendanceStatus.Present);
                var late = group.Count(g => g.Status == AttendanceStatus.Late);
                var absent = group.Count(g => g.Status == AttendanceStatus.Absent);

                double pct = total > 0 ? Math.Round(((present + (late * LateWeight)) / total) * 100.0, 1) : 0.0;

                results.Add(new StudentSubjectAttendanceItem
                {
                    SubjectId = group.Key,
                    SubjectCode = sample.Subject?.Code ?? "",
                    SubjectName = sample.Subject?.Name ?? "",
                    FacultyName = sample.RecordedByFaculty?.FullName ?? "Assigned Faculty",
                    TotalSessions = total,
                    PresentSessions = present,
                    AbsentSessions = absent,
                    LateSessions = late,
                    Percentage = pct
                });
            }

            return results;
        }
    }
}
