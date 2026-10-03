namespace AcademicScheduler.Data.Import;

public class StudentCsvRecord
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string HouseAffiliation { get; set; } = string.Empty;

    public int YearLevel { get; set; }

    public int CurrentCredits { get; set; }

    public int MaxCreditsAllowed { get; set; }

    public string CompletedCourses { get; set; } = string.Empty;
}