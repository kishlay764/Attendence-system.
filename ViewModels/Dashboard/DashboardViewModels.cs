using System;
using System.Collections.Generic;

namespace SmartAttendance.ViewModels.Dashboard
{
    public class AdminDashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalFaculty { get; set; }
        public int TotalCourses { get; set; }
        public int TotalClassSections { get; set; }
        public int TotalSubjects { get; set; }
        public double OverallAttendancePercentage { get; set; }
        public int TodaySessionsRecorded { get; set; }
        public int TodayAbsenteesCount { get; set; }
        public int LowAttendanceStudentsCount { get; set; }

        // Chart Data
        public List<string> TrendDates { get; set; } = new List<string>();
        public List<double> TrendPercentages { get; set; } = new List<double>();
        public List<string> SubjectLabels { get; set; } = new List<string>();
        public List<double> SubjectAverages { get; set; } = new List<double>();

        // Activity Feed
        public List<RecentActivityItem> RecentActivities { get; set; } = new List<RecentActivityItem>();
        public List<LowAttendanceStudentItem> TopDefaulters { get; set; } = new List<LowAttendanceStudentItem>();
    }

    public class FacultyDashboardViewModel
    {
        public string FacultyName { get; set; } = string.Empty;
        public string FacultyCode { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int AssignedClassesCount { get; set; }
        public int AssignedSubjectsCount { get; set; }
        public int TotalEnrolledStudents { get; set; }
        public int TodayClassesCount { get; set; }

        public List<FacultyClassScheduleItem> AssignedClasses { get; set; } = new List<FacultyClassScheduleItem>();
        public List<RecentActivityItem> RecentSubmissions { get; set; } = new List<RecentActivityItem>();
        public List<FacultySubjectSummaryItem> SubjectSummaries { get; set; } = new List<FacultySubjectSummaryItem>();
    }

    public class StudentDashboardViewModel
    {
        public string StudentName { get; set; } = string.Empty;
        public string RollNumber { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string ClassSectionName { get; set; } = string.Empty;
        public int Semester { get; set; }

        public double OverallPercentage { get; set; }
        public bool HasLowAttendanceWarning => OverallPercentage < 75.0 && TotalSessionsHeld > 0;
        public int TotalSessionsHeld { get; set; }
        public int TotalPresent { get; set; }
        public int TotalAbsent { get; set; }
        public int TotalLate { get; set; }

        public List<StudentSubjectAttendanceItem> SubjectBreakdowns { get; set; } = new List<StudentSubjectAttendanceItem>();
        public List<StudentDailyAttendanceItem> RecentAttendanceHistory { get; set; } = new List<StudentDailyAttendanceItem>();
        public List<string> MonthlyLabels { get; set; } = new List<string>();
        public List<double> MonthlyPercentages { get; set; } = new List<double>();
    }

    public class RecentActivityItem
    {
        public DateTime Date { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string ClassSection { get; set; } = string.Empty;
        public string FacultyName { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public double Percentage { get; set; }
    }

    public class LowAttendanceStudentItem
    {
        public int StudentId { get; set; }
        public string RollNumber { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string ClassSection { get; set; } = string.Empty;
        public int TotalSessions { get; set; }
        public int AttendedSessions { get; set; }
        public double AttendancePercentage { get; set; }
    }

    public class FacultyClassScheduleItem
    {
        public int AssignmentId { get; set; }
        public int SubjectId { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int ClassSectionId { get; set; }
        public string ClassSectionName { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public int EnrolledStudentCount { get; set; }
        public bool IsMarkedToday { get; set; }
    }

    public class FacultySubjectSummaryItem
    {
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string ClassSectionName { get; set; } = string.Empty;
        public int TotalSessions { get; set; }
        public double AverageAttendance { get; set; }
    }

    public class StudentSubjectAttendanceItem
    {
        public int SubjectId { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string FacultyName { get; set; } = string.Empty;
        public int TotalSessions { get; set; }
        public int PresentSessions { get; set; }
        public int AbsentSessions { get; set; }
        public int LateSessions { get; set; }
        public double Percentage { get; set; }
        public bool IsLow => Percentage < 75.0 && TotalSessions > 0;
    }

    public class StudentDailyAttendanceItem
    {
        public DateTime Date { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Present, Absent, Late
        public string? Remarks { get; set; }
        public string MarkedBy { get; set; } = string.Empty;
    }
}
