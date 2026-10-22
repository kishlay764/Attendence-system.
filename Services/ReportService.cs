using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Data;
using SmartAttendance.Models;
using SmartAttendance.ViewModels.Attendance;
using SmartAttendance.ViewModels.Reports;

namespace SmartAttendance.Services
{
    public class ReportService : IReportService
    {
        private const double LateWeight = 0.5;
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DailyAttendanceReportViewModel> GetDailyReportAsync(DateTime date, int? classSectionId = null)
        {
            var dateOnly = date.Date;

            var query = _context.AttendanceRecords
                .Include(a => a.Subject)
                .Include(a => a.ClassSection)
                .Include(a => a.RecordedByFaculty)
                .Where(a => a.Date.Date == dateOnly);

            if (classSectionId.HasValue && classSectionId > 0)
            {
                query = query.Where(a => a.ClassSectionId == classSectionId.Value);
            }

            var records = await query.ToListAsync();

            var sectionName = classSectionId.HasValue
                ? (await _context.ClassSections.FindAsync(classSectionId.Value))?.Name
                : "All Class Sections";

            var grouped = records.GroupBy(r => new { r.SubjectId, r.ClassSectionId }).ToList();
            var sessions = new List<DailySessionReportItem>();

            int totalEnrolled = 0;
            int totalPresent = 0;
            int totalAbsent = 0;
            int totalLate = 0;

            foreach (var g in grouped)
            {
                var s = g.First();
                var tot = g.Count();
                var p = g.Count(x => x.Status == AttendanceStatus.Present);
                var a = g.Count(x => x.Status == AttendanceStatus.Absent);
                var l = g.Count(x => x.Status == AttendanceStatus.Late);
                var pct = tot > 0 ? Math.Round(((p + (l * LateWeight)) / tot) * 100.0, 1) : 0;

                totalEnrolled += tot;
                totalPresent += p;
                totalAbsent += a;
                totalLate += l;

                sessions.Add(new DailySessionReportItem
                {
                    SubjectId = s.SubjectId,
                    SubjectCode = s.Subject?.Code ?? "",
                    SubjectName = s.Subject?.Name ?? "",
                    ClassSectionId = s.ClassSectionId,
                    ClassSectionName = s.ClassSection?.Name ?? "",
                    FacultyName = s.RecordedByFaculty?.FullName ?? "Assigned Faculty",
                    TotalEnrolled = tot,
                    PresentCount = p,
                    AbsentCount = a,
                    LateCount = l,
                    AttendancePercentage = pct
                });
            }

            double overall = totalEnrolled > 0
                ? Math.Round(((totalPresent + (totalLate * LateWeight)) / totalEnrolled) * 100.0, 1)
                : 0.0;

            return new DailyAttendanceReportViewModel
            {
                Date = dateOnly,
                ClassSectionId = classSectionId,
                ClassSectionName = sectionName,
                TotalRecordedSessions = sessions.Count,
                TotalStudentsCount = totalEnrolled,
                TotalPresentCount = totalPresent,
                TotalAbsentCount = totalAbsent,
                TotalLateCount = totalLate,
                OverallPercentage = overall,
                Sessions = sessions
            };
        }

        public async Task<SubjectWiseReportViewModel> GetSubjectWiseReportAsync(
            int? subjectId, int? classSectionId, DateTime? start, DateTime? end)
        {
            var model = new SubjectWiseReportViewModel
            {
                SubjectId = subjectId,
                ClassSectionId = classSectionId,
                StartDate = start,
                EndDate = end
            };

            if (!subjectId.HasValue || subjectId <= 0)
            {
                return model;
            }

            var subject = await _context.Subjects.FindAsync(subjectId.Value);
            model.SubjectName = subject?.Name;
            model.SubjectCode = subject?.Code;

            if (classSectionId.HasValue && classSectionId > 0)
            {
                var sec = await _context.ClassSections.FindAsync(classSectionId.Value);
                model.ClassSectionName = sec?.Name;
            }

            var query = _context.AttendanceRecords
                .Include(a => a.Student)
                .Where(a => a.SubjectId == subjectId.Value);

            if (classSectionId.HasValue && classSectionId > 0)
                query = query.Where(a => a.ClassSectionId == classSectionId.Value);

            if (start.HasValue)
                query = query.Where(a => a.Date.Date >= start.Value.Date);

            if (end.HasValue)
                query = query.Where(a => a.Date.Date <= end.Value.Date);

            var records = await query.ToListAsync();

            model.TotalSessionsConducted = records.Select(r => r.Date.Date).Distinct().Count();

            var studentGroups = records.GroupBy(r => r.StudentId);
            var studentRows = new List<SubjectStudentAttendanceRow>();

            double totalPctSum = 0;
            foreach (var g in studentGroups)
            {
                var s = g.First();
                var tot = g.Count();
                var p = g.Count(x => x.Status == AttendanceStatus.Present);
                var a = g.Count(x => x.Status == AttendanceStatus.Absent);
                var l = g.Count(x => x.Status == AttendanceStatus.Late);
                var pct = tot > 0 ? Math.Round(((p + (l * LateWeight)) / tot) * 100.0, 1) : 0;
                totalPctSum += pct;

                studentRows.Add(new SubjectStudentAttendanceRow
                {
                    StudentId = g.Key,
                    RollNumber = s.Student?.RollNumber ?? "",
                    StudentName = s.Student?.FullName ?? "",
                    TotalClassesHeld = tot,
                    PresentCount = p,
                    AbsentCount = a,
                    LateCount = l,
                    AttendancePercentage = pct
                });
            }

            model.StudentRows = studentRows.OrderBy(r => r.RollNumber).ToList();
            model.AverageAttendanceRate = studentRows.Any()
                ? Math.Round(totalPctSum / studentRows.Count, 1)
                : 0.0;

            return model;
        }

        public async Task<StudentWiseReportViewModel> GetStudentWiseReportAsync(
            int studentId, DateTime? start, DateTime? end)
        {
            var student = await _context.Students
                .Include(s => s.Course)
                .Include(s => s.ClassSection)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null) return new StudentWiseReportViewModel();

            var query = _context.AttendanceRecords
                .Include(a => a.Subject)
                .Include(a => a.RecordedByFaculty)
                .Where(a => a.StudentId == studentId);

            if (start.HasValue)
                query = query.Where(a => a.Date.Date >= start.Value.Date);

            if (end.HasValue)
                query = query.Where(a => a.Date.Date <= end.Value.Date);

            var records = await query.OrderByDescending(a => a.Date).ToListAsync();

            var totOverall = records.Count;
            var pOverall = records.Count(r => r.Status == AttendanceStatus.Present);
            var aOverall = records.Count(r => r.Status == AttendanceStatus.Absent);
            var lOverall = records.Count(r => r.Status == AttendanceStatus.Late);
            var pctOverall = totOverall > 0
                ? Math.Round(((pOverall + (lOverall * LateWeight)) / totOverall) * 100.0, 1)
                : 0.0;

            // Subject level stats
            var subjectGroups = records.GroupBy(r => r.SubjectId);
            var subjectStats = new List<StudentSubjectRow>();

            foreach (var g in subjectGroups)
            {
                var sample = g.First();
                var tot = g.Count();
                var p = g.Count(x => x.Status == AttendanceStatus.Present);
                var a = g.Count(x => x.Status == AttendanceStatus.Absent);
                var l = g.Count(x => x.Status == AttendanceStatus.Late);
                var pct = tot > 0 ? Math.Round(((p + (l * LateWeight)) / tot) * 100.0, 1) : 0;

                subjectStats.Add(new StudentSubjectRow
                {
                    SubjectCode = sample.Subject?.Code ?? "",
                    SubjectName = sample.Subject?.Name ?? "",
                    TotalHeld = tot,
                    Present = p,
                    Absent = a,
                    Late = l,
                    Percentage = pct
                });
            }

            var detailed = records.Select(r => new StudentDetailedRecordRow
            {
                Date = r.Date,
                SubjectCode = r.Subject?.Code ?? "",
                SubjectName = r.Subject?.Name ?? "",
                Status = r.Status,
                Remarks = r.Remarks,
                FacultyName = r.RecordedByFaculty?.FullName ?? "Assigned Faculty"
            }).ToList();

            return new StudentWiseReportViewModel
            {
                StudentId = student.Id,
                RollNumber = student.RollNumber,
                StudentName = student.FullName,
                CourseName = student.Course?.Name ?? "",
                ClassSectionName = student.ClassSection?.Name ?? "",
                CurrentSemester = student.CurrentSemester,
                StartDate = start,
                EndDate = end,
                TotalClassesOverall = totOverall,
                TotalPresentOverall = pOverall,
                TotalAbsentOverall = aOverall,
                TotalLateOverall = lOverall,
                OverallPercentage = pctOverall,
                SubjectStats = subjectStats,
                DetailedRecords = detailed
            };
        }

        public async Task<LowAttendanceReportViewModel> GetLowAttendanceReportAsync(
            double threshold = 75.0, int? classSectionId = null, int? subjectId = null)
        {
            var model = new LowAttendanceReportViewModel
            {
                Threshold = threshold,
                ClassSectionId = classSectionId,
                SubjectId = subjectId
            };

            var query = _context.AttendanceRecords
                .Include(a => a.Student)
                    .ThenInclude(s => s!.ClassSection)
                .Include(a => a.Subject)
                .AsQueryable();

            if (classSectionId.HasValue && classSectionId > 0)
                query = query.Where(a => a.ClassSectionId == classSectionId.Value);

            if (subjectId.HasValue && subjectId > 0)
                query = query.Where(a => a.SubjectId == subjectId.Value);

            var records = await query.ToListAsync();

            // Group by student and subject
            var groups = records.GroupBy(r => new { r.StudentId, r.SubjectId });
            var defaulters = new List<LowAttendanceReportItem>();

            foreach (var g in groups)
            {
                var first = g.First();
                var tot = g.Count();
                var p = g.Count(x => x.Status == AttendanceStatus.Present);
                var a = g.Count(x => x.Status == AttendanceStatus.Absent);
                var l = g.Count(x => x.Status == AttendanceStatus.Late);
                var attendedCredit = p + (l * LateWeight);
                var pct = tot > 0 ? Math.Round((attendedCredit / tot) * 100.0, 1) : 0;

                if (pct < threshold && tot > 0)
                {
                    // Formula for consecutive classes needed to reach threshold:
                    // (attendedCredit + x) / (tot + x) >= threshold / 100
                    // attendedCredit + x >= (threshold/100)*tot + (threshold/100)*x
                    // x * (1 - threshold/100) >= (threshold/100)*tot - attendedCredit
                    var targetRatio = threshold / 100.0;
                    var numerator = (targetRatio * tot) - attendedCredit;
                    var denominator = 1.0 - targetRatio;
                    int needed = 0;
                    if (numerator > 0 && denominator > 0)
                    {
                        needed = (int)Math.Ceiling(numerator / denominator);
                    }

                    defaulters.Add(new LowAttendanceReportItem
                    {
                        StudentId = first.StudentId,
                        RollNumber = first.Student?.RollNumber ?? "",
                        StudentName = first.Student?.FullName ?? "",
                        Email = first.Student?.Email ?? "",
                        Phone = first.Student?.Phone,
                        ClassSectionName = first.Student?.ClassSection?.Name ?? "Section",
                        SubjectCode = first.Subject?.Code ?? "",
                        SubjectName = first.Subject?.Name ?? "",
                        TotalSessions = tot,
                        PresentSessions = p,
                        AbsentSessions = a,
                        LateSessions = l,
                        AttendancePercentage = pct,
                        ClassesNeededToReachThreshold = needed
                    });
                }
            }

            model.Defaulters = defaulters.OrderBy(d => d.AttendancePercentage).ToList();
            return model;
        }

        public byte[] ExportAttendanceCsv(IEnumerable<AttendanceRecordItemViewModel> records)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Record ID,Date,Student Roll No,Student Name,Class Section,Subject Code,Subject Name,Status,Remarks,Recorded By,Created At");

            foreach (var r in records)
            {
                sb.AppendLine($"{r.Id},\"{r.Date:yyyy-MM-dd}\",\"{EscapeCsv(r.StudentRollNumber)}\",\"{EscapeCsv(r.StudentName)}\",\"{EscapeCsv(r.ClassSectionName)}\",\"{EscapeCsv(r.SubjectCode)}\",\"{EscapeCsv(r.SubjectName)}\",\"{r.Status}\",\"{EscapeCsv(r.Remarks ?? "")}\",\"{EscapeCsv(r.RecordedByFaculty)}\",\"{r.CreatedAt:yyyy-MM-dd HH:mm}\"");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public byte[] ExportLowAttendanceCsv(LowAttendanceReportViewModel report)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Low Attendance Defaulters Report - Threshold: {report.Threshold}% Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
            sb.AppendLine("Roll Number,Student Name,Email,Phone,Class Section,Subject Code,Subject Name,Total Sessions,Present,Absent,Late,Attendance %,Classes Needed for 75%");

            foreach (var d in report.Defaulters)
            {
                sb.AppendLine($"\"{EscapeCsv(d.RollNumber)}\",\"{EscapeCsv(d.StudentName)}\",\"{EscapeCsv(d.Email)}\",\"{EscapeCsv(d.Phone ?? "")}\",\"{EscapeCsv(d.ClassSectionName)}\",\"{EscapeCsv(d.SubjectCode)}\",\"{EscapeCsv(d.SubjectName)}\",{d.TotalSessions},{d.PresentSessions},{d.AbsentSessions},{d.LateSessions},{d.AttendancePercentage}%,{d.ClassesNeededToReachThreshold}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private static string EscapeCsv(string value)
        {
            return value.Replace("\"", "\"\"");
        }
    }
}
