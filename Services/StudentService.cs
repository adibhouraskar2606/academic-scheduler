using AcademicScheduler.Data;
using AcademicScheduler.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademicScheduler.Services;

public class StudentService
{
    private readonly AcademicSchedulerDbContext _dbContext;

    public StudentService(AcademicSchedulerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Student>> GetStudentsAsync()
    {
        return await _dbContext.Students
            .Include(student => student.CompletedCourses)
            .ThenInclude(completedCourse => completedCourse.Course)
            .OrderBy(student => student.LastName)
            .ThenBy(student => student.FirstName)
            .ToListAsync();
    }
    public async Task<Student?> GetStudentByIdAsync(int id)
    {
        return await _dbContext.Students
            .Include(student => student.CompletedCourses)
                .ThenInclude(completedCourse => completedCourse.Course)
            .Include(student => student.Enrollments)
                .ThenInclude(enrollment => enrollment.Course)
            .FirstOrDefaultAsync(student => student.Id == id);
    }
}