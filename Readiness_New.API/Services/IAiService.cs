using Readiness_New.Data;

namespace Readiness_New.API.Services;

public interface IAiService
{
    Task<string> GenerateSummaryAsync(ReadinessScore score);
}
