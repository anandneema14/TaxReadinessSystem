namespace Readiness_New.Data;

public class Rule
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool Enabled { get; set; } = true;
    public string Severity { get; set; } = "Error";
    public string Category { get; set; }
    public Condition Condition { get; set; }
}