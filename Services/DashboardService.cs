using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Data;
using SmartAttendance.Models;
using SmartAttendance.ViewModels.Dashboard;

namespace SmartAttendance.Services
{
    public class DashboardService : IDashboardService
    {
        private const double LateWeight = 0.5;
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardViewModel> GetAdminDashboardStatsAsync()
        {
            var totalStudents = await _context.Students.CountAsync(s => s.IsActive);
            var totalFaculty = await _context.Faculties.CountAsync(f => f.IsActive);
            var totalCourses = await _context.Courses.CountAsync();
            var totalSections = await _context.ClassSections.CountAsync();
            var totalSubjects = await _context.Subjects.CountAsync();

            var today = DateTime.Today;
            var todayRecords = await _context.AttendanceRecords
                .Where(a => a.Date.Date == today)
                .ToListAsync();

            var todaySessions = todayRecords
                .Select(a => new { a.ClassSectionId, a.SubjectId })
                .Distinct()
                .Count();

            var todayAbsentees = todayRecords.Count(a => a.Status == AttendanceStatus.Absent);

            // Overall attendance stats
            var allRecords = await _context.AttendanceRecords
                .Select(a => a.Status)
                .ToListAsync();

            double overallPct = 0;
            if (allRecords.Any())
            {
                var present = allRecords.Count(s => s == AttendanceStatus.Present);
                var late = allRecords.Count(s => s == AttendanceStatus.Late);
                overallPct = Math.Round(((present + (late * LateWeight)) / allRecords.Count) * 100.0, 1);
            }

            // Low attendance students calculation
            var studentStats = await _context.AttendanceRecords
                .GroupBy(a => a.StudentId)
                .Select(g => new
                {
                    StudentId = g.Key,
                    Total = g.Count(),
                    Present = g.Count(x => x.Status == AttendanceStatus.Present),
                    Late = g.Count(x => x.Status == AttendanceStatus.Late)
                })
                .ToListAsync();

            int lowCount = 0;
            var defaulterList = new List<LowAttendanceStudentItem>();

            foreach (var st in studentStats)
            {
                double pct = st.Total > 0 ? Math.Round(((st.Present + (st.Late * LateWeight)) / st.Total) * 100.0, 1) : 0;
                if (pct < 75.0 && st.Total > 0)
                {
                    lowCount++;
                }
            }

            // Top defaulters
            var lowStudents = studentStats
                .Select(st => new
                {
                    st.StudentId,
                    st.Total,
                    Attended = st.Present,
                    Pct = st.Total > 0 ? Math.Round(((st.Present + (st.Late * LateWeight)) / st.Total) * 100.0, 1) : 0
                })
                .Where(x => x.Pct < 75.0 && x.Total > 0)
                .OrderBy(x => x.Pct)
                .Take(5)
                .ToList();

            if (lowStudents.Any())
            {
                var lowIds = lowStudents.Select(x => x.StudentId).ToList();
                var studentInfo = await _context.Students
                    .Include(s => s.ClassSection)
                    .Where(s => lowIds.Contains(s.Id))
                    .ToDictionaryAsync(s => s.Id);

                foreach (var ls in lowStudents)
                {
                    if (studentInfo.TryGetValue(ls.StudentId, out var si))
                    {
                        defaulterList.Add(new LowAttendanceStudentItem
                        {
                            StudentId = ls.StudentId,
                            RollNumber = si.RollNumber,
                            StudentName = si.FullName,
                            ClassSection = si.ClassSection?.Name ?? "Unassigned",
                            TotalSessions = ls.Total,
                            AttendedSessions = ls.Attended,
                            AttendancePercentage = ls.Pct
                        });
                    }
                }
            }

            // Daily trend for past 7 recorded days
            var recentDates = await _context.AttendanceRecords
                .Select(a => a.Date.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .Take(7)
                .OrderBy(d => d)
                .ToListAsync();

            var trendDates = new List<string>();
            var trendPercentages = new List<double>();

            foreach (var d in recentDates)
            {
                var dayRecords = await _context.AttendanceRecords
                    .Where(a => a.Date.Date == d)
                    .Select(a => a.Status)
                    .ToListAsync();

                if (dayRecords.Any())
                {
                    var p = dayRecords.Count(s => s == AttendanceStatus.Present);
                    var l = dayRecords.Count(s => s == AttendanceStatus.Late);
                    var dayPct = Math.Round(((p + (l * LateWeight)) / dayRecords.Count) * 100.0, 1);
                    trendDates.Add(d.ToString("dd MMM"));
                    trendPercentages.Add(dayPct);
                }
            }

            // Subject-wise averages
            var subjects = await _context.Subjects
                .Include(s => s.AttendanceRecords)
                .Where(s => s.AttendanceRecords.Any())
                .Take(6)
                .ToListAsync();

            var subjectLabels = new List<string>();
            var subjectAverages = new List<double>();

            foreach (var sub in subjects)
            {
                var total = sub.AttendanceRecords.Count;
                var p = sub.AttendanceRecords.Count(a => a.Status == AttendanceStatus.Present);
                var l = sub.AttendanceRecords.Count(a => a.Status == AttendanceStatus.Late);
                var avg = total > 0 ? Math.Round(((p + (l * LateWeight)) / total) * 100.0, 1) : 0;
                subjectLabels.Add(sub.Code);
                subjectAverages.Add(avg);
            }

            // Recent activity feed (group by Date, Subject, ClassSection)
            var recentRecords = await _context.AttendanceRecords
                .Include(a => a.Subject)
                .Include(a => a.ClassSection)
                .Include(a => a.RecordedByFaculty)
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.CreatedAt)
                .Take(100)
                .ToListAsync();

            var recentActivities = recentRecords
                .GroupBy(r => new { r.Date.Date, r.SubjectId, r.ClassSectionId })
                .Take(6)
                .Select(g =>
                {
                    var sample = g.First();
                    var tot = g.Count();
                    var p = g.Count(x => x.Status == AttendanceStatus.Present);
                    var a = g.Count(x => x.Status == AttendanceStatus.Absent);
                    var l = g.Count(x => x.Status == AttendanceStatus.Late);
                    var pct = tot > 0 ? Math.Round(((p + (l * LateWeight)) / tot) * 100.0, 1) : 0;

                    return new RecentActivityItem
                    {
                        Date = sample.Date,
                        SubjectCode = sample.Subject?.Code ?? "",
                        SubjectName = sample.Subject?.Name ?? "",
                        ClassSection = sample.ClassSection?.Name ?? "",
                        FacultyName = sample.RecordedByFaculty?.FullName ?? "Faculty",
                        TotalStudents = tot,
                        PresentCount = p,
                        AbsentCount = a,
                        LateCount = l,
                        Percentage = pct
                    };
                })
                .ToList();

            return new AdminDashboardViewModel
            {
                TotalStudents = totalStudents,
                TotalFaculty = totalFaculty,
                TotalCourses = totalCourses,
                TotalClassSections = totalSections,
                TotalSubjects = totalSubjects,
                OverallAttendancePercentage = overallPct,
                TodaySessionsRecorded = todaySessions,
                TodayAbsenteesCount = todayAbsentees,
                LowAttendanceStudentsCount = lowCount,
                TrendDates = trendDates,
                TrendPercentages = trendPercentages,
                SubjectLabels = subjectLabels,
                SubjectAverages = subjectAverages,
                RecentActivities = recentActivities,
                TopDefaulters = defaulterList
            };
        }

        public async Task<FacultyDashboardViewModel> GetFacultyDashboardStatsAsync(int facultyId)
        {
            var faculty = await _context.Faculties
                .Include(f => f.Assignments)
                    .ThenInclude(a => a.Subject)
                .Include(f => f.Assignments)
                    .ThenInclude(a => a.ClassSection)
                .FirstOrDefaultAsync(f => f.Id == facultyId);

            if (faculty == null) return new FacultyDashboardViewModel();

            var assignments = faculty.Assignments.Where(a => a.IsActive).ToList();
            var classSectionIds = assignments.Select(a => a.ClassSectionId).Distinct().ToList();

            var totalStudents = await _context.StudentEnrollments
                .Where(e => classSectionIds.Contains(e.ClassSectionId) && e.IsActive)
                .Select(e => e.StudentId)
                .Distinct()
                .CountAsync();

            var today = DateTime.Today;
            var assignedCards = new List<FacultyClassScheduleItem>();

            foreach (var item in assignments)
            {
                var isMarkedToday = await _context.AttendanceRecords
                    .AnyAsync(a => a.ClassSectionId == item.ClassSectionId && a.SubjectId == item.SubjectId && a.Date.Date == today);

                var enrolledCount = await _context.StudentEnrollments
                    .CountAsync(e => e.ClassSectionId == item.ClassSectionId && e.IsActive);

                if (enrolledCount == 0)
                {
                    enrolledCount = await _context.Students
                        .CountAsync(s => s.ClassSectionId == item.ClassSectionId && s.IsActive);
                }

                assignedCards.Add(new FacultyClassScheduleItem
                {
                    AssignmentId = item.Id,
                    SubjectId = item.SubjectId,
                    SubjectCode = item.Subject?.Code ?? "",
                    SubjectName = item.Subject?.Name ?? "",
                    ClassSectionId = item.ClassSectionId,
                    ClassSectionName = item.ClassSection?.Name ?? "",
                    RoomNumber = item.ClassSection?.RoomNumber ?? "TBA",
                    EnrolledStudentCount = enrolledCount,
                    IsMarkedToday = isMarkedToday
                });
            }

            // Subject summaries for this faculty
            var subjectSummaries = new List<FacultySubjectSummaryItem>();
            foreach (var item in assignments)
            {
                var records = await _context.AttendanceRecords
                    .Where(a => a.ClassSectionId == item.ClassSectionId && a.SubjectId == item.SubjectId)
                    .ToListAsync();

                var sessions = records.Select(r => r.Date.Date).Distinct().Count();
                double avgPct = 0;
                if (records.Any())
                {
                    var p = records.Count(r => r.Status == AttendanceStatus.Present);
                    var l = records.Count(r => r.Status == AttendanceStatus.Late);
                    avgPct = Math.Round(((p + (l * LateWeight)) / records.Count) * 100.0, 1);
                }

                subjectSummaries.Add(new FacultySubjectSummaryItem
                {
                    SubjectCode = item.Subject?.Code ?? "",
                    SubjectName = item.Subject?.Name ?? "",
                    ClassSectionName = item.ClassSection?.Name ?? "",
                    TotalSessions = sessions,
                    AverageAttendance = avgPct
                });
            }

            // Recent submissions
            var recentRecords = await _context.AttendanceRecords
                .Include(a => a.Subject)
                .Include(a => a.ClassSection)
                .Where(a => a.RecordedByFacultyId == facultyId)
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.CreatedAt)
                .Take(50)
                .ToListAsync();

            var recentSubmissions = recentRecords
                .GroupBy(r => new { r.Date.Date, r.SubjectId, r.ClassSectionId })
                .Take(5)
                .Select(g =>
                {
                    var s = g.First();
                    var tot = g.Count();
                    var p = g.Count(x => x.Status == AttendanceStatus.Present);
                    var a = g.Count(x => x.Status == AttendanceStatus.Absent);
                    var l = g.Count(x => x.Status == AttendanceStatus.Late);
                    var pct = tot > 0 ? Math.Round(((p + (l * LateWeight)) / tot) * 100.0, 1) : 0;

                    return new RecentActivityItem
                    {
                        Date = s.Date,
                        SubjectCode = s.Subject?.Code ?? "",
                        SubjectName = s.Subject?.Name ?? "",
                        ClassSection = s.ClassSection?.Name ?? "",
                        FacultyName = faculty.FullName,
                        TotalStudents = tot,
                        PresentCount = p,
                        AbsentCount = a,
                        LateCount = l,
                        Percentage = pct
                    };
                })
                .ToList();

            return new FacultyDashboardViewModel
            {
                FacultyName = faculty.FullName,
                FacultyCode = faculty.FacultyCode,
                Department = faculty.Department,
                AssignedClassesCount = assignments.Select(a => a.ClassSectionId).Distinct().Count(),
                AssignedSubjectsCount = assignments.Select(a => a.SubjectId).Distinct().Count(),
                TotalEnrolledStudents = totalStudents,
                TodayClassesCount = assignments.Count,
                AssignedClasses = assignedCards,
                SubjectSummaries = subjectSummaries,
                RecentSubmissions = recentSubmissions
            };
        }

        public async Task<StudentDashboardViewModel> GetStudentDashboardStatsAsync(int studentId)
        {
            var student = await _context.Students
                .Include(s => s.Course)
                .Include(s => s.ClassSection)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null) return new StudentDashboardViewModel();

            var records = await _context.AttendanceRecords
                .Include(a => a.Subject)
                .Include(a => a.RecordedByFaculty)
                .Where(a => a.StudentId == studentId)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            var totalSessions = records.Count;
            var presentSessions = records.Count(r => r.Status == AttendanceStatus.Present);
            var absentSessions = records.Count(r => r.Status == AttendanceStatus.Absent);
            var lateSessions = records.Count(r => r.Status == AttendanceStatus.Late);

            double overallPct = totalSessions > 0
                ? Math.Round(((presentSessions + (lateSessions * LateWeight)) / totalSessions) * 100.0, 1)
                : 0.0;

            // Subject breakdowns
            var subjectGroups = records.GroupBy(r => r.SubjectId);
            var subjectBreakdowns = new List<StudentSubjectAttendanceItem>();

            foreach (var group in subjectGroups)
            {
                var s = group.First();
                var tot = group.Count();
                var p = group.Count(g => g.Status == AttendanceStatus.Present);
                var a = group.Count(g => g.Status == AttendanceStatus.Absent);
                var l = group.Count(g => g.Status == AttendanceStatus.Late);
                var pct = tot > 0 ? Math.Round(((p + (l * LateWeight)) / tot) * 100.0, 1) : 0;

                subjectBreakdowns.Add(new StudentSubjectAttendanceItem
                {
                    SubjectId = group.Key,
                    SubjectCode = s.Subject?.Code ?? "",
                    SubjectName = s.Subject?.Name ?? "",
                    FacultyName = s.RecordedByFaculty?.FullName ?? "Assigned Faculty",
                    TotalSessions = tot,
                    PresentSessions = p,
                    AbsentSessions = a,
                    LateSessions = l,
                    Percentage = pct
                });
            }

            // Recent history
            var recentHistory = records.Take(10).Select(r => new StudentDailyAttendanceItem
            {
                Date = r.Date,
                SubjectCode = r.Subject?.Code ?? "",
                SubjectName = r.Subject?.Name ?? "",
                Status = r.Status.ToString(),
                Remarks = r.Remarks,
                MarkedBy = r.RecordedByFaculty?.FullName ?? "Faculty"
            }).ToList();

            // Monthly trends for student
            var monthly = records
                .GroupBy(r => new { r.Date.Year, r.Date.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Take(6)
                .ToList();

            var monthlyLabels = new List<string>();
            var monthlyPcts = new List<double>();

            foreach (var m in monthly)
            {
                var dt = new DateTime(m.Key.Year, m.Key.Month, 1);
                monthlyLabels.Add(dt.ToString("MMM yyyy"));
                var tot = m.Count();
                var p = m.Count(x => x.Status == AttendanceStatus.Present);
                var l = m.Count(x => x.Status == AttendanceStatus.Late);
                var pct = tot > 0 ? Math.Round(((p + (l * LateWeight)) / tot) * 100.0, 1) : 0;
                monthlyPcts.Add(pct);
            }

            return new StudentDashboardViewModel
            {
                StudentName = student.FullName,
                RollNumber = student.RollNumber,
                RegistrationNumber = student.RegistrationNumber,
                CourseName = student.Course?.Name ?? "Course",
                ClassSectionName = student.ClassSection?.Name ?? "Section",
                Semester = student.CurrentSemester,
                OverallPercentage = overallPct,
                TotalSessionsHeld = totalSessions,
                TotalPresent = presentSessions,
                TotalAbsent = absentSessions,
                TotalLate = lateSessions,
                SubjectBreakdowns = subjectBreakdowns,
                RecentAttendanceHistory = recentHistory,
                MonthlyLabels = monthlyLabels,
                MonthlyPercentages = monthlyPcts
            };
        }
    }
}
