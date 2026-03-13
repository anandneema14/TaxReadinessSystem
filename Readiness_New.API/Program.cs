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

        var useAzureAppConfig = false;
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

                // Add Azure App Configuration services
                builder.Services.AddAzureAppConfiguration();
                useAzureAppConfig = true;
            }
        }

        // Add services to the container.
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAngular", policy =>
            {
                policy.WithOrigins("http://localhost:4200") // Default Angular port
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

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

        app.UseCors("AllowAngular");

        // Only use Azure App Configuration middleware if it was configured
        if (useAzureAppConfig)
        {
            app.UseAzureAppConfiguration();
        }

        app.UseAuthorization();
        
        app.MapControllers();

        app.Run();
    }
}