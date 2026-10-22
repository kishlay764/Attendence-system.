using System;
using System.Collections.Generic;
using SmartAttendance.Models;

namespace SmartAttendance.ViewModels.Reports
{
    public class DailyAttendanceReportViewModel
    {
        public DateTime Date { get; set; } = DateTime.Today;
        public int? ClassSectionId { get; set; }
        public string? ClassSectionName { get; set; }

        public int TotalRecordedSessions { get; set; }
        public int TotalStudentsCount { get; set; }
        public int TotalPresentCount { get; set; }
        public int TotalAbsentCount { get; set; }
        public int TotalLateCount { get; set; }
        public double OverallPercentage { get; set; }

        public List<DailySessionReportItem> Sessions { get; set; } = new List<DailySessionReportItem>();
    }

    public class DailySessionReportItem
    {
        public int SubjectId { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int ClassSectionId { get; set; }
        public string ClassSectionName { get; set; } = string.Empty;
        public string FacultyName { get; set; } = string.Empty;
        public int TotalEnrolled { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public double AttendancePercentage { get; set; }
    }

    public class SubjectWiseReportViewModel
    {
        public int? SubjectId { get; set; }
        public int? ClassSectionId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string? SubjectName { get; set; }
        public string? SubjectCode { get; set; }
        public string? ClassSectionName { get; set; }
        public int TotalSessionsConducted { get; set; }
        public double AverageAttendanceRate { get; set; }

        public List<SubjectStudentAttendanceRow> StudentRows { get; set; } = new List<SubjectStudentAttendanceRow>();
    }

    public class SubjectStudentAttendanceRow
    {
        public int StudentId { get; set; }
        public string RollNumber { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public int TotalClassesHeld { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public double AttendancePercentage { get; set; }
        public bool IsBelowThreshold => AttendancePercentage < 75.0 && TotalClassesHeld > 0;
    }

    public class StudentWiseReportViewModel
    {
        public int StudentId { get; set; }
        public string RollNumber { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string ClassSectionName { get; set; } = string.Empty;
        public int CurrentSemester { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int TotalClassesOverall { get; set; }
        public int TotalPresentOverall { get; set; }
        public int TotalAbsentOverall { get; set; }
        public int TotalLateOverall { get; set; }
        public double OverallPercentage { get; set; }

        public List<StudentSubjectRow> SubjectStats { get; set; } = new List<StudentSubjectRow>();
        public List<StudentDetailedRecordRow> DetailedRecords { get; set; } = new List<StudentDetailedRecordRow>();
    }

    public class StudentSubjectRow
    {
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int TotalHeld { get; set; }
        public int Present { get; set; }
        public int Absent { get; set; }
        public int Late { get; set; }
        public double Percentage { get; set; }
        public bool IsLow => Percentage < 75.0 && TotalHeld > 0;
    }

    public class StudentDetailedRecordRow
    {
        public DateTime Date { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public AttendanceStatus Status { get; set; }
        public string? Remarks { get; set; }
        public string FacultyName { get; set; } = string.Empty;
    }

    public class LowAttendanceReportViewModel
    {
        public double Threshold { get; set; } = 75.0;
        public int? ClassSectionId { get; set; }
        public int? SubjectId { get; set; }

        public List<LowAttendanceReportItem> Defaulters { get; set; } = new List<LowAttendanceReportItem>();
    }

    public class LowAttendanceReportItem
    {
        public int StudentId { get; set; }
        public string RollNumber { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string ClassSectionName { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int TotalSessions { get; set; }
        public int PresentSessions { get; set; }
        public int AbsentSessions { get; set; }
        public int LateSessions { get; set; }
        public double AttendancePercentage { get; set; }
        public int ClassesNeededToReachThreshold { get; set; } // Viva bonus: how many consecutive classes student must attend to reach 75%
    }
}
