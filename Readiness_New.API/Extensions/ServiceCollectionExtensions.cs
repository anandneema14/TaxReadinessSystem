using Readiness_New.API.Configurations;
using Readiness_New.API.Services;
using Readiness_new.RA;

namespace Readiness_New.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRuleEngine(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RuleEngineOptions>(configuration.GetSection("RuleEngine"));
        services.AddSingleton<IRuleRepository, JsonRuleLoader>();
        services.AddSingleton<IRuleEngine, RuleEngineService>();
        return services;
    } 
}