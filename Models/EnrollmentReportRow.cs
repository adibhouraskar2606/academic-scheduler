namespace AcademicScheduler.Models;

public class EnrollmentReportRow
{
    public string StudentName { get; set; } = "";
    public int YearLevel { get; set; }
    public string CourseNumber { get; set; } = "";
    public string CourseName { get; set; } = "";
    public string Department { get; set; } = "";
    public int Credits { get; set; }
}