namespace AcademicScheduler.Models;

public class Course
{
    public int Id { get; set; }

    public required string CourseNumber { get; set; }

    public required string CourseName { get; set; }

    public required string CourseDescription { get; set; }

    public required string Instructor { get; set; }

    public int Credits { get; set; }

    public string? PrerequisiteCourseNumber { get; set; }

    public required string Department { get; set; }

    public ICollection<CompletedCourse> CompletedByStudents { get; set; }
        = new List<CompletedCourse>();

    public ICollection<Enrollment> Enrollments { get; set; }
        = new List<Enrollment>();
}