using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using Readiness_New.API.Configurations;
using Readiness_New.Data;
using System.Text.Json;

namespace Readiness_New.API.Services;

public class AzureOpenAiService : IAiService
{
    private readonly AzureOpenAiOptions _options;
    private readonly ILogger<AzureOpenAiService> _logger;

    public AzureOpenAiService(IOptions<AzureOpenAiOptions> options, ILogger<AzureOpenAiService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> GenerateSummaryAsync(ReadinessScore score)
    {
        if (string.IsNullOrEmpty(_options.Key) || string.IsNullOrEmpty(_options.Endpoint))
        {
            _logger.LogWarning("Azure OpenAI configuration is missing. Returning default summary.");
            return GenerateDefaultSummary(score);
        }

        try
        {
            OpenAIClient client = new OpenAIClient(new Uri(_options.Endpoint), new AzureKeyCredential(_options.Key));

            var scoreJson = JsonSerializer.Serialize(score, new JsonSerializerOptions { WriteIndented = true });

            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = _options.DeploymentName,
                Messages =
                {
                    new ChatRequestSystemMessage("You are an expert tax advisor assistant. You provide concise, human-readable summaries of tax readiness scores. Be professional and helpful."),
                    new ChatRequestUserMessage($"Here is a structured readiness score output from a tax return validation system:\n\n{scoreJson}\n\nGenerate a clear summary."),
                    new ChatRequestAssistantMessage("Rules:"),
                    new ChatRequestAssistantMessage("1. Be concise and to the point."),
                    new ChatRequestAssistantMessage("2. Explain the readiness score in a way that is easy to understand."),
                    new ChatRequestAssistantMessage("3. Highlight top issues that need attention."),
                    new ChatRequestAssistantMessage("4. Suggest ways to improve the tax return."),
                    new ChatRequestAssistantMessage("5. Do not include any additional information.")
                },
                MaxTokens = 150,
                Temperature = 0.5f
            };

            Response<ChatCompletions> response = await client.GetChatCompletionsAsync(chatCompletionsOptions);
            string summary = response.Value.Choices[0].Message.Content;

            return summary.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating summary from Azure OpenAI");
            return GenerateDefaultSummary(score);
        }
    }

    private string GenerateDefaultSummary(ReadinessScore score)
    {
        var result = $"Your readiness score is {score.Score:F1} ({score.Level}). ";
        if (score.ValidationResult?.Violations?.Any() == true)
        {
            result += $"There are {score.ValidationResult.Violations.Count} issues identified that may need attention.";
        }
        else
        {
            result += "No major issues were identified in your tax return.";
        }
        return result;
    }
}
