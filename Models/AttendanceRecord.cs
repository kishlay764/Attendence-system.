using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAttendance.Models
{
    public class AttendanceRecord
    {
        public long Id { get; set; }

        [Required]
        [Display(Name = "Student")]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student? Student { get; set; }

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
        [DataType(DataType.Date)]
        [Display(Name = "Attendance Date")]
        public DateTime Date { get; set; }

        [Required]
        [Display(Name = "Status")]
        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

        [StringLength(250)]
        public string? Remarks { get; set; }

        // Audit information
        [Display(Name = "Recorded By")]
        public int? RecordedByFacultyId { get; set; }

        [ForeignKey("RecordedByFacultyId")]
        public virtual Faculty? RecordedByFaculty { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedAt { get; set; }

        [StringLength(450)]
        public string? ModifiedByUserId { get; set; }

        [ForeignKey("ModifiedByUserId")]
        public virtual ApplicationUser? ModifiedByUser { get; set; }
    }
}
