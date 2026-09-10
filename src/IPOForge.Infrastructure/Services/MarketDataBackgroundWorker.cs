using IPOForge.Application.Interfaces;
using IPOForge.Contracts.Admin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IPOForge.Infrastructure.Services;

public class MarketDataBackgroundWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MarketDataBackgroundWorker> _logger;

    public MarketDataBackgroundWorker(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<MarketDataBackgroundWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var isEnabled = _configuration.GetValue<bool?>("BackgroundWorker:Enabled") ?? true;
        if (!isEnabled)
        {
            _logger.LogInformation("MarketDataBackgroundWorker is disabled by configuration.");
            return;
        }

        _logger.LogInformation("IPOForge Market Data Background Worker initialized. Automated market sync active.");

        // Wait 12 seconds after startup to allow DB initializer to complete cleanly
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(12), stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Executing automated background market data synchronization...");
                using var scope = _serviceProvider.CreateScope();
                var refreshService = scope.ServiceProvider.GetRequiredService<IDataRefreshService>();

                var result = await refreshService.RefreshMarketDataAsync(
                    new DataRefreshRequest { ForceFullSync = true },
                    stoppingToken);

                _logger.LogInformation(
                    "Automated background refresh completed with status: {Status}, records processed: {Count}",
                    result.Status,
                    result.RecordsProcessed);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in automated market data background refresh cycle.");
            }

            var nextInterval = CalculateNextRefreshInterval();
            _logger.LogInformation("Next automated market data sync scheduled in {Minutes} minutes.", nextInterval.TotalMinutes);

            try
            {
                await Task.Delay(nextInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private TimeSpan CalculateNextRefreshInterval()
    {
        var configuredMinutes = _configuration.GetValue<int?>("BackgroundWorker:IntervalMinutes");
        if (configuredMinutes.HasValue && configuredMinutes.Value > 0)
        {
            return TimeSpan.FromMinutes(configuredMinutes.Value);
        }

        // Indian Standard Time (IST = UTC + 5:30)
        var istNow = DateTime.UtcNow.AddHours(5).AddMinutes(30);
        var isWeekday = istNow.DayOfWeek != DayOfWeek.Saturday && istNow.DayOfWeek != DayOfWeek.Sunday;

        // Active Indian Market / Bidding & Grey Market trading hours: 9:00 AM to 6:30 PM IST on weekdays
        var isMarketHours = isWeekday && istNow.Hour >= 9 && (istNow.Hour < 18 || (istNow.Hour == 18 && istNow.Minute <= 30));

        if (isMarketHours)
        {
            // During active market trading hours: refresh every 20 minutes for live GMP and bidding updates
            return TimeSpan.FromMinutes(20);
        }

        // During off-market hours and weekends: refresh every 60 minutes
        return TimeSpan.FromMinutes(60);
    }
}
