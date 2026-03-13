namespace Readiness_New.Data;

public class ReadinessScore
{
    public double Score { get; set; }
    public string Level { get; set; }
    public string Summary { get; set; }
    public List<CategoryScore> CategoryScores { get; set; } = new List<CategoryScore>();
    public ValidationResult ValidationResult { get; set; }
}

public class CategoryScore
{
    public string Category { get; set; }
    public double Score { get; set; }
}
