using System;
using System.ComponentModel.DataAnnotations;

namespace SmartAttendance.ViewModels.Management
{
    public class StudentCreateEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Roll Number is required")]
        [StringLength(50)]
        [Display(Name = "Roll Number")]
        public string RollNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Registration Number is required")]
        [StringLength(50)]
        [Display(Name = "Registration / Admission No")]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "First Name is required")]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name is required")]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [Display(Name = "Institutional Email")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Contact Phone")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Please select a Course")]
        [Display(Name = "Enrolled Course")]
        public int CourseId { get; set; }

        [Display(Name = "Class Section")]
        public int? ClassSectionId { get; set; }

        [Range(1, 12)]
        [Display(Name = "Current Semester")]
        public int CurrentSemester { get; set; } = 1;

        [Display(Name = "Gender")]
        public string Gender { get; set; } = "Male";

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Admission Date")]
        public DateTime AdmissionDate { get; set; } = DateTime.Today;

        public bool IsActive { get; set; } = true;

        // Credentials for user account
        [DataType(DataType.Password)]
        [Display(Name = "Account Password")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string? ConfirmPassword { get; set; }
    }
}
