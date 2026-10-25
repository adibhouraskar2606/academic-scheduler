using AcademicScheduler.Data;
using AcademicScheduler.Models;
using AcademicScheduler.Services;
using AcademicScheduler.Tests.Helpers;
using Xunit;
namespace AcademicScheduler.Tests;

public class EnrollmentServiceTests
{
    [Fact]
    public async Task CheckEligibility_WhenPrerequisiteMissing_ReturnsIneligible()
    {
        await using var database = await TestDatabase.CreateAsync();
        var dbContext = database.Context;

        var completedCourse = new Course
        {
            Id = 1,
            CourseNumber = "POTN-101",
            CourseName = "Introduction to Potions",
            CourseDescription = "Basic potion making",
            Instructor = "Professor Snape",
            Department = "Potions",
            Credits = 3
        };

        var requestedCourse = new Course
        {
            Id = 2,
            CourseNumber = "ALCHEMY-301",
            CourseName = "Advanced Alchemy",
            CourseDescription = "Advanced alchemical studies",
            Instructor = "Professor Flamel",
            Department = "Alchemy",
            Credits = 3,
            PrerequisiteCourseNumber = "POTN-201"
        };

        var student = new Student
        {
            Id = 1,
            FirstName = "Harry",
            LastName = "Potter",
            Email = "harry@test.com",
            HouseAffiliation = "Gryffindor",
            YearLevel = 3,
            CurrentCredits = 9,
            MaxCreditsAllowed = 18
        };

        student.CompletedCourses.Add(new CompletedCourse
        {
            Student = student,
            Course = completedCourse
        });

        dbContext.Students.Add(student);
        dbContext.Courses.Add(requestedCourse);
        await dbContext.SaveChangesAsync();

        // Act: ask our real enrollment service to check eligibility.
        var enrollmentService = EnrollmentServiceFactory.Create(dbContext);

        var result = enrollmentService.CheckEligibility(
            student,
            requestedCourse);

        // Assert: the missing prerequisite must block enrollment.
        Assert.False(result.IsEligible);
        Assert.Equal(
            "Missing prerequisite: POTN-201.",
            result.Reason);
    }
    [Fact]
    public async Task CheckEligibility_WhenPrerequisiteCompleted_ReturnsEligible()
    {
        // Arrange
        await using var database = await TestDatabase.CreateAsync();
        var dbContext = database.Context;

        var prerequisite = new Course
        {
            CourseNumber = "POTN-201",
            CourseName = "Intermediate Potions",
            CourseDescription = "Intermediate potion making",
            Instructor = "Professor Snape",
            Department = "Potions",
            Credits = 3
        };

        var advancedCourse = new Course
        {
            CourseNumber = "ALCHEMY-301",
            CourseName = "Advanced Alchemy",
            CourseDescription = "Advanced alchemical studies",
            Instructor = "Professor Flamel",
            Department = "Alchemy",
            Credits = 3,
            PrerequisiteCourseNumber = "POTN-201"
        };

        var student = new Student
        {
            FirstName = "Harry",
            LastName = "Potter",
            Email = "harry@test.com",
            HouseAffiliation = "Gryffindor",
            YearLevel = 3,
            CurrentCredits = 9,
            MaxCreditsAllowed = 18
        };

        student.CompletedCourses.Add(new CompletedCourse
        {
            Student = student,
            Course = prerequisite
        });

        dbContext.Students.Add(student);
        dbContext.Courses.Add(advancedCourse);
        await dbContext.SaveChangesAsync();

        // Act
        var service = EnrollmentServiceFactory.Create(dbContext);
        var result = service.CheckEligibility(student, advancedCourse);

        // Assert
        Assert.True(result.IsEligible);
        Assert.Null(result.Reason);
    }

    [Fact]
    public async Task CheckEligibility_WhenCourseAlreadyCompleted_ReturnsIneligible()
    {
        // Arrange
        await using var database = await TestDatabase.CreateAsync();
        var dbContext = database.Context;

        var course = new Course
        {
            CourseNumber = "POTN-101",
            CourseName = "Introduction to Potions",
            CourseDescription = "Basic potion making",
            Instructor = "Professor Snape",
            Department = "Potions",
            Credits = 3
        };

        var student = new Student
        {
            FirstName = "Harry",
            LastName = "Potter",
            Email = "harry@test.com",
            HouseAffiliation = "Gryffindor",
            YearLevel = 3,
            CurrentCredits = 9,
            MaxCreditsAllowed = 18
        };

        student.CompletedCourses.Add(new CompletedCourse
        {
            Student = student,
            Course = course
        });

        dbContext.Students.Add(student);
        await dbContext.SaveChangesAsync();

        // Act
        var service = EnrollmentServiceFactory.Create(dbContext);
        var result = service.CheckEligibility(student, course);

        // Assert
        Assert.False(result.IsEligible);
        Assert.Equal("Course already completed.", result.Reason);
    }

    [Fact]
    public async Task CheckEligibility_WhenCourseAlreadyAssigned_ReturnsIneligible()
    {
        // Arrange
        await using var database = await TestDatabase.CreateAsync();
        var dbContext = database.Context;

        var course = new Course
        {
            CourseNumber = "APPR-250",
            CourseName = "Apparition Training",
            CourseDescription = "Introduction to apparition",
            Instructor = "Professor Twycross",
            Department = "Transportation",
            Credits = 2
        };

        var student = new Student
        {
            FirstName = "Harry",
            LastName = "Potter",
            Email = "harry@test.com",
            HouseAffiliation = "Gryffindor",
            YearLevel = 3,
            CurrentCredits = 12,
            MaxCreditsAllowed = 18
        };

        student.Enrollments.Add(new Enrollment
        {
            Student = student,
            Course = course
        });

        dbContext.Students.Add(student);
        await dbContext.SaveChangesAsync();

        // Act
        var service = EnrollmentServiceFactory.Create(dbContext);
        var result = service.CheckEligibility(student, course);

        // Assert
        Assert.False(result.IsEligible);
        Assert.Equal(
            "Already assigned to this course.",
            result.Reason);
    }

    [Theory]
    [InlineData(4, true)]
    [InlineData(5, false)]
    public async Task CheckEligibility_WhenApproachingCreditLimit_ReturnsExpectedResult(
    int newCourseCredits,
    bool expectedEligibility)
    {
        // Arrange
        await using var database = await TestDatabase.CreateAsync();
        var dbContext = database.Context;

        var student = new Student
        {
            FirstName = "Harry",
            LastName = "Potter",
            Email = "harry@test.com",
            HouseAffiliation = "Gryffindor",
            YearLevel = 3,
            CurrentCredits = 14,
            MaxCreditsAllowed = 18
        };

        var course = new Course
        {
            CourseNumber = "TEST-301",
            CourseName = "Advanced Studies",
            CourseDescription = "Testing credit limits",
            Instructor = "Professor McGonagall",
            Department = "General",
            Credits = newCourseCredits
        };

        dbContext.Students.Add(student);
        dbContext.Courses.Add(course);
        await dbContext.SaveChangesAsync();

        // Act
        var service = EnrollmentServiceFactory.Create(dbContext);
        var result = service.CheckEligibility(student, course);

        // Assert
        Assert.Equal(expectedEligibility, result.IsEligible);

        if (!expectedEligibility)
        {
            Assert.Equal(
                "Would exceed maximum credit load of 18 credits.",
                result.Reason);
        }
        else
        {
            Assert.Null(result.Reason);
        }
    }
    [Fact]
    public async Task CheckEligibility_WhenExistingAssignmentsExceedLimit_ReturnsIneligible()
    {
        // Arrange
        await using var database = await TestDatabase.CreateAsync();
        var dbContext = database.Context;

        var student = new Student
        {
            FirstName = "Harry",
            LastName = "Potter",
            Email = "harry@test.com",
            HouseAffiliation = "Gryffindor",
            YearLevel = 3,
            CurrentCredits = 12,
            MaxCreditsAllowed = 18
        };

        var existingCourse = new Course
        {
            CourseNumber = "APPR-250",
            CourseName = "Apparition Training",
            CourseDescription = "Apparition basics",
            Instructor = "Professor Twycross",
            Department = "Transportation",
            Credits = 4
        };

        var requestedCourse = new Course
        {
            CourseNumber = "CHARM-301",
            CourseName = "Advanced Charms",
            CourseDescription = "Advanced charm techniques",
            Instructor = "Professor Flitwick",
            Department = "Charms",
            Credits = 3
        };

        student.Enrollments.Add(new Enrollment
        {
            Student = student,
            Course = existingCourse
        });

        dbContext.Students.Add(student);
        dbContext.Courses.Add(requestedCourse);

        await dbContext.SaveChangesAsync();

        // Act
        var service = EnrollmentServiceFactory.Create(dbContext);
        var result = service.CheckEligibility(student, requestedCourse);

        // Assert
        Assert.False(result.IsEligible);
        Assert.Equal(
            "Would exceed maximum credit load of 18 credits.",
            result.Reason);
    }
}