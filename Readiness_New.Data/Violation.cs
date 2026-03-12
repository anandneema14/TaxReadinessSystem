namespace Readiness_New.Data;

public class Violation
{
    public string RuleId { get; set; }
    public string RuleName { get; set; }
    public string Description { get; set; }
    public string Severity { get; set; }
    public string Category { get; set; }
    public List<string> AffectedFields { get; set; } = new List<string>();
    public Dictionary<string, object> Context { get; set; } = new Dictionary<string, object>();
}