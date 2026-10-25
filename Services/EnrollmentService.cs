using AcademicScheduler.Data;
using AcademicScheduler.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
namespace AcademicScheduler.Services;

public class EnrollmentService
{
    private readonly AcademicSchedulerDbContext _dbContext;
    private readonly IAuthorizationService _authorizationService;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public EnrollmentService(
    AcademicSchedulerDbContext dbContext,
    IAuthorizationService authorizationService,
    AuthenticationStateProvider authenticationStateProvider)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public EligibilityResult CheckEligibility(
        Student student,
        Course course)
    {
        // Rule 1: A completed course should not be taken again.
        if (student.CompletedCourses.Any(completed =>
                completed.CourseId == course.Id))
        {
            return EligibilityResult.Ineligible(
                "Course already completed.");
        }

        // Rule 2: A student cannot be assigned to the same course twice.
        if (student.Enrollments.Any(enrollment =>
                enrollment.CourseId == course.Id))
        {
            return EligibilityResult.Ineligible(
                "Already assigned to this course.");
        }

        // Rule 3: Required prerequisite must already be completed.
        if (!string.IsNullOrWhiteSpace(course.PrerequisiteCourseNumber))
        {
            var hasPrerequisite = student.CompletedCourses.Any(completed =>
                completed.Course.CourseNumber.Equals(
                    course.PrerequisiteCourseNumber,
                    StringComparison.OrdinalIgnoreCase));

            if (!hasPrerequisite)
            {
                return EligibilityResult.Ineligible(
                    $"Missing prerequisite: {course.PrerequisiteCourseNumber}.");
            }
        }

        // Rule 4: The assignment cannot exceed the student's credit limit.
        var assignedCredits = student.Enrollments
            .Sum(enrollment => enrollment.Course.Credits);

        var projectedCredits =
            student.CurrentCredits +
            assignedCredits +
            course.Credits;

        if (projectedCredits > student.MaxCreditsAllowed)
        {
            return EligibilityResult.Ineligible(
                $"Would exceed maximum credit load of " +
                $"{student.MaxCreditsAllowed} credits.");
        }

        return EligibilityResult.Eligible();
    }
    public async Task<EligibilityResult> AssignCourseAsync(
    int studentId,
    int courseId)
    {
        await EnsureCanManageEnrollmentsAsync();

        var student = await _dbContext.Students
            .Include(student => student.CompletedCourses)
                .ThenInclude(completed => completed.Course)
            .Include(student => student.Enrollments)
                .ThenInclude(enrollment => enrollment.Course)
            .FirstOrDefaultAsync(student => student.Id == studentId);

        if (student is null)
        {
            return EligibilityResult.Ineligible(
                "Student not found.");
        }

        var course = await _dbContext.Courses
            .FirstOrDefaultAsync(course => course.Id == courseId);

        if (course is null)
        {
            return EligibilityResult.Ineligible(
                "Course not found.");
        }

        var eligibility = CheckEligibility(student, course);

        if (!eligibility.IsEligible)
        {
            return eligibility;
        }

        var enrollment = new Enrollment
        {
            StudentId = student.Id,
            CourseId = course.Id,
            AssignedAt = DateTime.UtcNow
        };

        _dbContext.Enrollments.Add(enrollment);

        await _dbContext.SaveChangesAsync();

        return EligibilityResult.Eligible();
    }
    public async Task<bool> RemoveCourseAsync(
    int studentId,
    int courseId)
    {
        await EnsureCanManageEnrollmentsAsync();
        
        var enrollment = await _dbContext.Enrollments
            .FirstOrDefaultAsync(enrollment =>
                enrollment.StudentId == studentId &&
                enrollment.CourseId == courseId);

        if (enrollment is null)
        {
            return false;
        }

        _dbContext.Enrollments.Remove(enrollment);

        await _dbContext.SaveChangesAsync();

        return true;
    }
    private async Task EnsureCanManageEnrollmentsAsync()
    {
        var authenticationState =
            await _authenticationStateProvider.GetAuthenticationStateAsync();

        var authorizationResult =
            await _authorizationService.AuthorizeAsync(
                authenticationState.User,
                "CanManageEnrollments");

        if (!authorizationResult.Succeeded)
        {
            throw new UnauthorizedAccessException(
                "Only administrators can manage enrollments.");
        }
    }
}