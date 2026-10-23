using AcademicScheduler.Data;
using AcademicScheduler.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademicScheduler.Services;

public class EnrollmentReportService
{
    private readonly AcademicSchedulerDbContext _db;

    public EnrollmentReportService(AcademicSchedulerDbContext db)
    {
        _db = db;
    }

    public async Task<List<EnrollmentReportRow>> GetReportAsync(
        string? department = null)
    {
        var query = _db.Enrollments
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(department))
        {
            query = query.Where(e =>
                e.Course.Department == department);
        }

        return await query
            .OrderBy(e => e.Student.LastName)
            .ThenBy(e => e.Student.FirstName)
            .ThenBy(e => e.Course.CourseNumber)
            .Select(e => new EnrollmentReportRow
            {
                StudentName =
                    e.Student.FirstName + " " + e.Student.LastName,
                YearLevel = e.Student.YearLevel,
                CourseNumber = e.Course.CourseNumber,
                CourseName = e.Course.CourseName,
                Department = e.Course.Department,
                Credits = e.Course.Credits
            })
            .ToListAsync();
    }

    public async Task<List<string>> GetDepartmentsAsync()
    {
        return await _db.Courses
            .AsNoTracking()
            .Select(c => c.Department)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();
    }
}