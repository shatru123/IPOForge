using IPOForge.Application.Interfaces;

namespace IPOForge.Application.Services;

public class AiAnalysisProvider : IAiAnalysisProvider
{
    public Task<string> SummarizeBusinessModelAsync(string companyName, string sector, string description, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return Task.FromResult($"{companyName} operates in the {sector} sector.");
        }

        var summary = $"{companyName} is an established enterprise operating in the Indian {sector} landscape. " +
                      $"{description} The company drives its core revenues through high-value product deliveries and strategic client contracts.";
        return Task.FromResult(summary);
    }

    public Task<string> SummarizeKeyRisksAsync(string companyName, IReadOnlyList<string> rawRiskStatements, CancellationToken cancellationToken = default)
    {
        if (!rawRiskStatements.Any())
        {
            return Task.FromResult("No abnormal operational risk factors have been highlighted outside standard industry cycles.");
        }

        var count = rawRiskStatements.Count;
        var summary = $"{companyName} has {count} notable operational and financial risk factors outlined in its offer documents. " +
                      string.Join(" ", rawRiskStatements.Take(3).Select(r => $"• {r}"));
        return Task.FromResult(summary);
    }
}
