using System.Globalization;
using AcademicScheduler.Data.Import;
using AcademicScheduler.Models;
using CsvHelper;
using Microsoft.EntityFrameworkCore;

namespace AcademicScheduler.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(
        AcademicSchedulerDbContext dbContext,
        IWebHostEnvironment environment)
    {
        var hasStudents = await dbContext.Students.AnyAsync();
        var hasCourses = await dbContext.Courses.AnyAsync();

        if (hasStudents && hasCourses)
        {
            return;
        }

        if (hasStudents || hasCourses)
        {
            throw new InvalidOperationException(
                "The database contains partial seed data. " +
                "Delete the local database and restart the application.");
        }

        var coursesPath = Path.Combine(
            environment.ContentRootPath,
            "SeedData",
            "courses-starter.csv");

        var studentsPath = Path.Combine(
            environment.ContentRootPath,
            "SeedData",
            "students-starter.csv");

        var courseRecords = ReadCsv<CourseCsvRecord>(coursesPath);
        var studentRecords = ReadCsv<StudentCsvRecord>(studentsPath);

        var courses = courseRecords.Select(record => new Course
        {
            CourseNumber = record.CourseNumber,
            CourseName = record.CourseName,
            CourseDescription = record.CourseDescription,
            Instructor = record.Instructor,
            Credits = record.Credits,
            PrerequisiteCourseNumber =
                string.IsNullOrWhiteSpace(record.Prerequisites)
                    ? null
                    : record.Prerequisites,
            Department = record.Department
        }).ToList();

        dbContext.Courses.AddRange(courses);

        await dbContext.SaveChangesAsync();

        var coursesByNumber = courses.ToDictionary(
            course => course.CourseNumber,
            StringComparer.OrdinalIgnoreCase);

        foreach (var record in studentRecords)
        {
            var student = new Student
            {
                FirstName = record.FirstName,
                LastName = record.LastName,
                Email = record.Email,
                HouseAffiliation = record.HouseAffiliation,
                YearLevel = record.YearLevel,
                CurrentCredits = record.CurrentCredits,
                MaxCreditsAllowed = record.MaxCreditsAllowed
            };

            var completedCourseNumbers = record.CompletedCourses
                .Split(
                    '|',
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries);

            foreach (var courseNumber in completedCourseNumbers)
            {
                if (!coursesByNumber.TryGetValue(
                        courseNumber,
                        out var course))
                {
                    throw new InvalidOperationException(
                        $"Completed course '{courseNumber}' for " +
                        $"{student.FirstName} {student.LastName} " +
                        "was not found in the course catalog.");
                }

                student.CompletedCourses.Add(new CompletedCourse
                {
                    Course = course
                });
            }

            dbContext.Students.Add(student);
        }

        await dbContext.SaveChangesAsync();
    }

    private static List<T> ReadCsv<T>(string path)
    {
        using var reader = new StreamReader(path);
        using var csv = new CsvReader(
            reader,
            CultureInfo.InvariantCulture);

        return csv.GetRecords<T>().ToList();
    }
}