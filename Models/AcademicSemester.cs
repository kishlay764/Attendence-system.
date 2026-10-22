using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartAttendance.Models
{
    public class AcademicSemester
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Semester Name")]
        public string Name { get; set; } = string.Empty; // e.g. "Fall 2026", "Semester 5"

        [Required]
        [StringLength(50)]
        [Display(Name = "Academic Year")]
        public string AcademicYear { get; set; } = string.Empty; // e.g. "2026-2027"

        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Is Current Semester")]
        public bool IsCurrent { get; set; } = false;

        // Navigation
        public virtual ICollection<ClassSection> ClassSections { get; set; } = new List<ClassSection>();
        public virtual ICollection<StudentEnrollment> Enrollments { get; set; } = new List<StudentEnrollment>();
        public virtual ICollection<FacultyAssignment> FacultyAssignments { get; set; } = new List<FacultyAssignment>();
    }
}
