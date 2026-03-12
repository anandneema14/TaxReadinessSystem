using Microsoft.AspNetCore.Mvc;
using Readiness_New.Data;
using Readiness_new.RA;

namespace Readiness_New.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaxReturnValidationController : ControllerBase
{
    private readonly IRuleEngine _ruleEngine;
    private readonly IRuleRepository _ruleLoader;
    private readonly ILogger<TaxReturnValidationController> _logger;
    private static RuleSet? _cachedRuleSet;
    private static readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1);

    public TaxReturnValidationController(IRuleEngine ruleEngine,
        IRuleRepository ruleLoader, ILogger<TaxReturnValidationController> logger)
    {
        _ruleEngine = ruleEngine; 
        _ruleLoader = ruleLoader; 
        _logger = logger;
    }

    /// <summary>///
    /// Validates a tax return against configured rules
    /// </summary>
    /// <param name="taxReturn">The tax return to validate</param>
    /// <returns>Validation result with any violations found</returns>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ValidationResult>> ValidateTaxReturn([FromBody] TaxReturn taxReturn)
    {
        try
        {
            if (taxReturn == null)
            {
                return BadRequest("Tax return cannot be null");
            }

            var rules = await GetRulesAsync();
            var result = _ruleEngine.ValidateTaxReturn(taxReturn, rules);

            _logger.LogInformation(
                "Validated tax return for taxpayer {TaxpayerId}. Valid: {IsValid}, Violations: {ViolationCount}",
                taxReturn.TaxpayerId, result.IsValid, result.ViolationCount);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating tax return");
            return StatusCode(500, "An error occurred while validating the tax return");
        }
    }

    /// <summary>///
    /// Validates tax return data provided as a dictionary
    /// </summary>///
    /// <param name="taxReturnData">Tax return data as key-value pairs</param>
    /// <returns>Validation result with any violations found</returns>
    [HttpPost("validate/dictionary")]
    [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ValidationResult>> ValidateTaxReturnData(
        [FromBody] Dictionary<string, object> taxReturnData)
    {
        try
        {
            if (taxReturnData == null || taxReturnData.Count == 0)
            {
                return BadRequest("Tax return data cannot be null or empty");
            }

            var rules = await GetRulesAsync();
            var result = _ruleEngine.ValidateTaxReturn(taxReturnData, rules);

            _logger.LogInformation("Validated tax return data. Valid: {IsValid}, Violations: {ViolationCount}",
                result.IsValid, result.ViolationCount);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating tax return data"); 
            return StatusCode(500, "An error occurred while validating the tax return");
        }
    }

    /// <summary>///
    /// Gets all active validation rules
    /// </summary>///
    /// <returns>List of all rules</returns>
    [HttpGet("rules")]
    [ProducesResponseType(typeof(List<Rule>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<Rule>>> GetRules()
    {
        try
        {
            var rules = await GetRulesAsync();
            return Ok(rules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving rules"); 
            return StatusCode(500, "An error occurred while retrieving rules");
        }
    }

    /// <summary>///
    /// Gets rules filtered by category
    /// </summary>///
    /// <param name="category">The category to filter by</param>
    /// <returns>List of rules in the specified category</returns>
    [HttpGet("rules/category/{category}")]
    [ProducesResponseType(typeof(List<Rule>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<Rule>>> GetRulesByCategory(string category)
    {
        try
        {
            var rules = await GetRulesAsync();
            var filteredRules = rules.Where(r => r.Category == category).ToList();
            return Ok(filteredRules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving rules by category");
            return StatusCode(500, "An error occurred while retrieving rules");
        }
    }

    /// <summary>///
    /// Reloads rules from configuration
    /// </summary>///
    /// <returns>Success message</returns>
    [HttpPost("rules/reload")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ReloadRules()
    {
        try
        {
            await _cacheLock.WaitAsync();
            try
            {
                _cachedRuleSet = await _ruleLoader.LoadRulesAsync("rules.json");
                _logger.LogInformation("Rules reloaded successfully. Total rules: {RuleCount}",
                    _cachedRuleSet.Rules.Count);
                return Ok(new { message = "Rules reloaded successfully", ruleCount = _cachedRuleSet.Rules.Count });
            }
            finally
            {
                _cacheLock.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reloading rules"); 
            return StatusCode(500, "An error occurred while reloading rules");
        }
    }

    private async Task<List<Rule>> GetRulesAsync()
    {
        if (_cachedRuleSet != null)
        {
            return _cachedRuleSet.Rules;
        } 
        
        await _cacheLock.WaitAsync();
        try
        {
            if (_cachedRuleSet == null)
            {
                _cachedRuleSet = await _ruleLoader.LoadRulesAsync("rules.json");
                _logger.LogInformation("Rules loaded for the first time. Total rules: {RuleCount}",
                    _cachedRuleSet.Rules.Count);
            }

            return _cachedRuleSet.Rules;
        }
        finally
        {
            _cacheLock.Release();
        }
    }
}