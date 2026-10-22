using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAttendance.Models
{
    public class StudentEnrollment
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Student")]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student? Student { get; set; }

        [Required]
        [Display(Name = "Class Section")]
        public int ClassSectionId { get; set; }

        [ForeignKey("ClassSectionId")]
        public virtual ClassSection? ClassSection { get; set; }

        [Required]
        [Display(Name = "Academic Semester")]
        public int AcademicSemesterId { get; set; }

        [ForeignKey("AcademicSemesterId")]
        public virtual AcademicSemester? AcademicSemester { get; set; }

        [DataType(DataType.Date)]
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}
