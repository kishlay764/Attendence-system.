using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartAttendance.ViewModels.Attendance;
using SmartAttendance.ViewModels.Dashboard;

namespace SmartAttendance.Services
{
    public interface IAttendanceService
    {
        Task<MarkAttendanceViewModel> GetAttendanceSheetAsync(int classSectionId, int subjectId, DateTime date);
        Task<(bool Success, string Message, int RecordedCount)> SaveAttendanceAsync(AttendanceBatchSubmissionModel model, int? recordedByFacultyId, string userId);
        Task<AttendanceHistoryViewModel> GetAttendanceHistoryAsync(AttendanceHistoryViewModel filter);
        Task<double> CalculateStudentOverallAttendanceAsync(int studentId, int? semesterId = null);
        Task<List<StudentSubjectAttendanceItem>> GetStudentSubjectBreakdownsAsync(int studentId, int? semesterId = null);
    }
}
