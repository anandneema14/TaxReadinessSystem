using Readiness_New.API.Extensions;
using Readiness_New.API.Configurations;
using Readiness_New.API.Data;
using Readiness_New.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Readiness_New.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add Azure App Configuration
        var connectionString = builder.Configuration.GetConnectionString("AppConfig");
        var ruleEngineSection = builder.Configuration.GetSection("RuleEngine");
        var provider = ruleEngineSection["Provider"];

        bool useAzureAppConfig = false;

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

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? "a-very-long-and-secure-secret-key-that-must-be-at-least-32-chars";

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"] ?? "ReadinessApp",
                ValidAudience = jwtSettings["Audience"] ?? "ReadinessAppUsers",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
            };
        });

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
            });

            options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });

        builder.Services.AddRuleEngine(builder.Configuration);

        var app = builder.Build();

        // Seed initial data
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                context.Database.Migrate();
                await DbSeeder.SeedRolesAndAdminAsync(services);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseCors("AllowAngular");

        if (useAzureAppConfig)
        {
            app.UseAzureAppConfiguration();
        }

        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapControllers();

        app.Run();
    }
}