using Readiness_New.Data;

namespace Readiness_new.RA;

public interface IRuleEngine
{
    ValidationResult ValidateTaxReturn(TaxReturn taxReturn, List<Rule> rules);

    ValidationResult ValidateTaxReturn(Dictionary<string, object> taxReturnData, List<Rule> rules);
}