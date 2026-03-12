namespace Readiness_New.API.Configurations;

public class RuleEngineOptions
{
    public string RulesPath { get; set; } = "rules.json";
    public bool AutoReload { get; set; }
    public int AutoReloadInterval { get; set; } = 60;
    public string Provider { get; set; } = "File"; // File or AzureAppConfig
    public AzureAppConfigOptions AzureAppConfig { get; set; } = new();
}

public class AzureAppConfigOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string Key { get; set; } = "Readiness:Rules";
    public string Label { get; set; } = "\0"; // Default label
}