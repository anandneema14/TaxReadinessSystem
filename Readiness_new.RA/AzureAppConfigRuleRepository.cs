using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Readiness_New.Data;

namespace Readiness_new.RA;

public class AzureAppConfigRuleRepository : IRuleRepository
{
    private readonly IConfiguration _configuration;
    private readonly string _ruleKey;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public AzureAppConfigRuleRepository(IConfiguration configuration, string ruleKey)
    {
        _configuration = configuration;
        _ruleKey = ruleKey;
    }

    public Task<RuleSet?> LoadRulesAsync(string path)
    {
        // For Azure App Config, 'path' is ignored or could be used as the key if not provided in constructor
        var keyToUse = string.IsNullOrEmpty(path) || path == "rules.json" ? _ruleKey : path;
        var json = _configuration[keyToUse];
        
        if (string.IsNullOrEmpty(json))
        {
            return Task.FromResult<RuleSet?>(null);
        }

        return Task.FromResult(JsonSerializer.Deserialize<RuleSet>(json, _jsonOptions));
    }

    public Task<List<Rule>> LoadRulesFromJsonAsync(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return Task.FromResult(new List<Rule>());
        }
        
        var ruleSet = JsonSerializer.Deserialize<RuleSet>(json, _jsonOptions);
        return Task.FromResult(ruleSet?.Rules ?? new List<Rule>());
    }
}
