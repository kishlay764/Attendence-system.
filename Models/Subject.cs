using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAttendance.Models
{
    public class Subject
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Subject Code")]
        public string Code { get; set; } = string.Empty; // e.g. "CS501"

        [Required]
        [StringLength(150)]
        [Display(Name = "Subject Name")]
        public string Name { get; set; } = string.Empty; // e.g. "Database Management Systems"

        [Range(1, 10)]
        public int Credits { get; set; } = 4;

        [Range(1, 12)]
        [Display(Name = "Semester")]
        public int SemesterNumber { get; set; } = 1;

        [Required]
        [Display(Name = "Course")]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }

        // Navigation
        public virtual ICollection<FacultyAssignment> FacultyAssignments { get; set; } = new List<FacultyAssignment>();
        public virtual ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    }
}
