using System;
using Microsoft.AspNetCore.Identity;

namespace SmartAttendance.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty; // "Admin", "Faculty", "Student"
        public string? AvatarPath { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual Faculty? FacultyProfile { get; set; }
        public virtual Student? StudentProfile { get; set; }
    }
}
