namespace Readiness_New.API.Configurations;

public class RuleEngineOptions
{
    public string RulesPath { get; set; }
    public bool AutoReload { get; set; }
    public int AutoReloadInterval { get; set; } = 60;
}