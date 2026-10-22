using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Models;

namespace SmartAttendance.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // 1. Seed Roles
            string[] roles = { "Admin", "Faculty", "Student" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Seed Admin User
            var adminEmail = "admin@smartattendance.edu";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    UserRole = "Admin",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // 3. Seed Academic Semester
            if (!await context.AcademicSemesters.AnyAsync())
            {
                var sem1 = new AcademicSemester
                {
                    Name = "Fall 2026",
                    AcademicYear = "2026-2027",
                    StartDate = new DateTime(2026, 8, 1),
                    EndDate = new DateTime(2026, 12, 20),
                    IsCurrent = true
                };
                var sem2 = new AcademicSemester
                {
                    Name = "Spring 2026",
                    AcademicYear = "2025-2026",
                    StartDate = new DateTime(2026, 1, 10),
                    EndDate = new DateTime(2026, 5, 25),
                    IsCurrent = false
                };
                context.AcademicSemesters.AddRange(sem1, sem2);
                await context.SaveChangesAsync();
            }

            var currentSemester = await context.AcademicSemesters.FirstOrDefaultAsync(s => s.IsCurrent)
                                  ?? await context.AcademicSemesters.FirstAsync();

            // 4. Seed Courses
            if (!await context.Courses.AnyAsync())
            {
                var cse = new Course
                {
                    Code = "BTECH-CSE",
                    Name = "B.Tech Computer Science & Engineering",
                    Department = "Computer Science & Engineering",
                    DurationYears = 4,
                    Description = "Undergraduate program in modern computing, AI, systems, and software engineering."
                };
                var it = new Course
                {
                    Code = "BTECH-IT",
                    Name = "B.Tech Information Technology",
                    Department = "Information Technology",
                    DurationYears = 4,
                    Description = "Undergraduate program focused on networking, cybersecurity, and cloud technologies."
                };
                context.Courses.AddRange(cse, it);
                await context.SaveChangesAsync();
            }

            var cseCourse = await context.Courses.FirstAsync(c => c.Code == "BTECH-CSE");
            var itCourse = await context.Courses.FirstAsync(c => c.Code == "BTECH-IT");

            // 5. Seed Class Sections
            if (!await context.ClassSections.AnyAsync())
            {
                var secA = new ClassSection
                {
                    Name = "CSE-5A",
                    CourseId = cseCourse.Id,
                    AcademicSemesterId = currentSemester.Id,
                    RoomNumber = "Room 301",
                    Capacity = 60
                };
                var secB = new ClassSection
                {
                    Name = "CSE-5B",
                    CourseId = cseCourse.Id,
                    AcademicSemesterId = currentSemester.Id,
                    RoomNumber = "Room 302",
                    Capacity = 60
                };
                var secIT = new ClassSection
                {
                    Name = "IT-5A",
                    CourseId = itCourse.Id,
                    AcademicSemesterId = currentSemester.Id,
                    RoomNumber = "Room 401",
                    Capacity = 60
                };
                context.ClassSections.AddRange(secA, secB, secIT);
                await context.SaveChangesAsync();
            }

            var cseSectionA = await context.ClassSections.FirstAsync(s => s.Name == "CSE-5A");

            // 6. Seed Subjects
            if (!await context.Subjects.AnyAsync())
            {
                var subjects = new List<Subject>
                {
                    new Subject { Code = "CS501", Name = "Database Management Systems", Credits = 4, SemesterNumber = 5, CourseId = cseCourse.Id },
                    new Subject { Code = "CS502", Name = "Computer Networks", Credits = 4, SemesterNumber = 5, CourseId = cseCourse.Id },
                    new Subject { Code = "CS503", Name = "Operating Systems", Credits = 4, SemesterNumber = 5, CourseId = cseCourse.Id },
                    new Subject { Code = "CS504", Name = "Web Technologies", Credits = 3, SemesterNumber = 5, CourseId = cseCourse.Id },
                    new Subject { Code = "CS301", Name = "Data Structures & Algorithms", Credits = 4, SemesterNumber = 3, CourseId = cseCourse.Id }
                };
                context.Subjects.AddRange(subjects);
                await context.SaveChangesAsync();
            }

            var subDbms = await context.Subjects.FirstAsync(s => s.Code == "CS501");
            var subCn = await context.Subjects.FirstAsync(s => s.Code == "CS502");
            var subOs = await context.Subjects.FirstAsync(s => s.Code == "CS503");
            var subWeb = await context.Subjects.FirstAsync(s => s.Code == "CS504");

            // 7. Seed Faculty Accounts
            var facultyList = new[]
            {
                new { Email = "faculty.sharma@smartattendance.edu", Code = "FAC-CS01", First = "Rajesh", Last = "Sharma", Dept = "Computer Science & Engineering", Desig = "Professor & HOD", Qual = "Ph.D. in Computer Science" },
                new { Email = "faculty.patel@smartattendance.edu", Code = "FAC-CS02", First = "Anita", Last = "Patel", Dept = "Computer Science & Engineering", Desig = "Assistant Professor", Qual = "M.Tech in Software Engineering" },
                new { Email = "faculty.verma@smartattendance.edu", Code = "FAC-CS03", First = "Vikram", Last = "Verma", Dept = "Computer Science & Engineering", Desig = "Associate Professor", Qual = "Ph.D. in Distributed Systems" }
            };

            foreach (var f in facultyList)
            {
                var user = await userManager.FindByEmailAsync(f.Email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = f.Email,
                        Email = f.Email,
                        FullName = $"Dr. {f.First} {f.Last}",
                        UserRole = "Faculty",
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    var res = await userManager.CreateAsync(user, "Faculty@123");
                    if (res.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Faculty");

                        var facultyProfile = new Faculty
                        {
                            UserId = user.Id,
                            FacultyCode = f.Code,
                            FirstName = f.First,
                            LastName = f.Last,
                            Email = f.Email,
                            Phone = "+91 98765 43210",
                            Department = f.Dept,
                            Designation = f.Desig,
                            Qualification = f.Qual,
                            JoiningDate = new DateTime(2021, 7, 1)
                        };
                        context.Faculties.Add(facultyProfile);
                    }
                }
            }
            await context.SaveChangesAsync();

            var facSharma = await context.Faculties.FirstAsync(f => f.FacultyCode == "FAC-CS01");
            var facPatel = await context.Faculties.FirstAsync(f => f.FacultyCode == "FAC-CS02");
            var facVerma = await context.Faculties.FirstAsync(f => f.FacultyCode == "FAC-CS03");

            // 8. Seed Faculty Assignments
            if (!await context.FacultyAssignments.AnyAsync())
            {
                var assignments = new List<FacultyAssignment>
                {
                    new FacultyAssignment { FacultyId = facSharma.Id, SubjectId = subDbms.Id, ClassSectionId = cseSectionA.Id, AcademicSemesterId = currentSemester.Id },
                    new FacultyAssignment { FacultyId = facSharma.Id, SubjectId = subCn.Id, ClassSectionId = cseSectionA.Id, AcademicSemesterId = currentSemester.Id },
                    new FacultyAssignment { FacultyId = facPatel.Id, SubjectId = subWeb.Id, ClassSectionId = cseSectionA.Id, AcademicSemesterId = currentSemester.Id },
                    new FacultyAssignment { FacultyId = facVerma.Id, SubjectId = subOs.Id, ClassSectionId = cseSectionA.Id, AcademicSemesterId = currentSemester.Id }
                };
                context.FacultyAssignments.AddRange(assignments);
                await context.SaveChangesAsync();
            }

            // 9. Seed Students
            var studentSeedData = new[]
            {
                new { Email = "student.rahul@smartattendance.edu", Roll = "26BCSE01", Reg = "REG2026001", First = "Rahul", Last = "Kumar", Gender = "Male" },
                new { Email = "student.priya@smartattendance.edu", Roll = "26BCSE02", Reg = "REG2026002", First = "Priya", Last = "Singh", Gender = "Female" },
                new { Email = "student.amit@smartattendance.edu", Roll = "26BCSE03", Reg = "REG2026003", First = "Amit", Last = "Patel", Gender = "Male" },
                new { Email = "student.neha@smartattendance.edu", Roll = "26BCSE04", Reg = "REG2026004", First = "Neha", Last = "Gupta", Gender = "Female" },
                new { Email = "student.rohit@smartattendance.edu", Roll = "26BCSE05", Reg = "REG2026005", First = "Rohit", Last = "Verma", Gender = "Male" }, // Will have low attendance
                new { Email = "student.ananya@smartattendance.edu", Roll = "26BCSE06", Reg = "REG2026006", First = "Ananya", Last = "Roy", Gender = "Female" },
                new { Email = "student.suresh@smartattendance.edu", Roll = "26BCSE07", Reg = "REG2026007", First = "Suresh", Last = "Menon", Gender = "Male" },
                new { Email = "student.sneha@smartattendance.edu", Roll = "26BCSE08", Reg = "REG2026008", First = "Sneha", Last = "Reddy", Gender = "Female" } // Will have low attendance
            };

            foreach (var st in studentSeedData)
            {
                var user = await userManager.FindByEmailAsync(st.Email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = st.Email,
                        Email = st.Email,
                        FullName = $"{st.First} {st.Last}",
                        UserRole = "Student",
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    var res = await userManager.CreateAsync(user, "Student@123");
                    if (res.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Student");

                        var studentProfile = new Student
                        {
                            UserId = user.Id,
                            RollNumber = st.Roll,
                            RegistrationNumber = st.Reg,
                            FirstName = st.First,
                            LastName = st.Last,
                            Email = st.Email,
                            Phone = "+91 91234 56789",
                            CourseId = cseCourse.Id,
                            ClassSectionId = cseSectionA.Id,
                            CurrentSemester = 5,
                            Gender = st.Gender,
                            AdmissionDate = new DateTime(2024, 8, 1)
                        };
                        context.Students.Add(studentProfile);
                    }
                }
            }
            await context.SaveChangesAsync();

            // 10. Seed Student Enrollments
            var allStudents = await context.Students.Where(s => s.ClassSectionId == cseSectionA.Id).ToListAsync();
            if (!await context.StudentEnrollments.AnyAsync())
            {
                foreach (var s in allStudents)
                {
                    context.StudentEnrollments.Add(new StudentEnrollment
                    {
                        StudentId = s.Id,
                        ClassSectionId = cseSectionA.Id,
                        AcademicSemesterId = currentSemester.Id,
                        EnrollmentDate = new DateTime(2026, 8, 1)
                    });
                }
                await context.SaveChangesAsync();
            }

            // 11. Seed Realistic Attendance Records for the past 14 weekdays across 4 subjects
            if (!await context.AttendanceRecords.AnyAsync())
            {
                var rand = new Random(42); // deterministic seed for consistent viva demo
                var subjectsToRecord = new[]
                {
                    new { Sub = subDbms, Fac = facSharma },
                    new { Sub = subCn, Fac = facSharma },
                    new { Sub = subWeb, Fac = facPatel },
                    new { Sub = subOs, Fac = facVerma }
                };

                var attendanceRecords = new List<AttendanceRecord>();
                var baseDate = DateTime.Today.AddDays(-20);

                for (int day = 0; day <= 20; day++)
                {
                    var currentDate = baseDate.AddDays(day);
                    if (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday)
                        continue; // Skip weekends

                    foreach (var pair in subjectsToRecord)
                    {
                        foreach (var student in allStudents)
                        {
                            AttendanceStatus status;

                            // Custom logic: Rohit (26BCSE05) and Sneha (26BCSE08) are chronic absentees (< 70% attendance)
                            if (student.RollNumber == "26BCSE05")
                            {
                                int r = rand.Next(100);
                                status = r < 50 ? AttendanceStatus.Absent : (r < 70 ? AttendanceStatus.Late : AttendanceStatus.Present);
                            }
                            else if (student.RollNumber == "26BCSE08")
                            {
                                int r = rand.Next(100);
                                status = r < 45 ? AttendanceStatus.Absent : (r < 65 ? AttendanceStatus.Late : AttendanceStatus.Present);
                            }
                            else
                            {
                                // Regular students (85% - 95% attendance)
                                int r = rand.Next(100);
                                status = r < 85 ? AttendanceStatus.Present : (r < 93 ? AttendanceStatus.Late : AttendanceStatus.Absent);
                            }

                            attendanceRecords.Add(new AttendanceRecord
                            {
                                StudentId = student.Id,
                                SubjectId = pair.Sub.Id,
                                ClassSectionId = cseSectionA.Id,
                                Date = currentDate,
                                Status = status,
                                Remarks = status == AttendanceStatus.Late ? "Arrived 10 mins late" : (status == AttendanceStatus.Absent ? "Uninformed absence" : null),
                                RecordedByFacultyId = pair.Fac.Id,
                                CreatedAt = currentDate.AddHours(10)
                            });
                        }
                    }
                }

                context.AttendanceRecords.AddRange(attendanceRecords);
                await context.SaveChangesAsync();
            }
        }
    }
}
