namespace AcademicScheduler.Models;

public class EligibilityResult
{
    public bool IsEligible { get; init; }

    public string? Reason { get; init; }

    public static EligibilityResult Eligible()
    {
        return new EligibilityResult
        {
            IsEligible = true
        };
    }

    public static EligibilityResult Ineligible(string reason)
    {
        return new EligibilityResult
        {
            IsEligible = false,
            Reason = reason
        };
    }
}