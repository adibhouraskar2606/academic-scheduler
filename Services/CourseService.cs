using AcademicScheduler.Data;
using AcademicScheduler.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademicScheduler.Services;

public class CourseService
{
    private readonly AcademicSchedulerDbContext _dbContext;

    public CourseService(AcademicSchedulerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Course>> GetCoursesAsync()
    {
        return await _dbContext.Courses
            .OrderBy(course => course.Department)
            .ThenBy(course => course.CourseNumber)
            .ToListAsync();
    }
}