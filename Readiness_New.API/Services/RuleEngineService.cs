using System.Reflection;
using System.Text.Json;
using Readiness_New.Data;
using Readiness_new.RA;

namespace Readiness_New.API.Services;

public class RuleEngineService : IRuleEngine
{
    public ValidationResult ValidateTaxReturn(TaxReturn taxReturn, List<Rule> rules)
    {
        if (taxReturn == null)
        {
            throw new ArgumentNullException(nameof(taxReturn));
        }

        var data = ConvertObjectToDictionary(taxReturn);
        return ValidateTaxReturn(data, rules);
    }

    public ValidationResult ValidateTaxReturn(Dictionary<string, object> taxReturnData, List<Rule> rules)
    {
        var result = new ValidationResult();
        if (rules == null || rules.Count == 0)
        {
            return result;
        }
        
        var enabledRules = rules.Where(r => r.Enabled).ToList();
        result.TotalRulesEvaluated = enabledRules.Count;

        foreach (var rule in enabledRules)
        {
            try
            {
                var affectedFields = new List<string>();
                var isViolated = EvaluateCondition(rule.Condition, taxReturnData, affectedFields);

                if (isViolated)
                {
                    var violation = new Violation
                    {
                        RuleId = rule.Id,
                        RuleName = rule.Name,
                        Description = rule.Description,
                        Severity = rule.Severity,
                        Category = rule.Category,
                        AffectedFields = affectedFields.Distinct().ToList()
                    };
                    
                    result.Violations.Add(violation);
                }
            }
            catch (Exception ex)
            {
                result.Violations.Add(new Violation
                    {
                        RuleId = rule.Id,
                        RuleName = rule.Name,
                        Description = $"Error evaluating rule: {ex.Message}",
                        Severity = "System",
                        Category = "RuleEvaluation"
                    });
            }
        }
        return result;
    }

    private bool EvaluateCondition(Condition condition, Dictionary<string, object> data, List<string> affectedFields)
    {
        if (condition == null)
        {
            return false;
        }

        if (condition.Conditions != null && condition.Conditions.Count > 0)
        {
            return EvaluateCompositeCondition(condition, data, affectedFields);
        }
        
        return EvaluateSimpleCondition(condition, data, affectedFields);
    }

    private bool EvaluateCompositeCondition(Condition condition, Dictionary<string, object> data,
        List<string> affectedFields)
    {
        var results = condition.Conditions.Select(c => EvaluateCondition(c, data, affectedFields)).ToList();

        return condition.LogicalOperator?.ToUpper() switch
        {
            "AND" => results.All(r => r),
            "OR" => results.Any(r => r),
            _ => throw new ArgumentException("Invalid logical operator:")
        };
    }

    private bool EvaluateSimpleCondition(Condition condition, Dictionary<string, object> data,
        List<string> affectedFields)
    {
        if (string.IsNullOrWhiteSpace(condition.Field) || !data.ContainsKey(condition.Field))
        {
            return false;
        }
        affectedFields.Add(condition.Field);
        
        var fieldValue = GetFieldValue(condition.Field, data);

        if (condition.Operator?.ToUpper() == "EXISTS")
        {
            return fieldValue != null;
        }
        
        if (condition.Operator?.ToUpper() == "NOTEXISTS")
        {
            return fieldValue == null;
        }
        
        if (condition.Operator?.ToUpper() == "ISEMPTY")
        {
            return fieldValue == null || string.IsNullOrWhiteSpace(fieldValue?.ToString());
        }
        
        if (condition.Operator?.ToUpper() == "NOTEMPTY")
        {
            return fieldValue != null || !string.IsNullOrWhiteSpace(fieldValue?.ToString());
        }

        if (fieldValue == null)
        {
            return false;
        }

        return condition.Operator?.ToUpper() switch
        {
            "EQUALS" or "EQ" => CompareValues(fieldValue, condition.Value) == 0,
            "NOTEQUALS" or "NE" => CompareValues(fieldValue, condition.Value) == 0,
            "GREATERTHAN" or "GT" => CompareValues(fieldValue, condition.Value) == 0,
            "GREATERTHANOREQUAL" or "GTE" => CompareValues(fieldValue, condition.Value) >= 0,
            "LESSTHAN" or "LT" => CompareValues(fieldValue, condition.Value) == 0,
            "LESSTHANOREQUAL" or "LTE" => CompareValues(fieldValue, condition.Value) <= 0,
            "CONTAINS" => fieldValue?.ToString()
                ?.Contains(condition.Value?.ToString(), StringComparison.OrdinalIgnoreCase) ?? false,
            "STARTSWITH" => fieldValue?.ToString()
                ?.StartsWith(condition.Value?.ToString(), StringComparison.OrdinalIgnoreCase) ?? false,
            "ENDSWITH" => fieldValue?.ToString()
                ?.EndsWith(condition.Value?.ToString(), StringComparison.OrdinalIgnoreCase) ?? false,
            "IN" => IsInList(fieldValue, condition.Value),
            "NOTIN" => !IsInList(fieldValue, condition.Value),
            _ => false
        };
    }

    private object GetFieldValue(string fieldPath, Dictionary<string, object> data)
    {
        if(data == null || !data.ContainsKey(fieldPath))
        {
            return null;
        }
        
        var parts = fieldPath.Split('.');
        object current = data;

        foreach (var part in parts)
        {
            if (current == null)
            {
                return null;
            }
            if (current is Dictionary<string, object> dict)
            {
                if(!dict.TryGetValue(part, out current))
                    return null;
            }
            else
            {
                var property = current.GetType().GetProperty(part, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (property == null)
                {
                    return null;
                }
                current = property.GetValue(current);
            }
        }
        return current;
    }

    private int CompareValues(object value1, object value2)
    {
        if(value1 == null && value2 == null)
            return 0;
        if(value1 == null)
            return -1;
        if(value2 == null)
            return 1;
        try
        {
            if (value1 is IComparable comparable1)
            {
                var convertedValue2 = Convert.ChangeType(value2, comparable1.GetType());
                return comparable1.CompareTo(convertedValue2);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        return string.Compare(value1.ToString(), value2.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private bool IsInList(object value, object listValue)
    {
        if (value == null)
        {
            return false;
        }

        try
        {
            if (listValue is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in jsonElement.EnumerateArray())
                {
                    var itemValue = item.ValueKind == JsonValueKind.String ? item.GetString() : item.ToString();
                    if (CompareValues(value, itemValue) == 0)
                    {
                        return true;
                    }
                }
            }
            else if (listValue is IEnumerable<object> list)
            {
                return list.Any(item => CompareValues(value, item) == 0);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        return false;
    }

    private static Dictionary<string, object> ConvertObjectToDictionary(object obj)
    {
        var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        
        if(obj == null)
            return dict;

        var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in properties)
        {
            try
            {
                var value = prop.GetValue(obj);
                dict.Add(prop.Name, value);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        return dict;
    }
}