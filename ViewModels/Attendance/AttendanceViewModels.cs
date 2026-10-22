using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SmartAttendance.Models;

namespace SmartAttendance.ViewModels.Attendance
{
    public class MarkAttendanceViewModel
    {
        [Required(ErrorMessage = "Please select a Class Section")]
        [Display(Name = "Class Section")]
        public int ClassSectionId { get; set; }

        public string? ClassSectionName { get; set; }

        [Required(ErrorMessage = "Please select a Subject")]
        [Display(Name = "Subject")]
        public int SubjectId { get; set; }

        public string? SubjectName { get; set; }
        public string? SubjectCode { get; set; }

        [Required(ErrorMessage = "Please select a Date")]
        [DataType(DataType.Date)]
        [Display(Name = "Attendance Date")]
        public DateTime Date { get; set; } = DateTime.Today;

        public bool IsAlreadyMarked { get; set; }
        public bool IsEditMode { get; set; }
        public string? RecordedByName { get; set; }
        public DateTime? LastModifiedAt { get; set; }

        public List<StudentAttendanceEntryViewModel> Students { get; set; } = new List<StudentAttendanceEntryViewModel>();
    }

    public class StudentAttendanceEntryViewModel
    {
        public int StudentId { get; set; }
        public string RollNumber { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
        public string? Remarks { get; set; }
        public long? AttendanceRecordId { get; set; }
        public double OverallAttendancePercentage { get; set; }
    }

    public class AttendanceBatchSubmissionModel
    {
        [Required]
        public int ClassSectionId { get; set; }

        [Required]
        public int SubjectId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        public List<StudentStatusSubmissionItem> Students { get; set; } = new List<StudentStatusSubmissionItem>();
    }

    public class StudentStatusSubmissionItem
    {
        public int StudentId { get; set; }
        public AttendanceStatus Status { get; set; }
        public string? Remarks { get; set; }
    }

    public class AttendanceHistoryViewModel
    {
        public int? ClassSectionId { get; set; }
        public int? SubjectId { get; set; }
        public int? StudentId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public AttendanceStatus? Status { get; set; }
        public string? SearchTerm { get; set; }

        public List<AttendanceRecordItemViewModel> Records { get; set; } = new List<AttendanceRecordItemViewModel>();

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalRecords { get; set; }
        public int PageSize { get; set; } = 25;
    }

    public class AttendanceRecordItemViewModel
    {
        public long Id { get; set; }
        public DateTime Date { get; set; }
        public string StudentRollNumber { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string ClassSectionName { get; set; } = string.Empty;
        public AttendanceStatus Status { get; set; }
        public string? Remarks { get; set; }
        public string RecordedByFaculty { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
