using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartAttendance.ViewModels.Attendance;
using SmartAttendance.ViewModels.Reports;

namespace SmartAttendance.Services
{
    public interface IReportService
    {
        Task<DailyAttendanceReportViewModel> GetDailyReportAsync(DateTime date, int? classSectionId = null);
        Task<SubjectWiseReportViewModel> GetSubjectWiseReportAsync(int? subjectId, int? classSectionId, DateTime? start, DateTime? end);
        Task<StudentWiseReportViewModel> GetStudentWiseReportAsync(int studentId, DateTime? start, DateTime? end);
        Task<LowAttendanceReportViewModel> GetLowAttendanceReportAsync(double threshold = 75.0, int? classSectionId = null, int? subjectId = null);
        byte[] ExportAttendanceCsv(IEnumerable<AttendanceRecordItemViewModel> records);
        byte[] ExportLowAttendanceCsv(LowAttendanceReportViewModel report);
    }
}
