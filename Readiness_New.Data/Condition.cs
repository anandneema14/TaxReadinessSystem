namespace Readiness_New.Data;

public class Condition
{
    public string Field { get; set; }
    public string Operator { get; set; }
    public object Value { get; set; }
    public string LogicalOperator { get; set; }
    public List<Condition> Conditions { get; set; }
}