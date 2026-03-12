namespace Readiness_New.Data;

public class ValidationResult
{
    public bool IsValid => !Violations.Any();
    public List<Violation> Violations { get; set; } = new List<Violation>();
    public int TotalRulesEvaluated { get; set; }
    public int ViolationCount => Violations.Count;

    public List<Violation> GetViolationsBySeverity(string severity)
    {
        return Violations.Where(v => v.Severity == severity).ToList();
    }

    public List<Violation> GetViolationsByCategory(string category)
    {
        return Violations.Where(v => v.Category == category).ToList();
    }
}