using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAttendance.Models
{
    public class ClassSection
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Section Name")]
        public string Name { get; set; } = string.Empty; // e.g. "CSE-5A", "Section A"

        [Required]
        [Display(Name = "Course")]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }

        [Required]
        [Display(Name = "Academic Semester")]
        public int AcademicSemesterId { get; set; }

        [ForeignKey("AcademicSemesterId")]
        public virtual AcademicSemester? AcademicSemester { get; set; }

        [StringLength(50)]
        [Display(Name = "Room Number")]
        public string? RoomNumber { get; set; }

        [Range(1, 200)]
        public int Capacity { get; set; } = 60;

        // Navigation
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
        public virtual ICollection<StudentEnrollment> Enrollments { get; set; } = new List<StudentEnrollment>();
        public virtual ICollection<FacultyAssignment> FacultyAssignments { get; set; } = new List<FacultyAssignment>();
        public virtual ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    }
}
