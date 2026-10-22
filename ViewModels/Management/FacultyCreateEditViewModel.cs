using System;
using System.ComponentModel.DataAnnotations;

namespace SmartAttendance.ViewModels.Management
{
    public class FacultyCreateEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Faculty Code is required")]
        [StringLength(50)]
        [Display(Name = "Faculty Code / ID")]
        public string FacultyCode { get; set; } = string.Empty;

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
        [Display(Name = "Faculty Email")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Department is required")]
        [StringLength(100)]
        [Display(Name = "Department")]
        public string Department { get; set; } = "Computer Science & Engineering";

        [Required(ErrorMessage = "Designation is required")]
        [StringLength(100)]
        [Display(Name = "Designation")]
        public string Designation { get; set; } = "Assistant Professor";

        [StringLength(150)]
        [Display(Name = "Qualification")]
        public string? Qualification { get; set; } = "Ph.D. / M.Tech";

        [DataType(DataType.Date)]
        [Display(Name = "Joining Date")]
        public DateTime JoiningDate { get; set; } = DateTime.Today;

        public bool IsActive { get; set; } = true;

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
