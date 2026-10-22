using System;
using System.ComponentModel.DataAnnotations;

namespace SmartAttendance.ViewModels.Account
{
    public class ProfileViewModel
    {
        public string UserId { get; set; } = string.Empty;

        [Display(Name = "Full Name")]
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Role")]
        public string Role { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        [Phone]
        public string? PhoneNumber { get; set; }

        // Role-specific info
        public string? Identifier { get; set; } // Roll Number or Faculty Code
        public string? Department { get; set; }
        public string? Designation { get; set; }
        public string? CourseName { get; set; }
        public string? ClassSectionName { get; set; }
        public int? CurrentSemester { get; set; }

        public DateTime MemberSince { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
