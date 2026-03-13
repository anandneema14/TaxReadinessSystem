using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Readiness_New.API.Configurations;
using Readiness_New.Data;
using Readiness_new.RA;

namespace Readiness_New.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReadinessController : ControllerBase
{
    private readonly IRuleEngine _ruleEngine;
    private readonly IRuleRepository _ruleLoader;
    private readonly ILogger<ReadinessController> _logger;
    private readonly RuleEngineOptions _options;
    private static RuleSet? _cachedRuleSet;
    private static readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1);

    public ReadinessController(IRuleEngine ruleEngine,
        IRuleRepository ruleLoader, ILogger<ReadinessController> logger,
        IOptions<RuleEngineOptions> options)
    {
        _ruleEngine = ruleEngine;
        _ruleLoader = ruleLoader;
        _logger = logger;
        _options = options.Value;
    }

    [HttpPost("score")]
    [ProducesResponseType(typeof(ReadinessScore), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ReadinessScore>> CalculateScore([FromBody] TaxReturn taxReturn)
    {
        try
        {
            if (taxReturn == null)
            {
                return BadRequest("Tax return cannot be null");
            }

            var rules = await GetRulesAsync();
            var score = _ruleEngine.CalculateReadinessScore(taxReturn, rules);

            _logger.LogInformation(
                "Calculated readiness score for taxpayer {TaxpayerId}. Score: {Score}, Level: {Level}",
                taxReturn.TaxpayerId, score.Score, score.Level);
            return Ok(score);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating readiness score");
            return StatusCode(500, "An error occurred while calculating the readiness score");
        }
    }

    [HttpPost("score/dictionary")]
    [ProducesResponseType(typeof(ReadinessScore), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ReadinessScore>> CalculateScoreData(
        [FromBody] Dictionary<string, object> taxReturnData)
    {
        try
        {
            if (taxReturnData == null || taxReturnData.Count == 0)
            {
                return BadRequest("Tax return data cannot be null or empty");
            }

            var rules = await GetRulesAsync();
            var score = _ruleEngine.CalculateReadinessScore(taxReturnData, rules);

            _logger.LogInformation("Calculated readiness score for dictionary data. Score: {Score}, Level: {Level}",
                score.Score, score.Level);
            return Ok(score);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating readiness score data");
            return StatusCode(500, "An error occurred while calculating the readiness score");
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
                _cachedRuleSet = await _ruleLoader.LoadRulesAsync(_options.RulesPath);
            }

            return _cachedRuleSet?.Rules ?? new List<Rule>();
        }
        finally
        {
            _cacheLock.Release();
        }
    }
}
