using AcademicScheduler.Data;
using AcademicScheduler.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademicScheduler.Services;

public class DashboardService
{
    private readonly AcademicSchedulerDbContext _db;

    public DashboardService(AcademicSchedulerDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardStats> GetStatsAsync()
    {
        var totalStudents = await _db.Students.CountAsync();

        var totalCourses = await _db.Courses.CountAsync();

        var totalEnrollments = await _db.Enrollments.CountAsync();

        var totalCredits = await _db.Enrollments
            .SumAsync(e => (int?)e.Course.Credits) ?? 0;

        var averageCredits = totalStudents == 0
            ? 0
            : (double)totalCredits / totalStudents;

        var departmentEnrollments = await _db.Enrollments
            .GroupBy(e => e.Course.Department)
            .Select(g => new DepartmentEnrollment
            {
                Department = g.Key,
                EnrollmentCount = g.Count()
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .ToListAsync();

        return new DashboardStats
        {
            TotalStudents = totalStudents,
            TotalCourses = totalCourses,
            TotalEnrollments = totalEnrollments,
            AverageCredits = averageCredits,
            DepartmentEnrollments = departmentEnrollments
        };
    }
}