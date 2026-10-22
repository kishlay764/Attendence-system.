using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartAttendance.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Course Code")]
        public string Code { get; set; } = string.Empty; // e.g. "BTECH-CSE"

        [Required]
        [StringLength(150)]
        [Display(Name = "Course Name")]
        public string Name { get; set; } = string.Empty; // e.g. "B.Tech Computer Science & Engineering"

        [Required]
        [StringLength(100)]
        [Display(Name = "Department")]
        public string Department { get; set; } = string.Empty; // e.g. "Computer Science & Engineering"

        [Range(1, 6)]
        [Display(Name = "Duration (Years)")]
        public int DurationYears { get; set; } = 4;

        [StringLength(500)]
        public string? Description { get; set; }

        // Navigation
        public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();
        public virtual ICollection<ClassSection> ClassSections { get; set; } = new List<ClassSection>();
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    }
}
