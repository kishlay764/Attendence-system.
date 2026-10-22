using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Models;

namespace SmartAttendance.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; } = null!;
        public DbSet<Faculty> Faculties { get; set; } = null!;
        public DbSet<Course> Courses { get; set; } = null!;
        public DbSet<Subject> Subjects { get; set; } = null!;
        public DbSet<AcademicSemester> AcademicSemesters { get; set; } = null!;
        public DbSet<ClassSection> ClassSections { get; set; } = null!;
        public DbSet<StudentEnrollment> StudentEnrollments { get; set; } = null!;
        public DbSet<FacultyAssignment> FacultyAssignments { get; set; } = null!;
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Indexes
            builder.Entity<Student>()
                .HasIndex(s => s.RollNumber)
                .IsUnique();

            builder.Entity<Faculty>()
                .HasIndex(f => f.FacultyCode)
                .IsUnique();

            builder.Entity<Course>()
                .HasIndex(c => c.Code)
                .IsUnique();

            builder.Entity<Subject>()
                .HasIndex(s => s.Code)
                .IsUnique();

            // Crucial: Prevent duplicate attendance records for the same student, subject, class, and date
            builder.Entity<AttendanceRecord>()
                .HasIndex(a => new { a.StudentId, a.SubjectId, a.ClassSectionId, a.Date })
                .IsUnique();

            // Relationship configs & Delete Behaviors (Restrict/NoAction to prevent multiple cascade paths in SQL Server)
            builder.Entity<Student>()
                .HasOne(s => s.User)
                .WithOne(u => u.StudentProfile)
                .HasForeignKey<Student>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Faculty>()
                .HasOne(f => f.User)
                .WithOne(u => u.FacultyProfile)
                .HasForeignKey<Faculty>(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AttendanceRecord>()
                .HasOne(a => a.Student)
                .WithMany(s => s.AttendanceRecords)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AttendanceRecord>()
                .HasOne(a => a.Subject)
                .WithMany(s => s.AttendanceRecords)
                .HasForeignKey(a => a.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AttendanceRecord>()
                .HasOne(a => a.ClassSection)
                .WithMany(c => c.AttendanceRecords)
                .HasForeignKey(a => a.ClassSectionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AttendanceRecord>()
                .HasOne(a => a.RecordedByFaculty)
                .WithMany(f => f.RecordedAttendances)
                .HasForeignKey(a => a.RecordedByFacultyId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<StudentEnrollment>()
                .HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StudentEnrollment>()
                .HasOne(e => e.ClassSection)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.ClassSectionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<FacultyAssignment>()
                .HasOne(fa => fa.Faculty)
                .WithMany(f => f.Assignments)
                .HasForeignKey(fa => fa.FacultyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<FacultyAssignment>()
                .HasOne(fa => fa.Subject)
                .WithMany(s => s.FacultyAssignments)
                .HasForeignKey(fa => fa.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<FacultyAssignment>()
                .HasOne(fa => fa.ClassSection)
                .WithMany(c => c.FacultyAssignments)
                .HasForeignKey(fa => fa.ClassSectionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
