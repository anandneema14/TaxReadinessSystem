using System.Text.Json;
using Readiness_New.Data;

namespace Readiness_new.RA;

public class JsonRuleLoader : IRuleRepository
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public async Task<RuleSet?> LoadRulesAsync(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Rules file not found at {path}");
        }
        
        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<RuleSet>(json, _jsonOptions);
    }

    public async Task<List<Rule>> LoadRulesFromJsonAsync(string json)
    {
        var ruleSet = JsonSerializer.Deserialize<RuleSet>(json, _jsonOptions);
        return ruleSet?.Rules ?? new List<Rule>();
    }
}