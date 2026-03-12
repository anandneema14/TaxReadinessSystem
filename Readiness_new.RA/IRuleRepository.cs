using Readiness_New.Data;

namespace Readiness_new.RA;

public interface IRuleRepository
{
    Task<RuleSet?> LoadRulesAsync(string path);
    Task<List<Rule>> LoadRulesFromJsonAsync(string json);
}