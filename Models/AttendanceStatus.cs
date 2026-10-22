using System.ComponentModel.DataAnnotations;

namespace SmartAttendance.Models
{
    public enum AttendanceStatus
    {
        [Display(Name = "Present")]
        Present = 1,

        [Display(Name = "Absent")]
        Absent = 2,

        [Display(Name = "Late")]
        Late = 3
    }
}
