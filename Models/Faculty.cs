using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAttendance.Models
{
    public class Faculty
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Faculty ID / Code")]
        public string FacultyCode { get; set; } = string.Empty; // e.g. "FAC-CS01"

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
        [StringLength(100)]
        public string Department { get; set; } = "Computer Science & Engineering";

        [Required]
        [StringLength(100)]
        public string Designation { get; set; } = "Assistant Professor"; // Professor, Associate Professor, Assistant Professor, HOD

        [StringLength(150)]
        public string? Qualification { get; set; } = "Ph.D. / M.Tech";

        [DataType(DataType.Date)]
        [Display(Name = "Joining Date")]
        public DateTime JoiningDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual ICollection<FacultyAssignment> Assignments { get; set; } = new List<FacultyAssignment>();
        public virtual ICollection<AttendanceRecord> RecordedAttendances { get; set; } = new List<AttendanceRecord>();
    }
}
