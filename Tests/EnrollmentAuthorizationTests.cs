
using System;
using System.Threading.Tasks;
using AcademicScheduler.Data;
using AcademicScheduler.Tests.Helpers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AcademicScheduler.Tests;

public class EnrollmentAuthorizationTests
{
    [Theory]
    [InlineData("Viewer")]
    [InlineData(null)]
    public async Task AssignCourse_WhenNotAdministrator_ThrowsUnauthorized(
        string? role)
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AcademicSchedulerDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext = new AcademicSchedulerDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var service = EnrollmentServiceFactory.Create(dbContext, role);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.AssignCourseAsync(1, 1));
    }

    [Theory]
    [InlineData("Viewer")]
    [InlineData(null)]
    public async Task RemoveCourse_WhenNotAdministrator_ThrowsUnauthorized(
        string? role)
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AcademicSchedulerDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext = new AcademicSchedulerDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var service = EnrollmentServiceFactory.Create(dbContext, role);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.RemoveCourseAsync(1, 1));
    }
}
