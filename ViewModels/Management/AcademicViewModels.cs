using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartAttendance.ViewModels.Management
{
    public class CourseViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Course Code is required")]
        [StringLength(20)]
        [Display(Name = "Course Code")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Course Name is required")]
        [StringLength(150)]
        [Display(Name = "Course Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        [StringLength(100)]
        [Display(Name = "Department")]
        public string Department { get; set; } = "Computer Science & Engineering";

        [Range(1, 6)]
        [Display(Name = "Duration (Years)")]
        public int DurationYears { get; set; } = 4;

        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        public int TotalSubjects { get; set; }
        public int TotalStudents { get; set; }
    }

    public class SubjectViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Subject Code is required")]
        [StringLength(20)]
        [Display(Name = "Subject Code")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Subject Name is required")]
        [StringLength(150)]
        [Display(Name = "Subject Name")]
        public string Name { get; set; } = string.Empty;

        [Range(1, 10)]
        [Display(Name = "Credits")]
        public int Credits { get; set; } = 4;

        [Range(1, 12)]
        [Display(Name = "Semester Number")]
        public int SemesterNumber { get; set; } = 1;

        [Required(ErrorMessage = "Please select a Course")]
        [Display(Name = "Course")]
        public int CourseId { get; set; }

        public string? CourseName { get; set; }
    }

    public class ClassSectionViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Section Name is required")]
        [StringLength(50)]
        [Display(Name = "Section Name")]
        public string Name { get; set; } = string.Empty; // e.g. "CSE-5A"

        [Required(ErrorMessage = "Please select a Course")]
        [Display(Name = "Course")]
        public int CourseId { get; set; }

        public string? CourseName { get; set; }

        [Required(ErrorMessage = "Please select an Academic Semester")]
        [Display(Name = "Academic Semester")]
        public int AcademicSemesterId { get; set; }

        public string? SemesterName { get; set; }

        [StringLength(50)]
        [Display(Name = "Room Number")]
        public string? RoomNumber { get; set; }

        [Range(1, 300)]
        [Display(Name = "Student Capacity")]
        public int Capacity { get; set; } = 60;

        public int EnrolledStudentsCount { get; set; }
    }

    public class AcademicSemesterViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Semester Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Academic Year")]
        public string AcademicYear { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(5);

        [Display(Name = "Current Active Semester")]
        public bool IsCurrent { get; set; } = false;
    }

    public class FacultyAssignmentViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a Faculty Member")]
        [Display(Name = "Faculty Member")]
        public int FacultyId { get; set; }

        public string? FacultyName { get; set; }
        public string? FacultyCode { get; set; }

        [Required(ErrorMessage = "Please select a Subject")]
        [Display(Name = "Subject")]
        public int SubjectId { get; set; }

        public string? SubjectName { get; set; }
        public string? SubjectCode { get; set; }

        [Required(ErrorMessage = "Please select a Class Section")]
        [Display(Name = "Class Section")]
        public int ClassSectionId { get; set; }

        public string? ClassSectionName { get; set; }

        [Required(ErrorMessage = "Please select an Academic Semester")]
        [Display(Name = "Academic Semester")]
        public int AcademicSemesterId { get; set; }

        public string? SemesterName { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Assigned Date")]
        public DateTime AssignedDate { get; set; } = DateTime.Today;

        public bool IsActive { get; set; } = true;
    }

    public class EnrollmentManagementViewModel
    {
        public int ClassSectionId { get; set; }
        public string ClassSectionName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string SemesterName { get; set; } = string.Empty;

        public List<EnrolledStudentRowViewModel> EnrolledStudents { get; set; } = new List<EnrolledStudentRowViewModel>();
        public List<AvailableStudentOptionViewModel> AvailableStudents { get; set; } = new List<AvailableStudentOptionViewModel>();
    }

    public class EnrolledStudentRowViewModel
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public string RollNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; }
    }

    public class AvailableStudentOptionViewModel
    {
        public int StudentId { get; set; }
        public string RollNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
