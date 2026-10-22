using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAttendance.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Roll Number")]
        public string RollNumber { get; set; } = string.Empty; // e.g. "26BCSE01"

        [Required]
        [StringLength(50)]
        [Display(Name = "Registration / Admission No")]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [NotMapped]
        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}".Trim();

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        public string? Phone { get; set; }

        [Required]
        [Display(Name = "Course")]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }

        [Display(Name = "Current Class Section")]
        public int? ClassSectionId { get; set; }

        [ForeignKey("ClassSectionId")]
        public virtual ClassSection? ClassSection { get; set; }

        [Range(1, 12)]
        [Display(Name = "Current Semester")]
        public int CurrentSemester { get; set; } = 5;

        [StringLength(20)]
        public string? Gender { get; set; } = "Male";

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Admission Date")]
        public DateTime AdmissionDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual ICollection<StudentEnrollment> Enrollments { get; set; } = new List<StudentEnrollment>();
        public virtual ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    }
}
