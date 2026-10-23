namespace AcademicScheduler.Models;

public class DashboardStats
{
    public int TotalStudents { get; set; }

    public int TotalCourses { get; set; }

    public int TotalEnrollments { get; set; }

    public double AverageCredits { get; set; }

    public List<DepartmentEnrollment> DepartmentEnrollments
        { get; set; } = new();
}

public class DepartmentEnrollment
{
    public string Department { get; set; } = string.Empty;

    public int EnrollmentCount { get; set; }
}