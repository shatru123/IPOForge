using IPOForge.Application.Interfaces;
using IPOForge.Contracts.Admin;

namespace IPOForge.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("IPOForge Background Worker Service started at: {time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Worker performing scheduled market data synchronization...");
                using var scope = _serviceProvider.CreateScope();
                var refreshService = scope.ServiceProvider.GetRequiredService<IDataRefreshService>();
                await refreshService.RefreshMarketDataAsync(new DataRefreshRequest(), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during worker background refresh cycle.");
            }

            // Wait 1 hour before next background cycle
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
