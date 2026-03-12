using Readiness_New.API.Extensions;
using Readiness_New.API.Configurations;

namespace Readiness_New.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add Azure App Configuration
        var connectionString = builder.Configuration.GetConnectionString("AppConfig");
        var ruleEngineSection = builder.Configuration.GetSection("RuleEngine");
        var provider = ruleEngineSection["Provider"];

        if (provider?.Equals("AzureAppConfig", StringComparison.OrdinalIgnoreCase) == true)
        {
            var appConfigConnString = ruleEngineSection["AzureAppConfig:ConnectionString"] ?? connectionString;
            if (!string.IsNullOrEmpty(appConfigConnString))
            {
                builder.Configuration.AddAzureAppConfiguration(options =>
                {
                    options.Connect(appConfigConnString)
                           .ConfigureRefresh(refresh =>
                           {
                               refresh.Register(ruleEngineSection["AzureAppConfig:Key"] ?? "Readiness:Rules", refreshAll: true)
                                      .SetCacheExpiration(TimeSpan.FromSeconds(30));
                           });
                });
            }
        }

        // Add services to the container.

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddRuleEngine(builder.Configuration);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAzureAppConfiguration();

        app.UseAuthorization();
        
        app.MapControllers();

        app.Run();
    }
}