namespace AcademicScheduler.Models;

public class Student
{
    public int Id { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public required string Email { get; set; }

    public required string HouseAffiliation { get; set; }

    public int YearLevel { get; set; }

    public int CurrentCredits { get; set; }

    public int MaxCreditsAllowed { get; set; }

    public ICollection<CompletedCourse> CompletedCourses { get; set; }
        = new List<CompletedCourse>();

    public ICollection<Enrollment> Enrollments { get; set; }
        = new List<Enrollment>();
}