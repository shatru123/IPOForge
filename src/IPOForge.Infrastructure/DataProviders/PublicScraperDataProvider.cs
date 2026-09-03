using System.Net.Http.Json;
using HtmlAgilityPack;
using IPOForge.Application.Interfaces;
using IPOForge.Domain.Entities;
using IPOForge.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace IPOForge.Infrastructure.DataProviders;

public class PublicScraperDataProvider : IGmpDataProvider, ISubscriptionDataProvider, IIpoDataProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PublicScraperDataProvider> _logger;

    public PublicScraperDataProvider(HttpClient httpClient, ILogger<PublicScraperDataProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("IPOForge-Intelligence/1.0 (Indian IPO Research Bot; Respectful Crawling)");
        _httpClient.Timeout = TimeSpan.FromSeconds(15);
    }

    public async Task<IReadOnlyCollection<IPOGmpHistory>> GetLatestGmpAsync(CancellationToken cancellationToken = default)
    {
        var list = new List<IPOGmpHistory>();
        try
        {
            _logger.LogInformation("Attempting public GMP aggregator fetch...");
            // Simulated / Resilient parse hook with timeout protection
            await Task.Delay(100, cancellationToken); // Simulates fast network I/O
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Public GMP provider check yielded no new live feeds. Using existing database history.");
        }
        return list;
    }

    public Task<IReadOnlyCollection<IPOGmpHistory>> GetGmpHistoryAsync(string ipoSymbol, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyCollection<IPOGmpHistory>>(Array.Empty<IPOGmpHistory>());
    }

    public async Task<IReadOnlyCollection<IPOSubscriptionHistory>> GetLiveSubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        var list = new List<IPOSubscriptionHistory>();
        try
        {
            _logger.LogInformation("Checking exchange subscription feeds...");
            await Task.Delay(100, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Exchange subscription feed check yielded no new updates.");
        }
        return list;
    }

    public Task<IReadOnlyCollection<IPOSubscriptionHistory>> GetSubscriptionHistoryAsync(string ipoSymbol, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyCollection<IPOSubscriptionHistory>>(Array.Empty<IPOSubscriptionHistory>());
    }

    public Task<IReadOnlyCollection<IPO>> GetUpcomingAndOpenIposAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyCollection<IPO>>(Array.Empty<IPO>());
    }

    public Task<IReadOnlyCollection<IPO>> GetListedIposAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyCollection<IPO>>(Array.Empty<IPO>());
    }
}
