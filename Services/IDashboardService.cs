using System.Threading.Tasks;
using SmartAttendance.ViewModels.Dashboard;

namespace SmartAttendance.Services
{
    public interface IDashboardService
    {
        Task<AdminDashboardViewModel> GetAdminDashboardStatsAsync();
        Task<FacultyDashboardViewModel> GetFacultyDashboardStatsAsync(int facultyId);
        Task<StudentDashboardViewModel> GetStudentDashboardStatsAsync(int studentId);
    }
}
