namespace AcademicScheduler.Data.Import;

public class CourseCsvRecord
{
    public string CourseNumber { get; set; } = string.Empty;

    public string CourseName { get; set; } = string.Empty;

    public string CourseDescription { get; set; } = string.Empty;

    public string Instructor { get; set; } = string.Empty;

    public int Credits { get; set; }

    public string? Prerequisites { get; set; }

    public string Department { get; set; } = string.Empty;
}