using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAttendance.Models
{
    public class FacultyAssignment
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Faculty")]
        public int FacultyId { get; set; }

        [ForeignKey("FacultyId")]
        public virtual Faculty? Faculty { get; set; }

        [Required]
        [Display(Name = "Subject")]
        public int SubjectId { get; set; }

        [ForeignKey("SubjectId")]
        public virtual Subject? Subject { get; set; }

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
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}
