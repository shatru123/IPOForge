using System.Diagnostics;
using IPOForge.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace IPOForge.Api.Controllers;

[ApiController]
public class HealthController : ControllerBase
{
    private readonly IpoForgeDbContext _dbContext;
    private static readonly DateTime _startTime = DateTime.UtcNow;

    public HealthController(IpoForgeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("health")]
    [HttpGet("healthz")]
    [HttpGet("api/health")]
    public async Task<IActionResult> GetHealth(CancellationToken cancellationToken)
    {
        bool dbHealthy = false;
        string dbProvider = "Unknown";
        try
        {
            dbProvider = _dbContext.Database.ProviderName ?? "Unknown";
            dbHealthy = await _dbContext.Database.CanConnectAsync(cancellationToken);
        }
        catch
        {
            dbHealthy = false;
        }

        var uptime = DateTime.UtcNow - _startTime;

        var response = new
        {
            status = dbHealthy ? "Healthy" : "Degraded",
            service = "IPOForge Intelligence Platform",
            version = "1.0.0",
            author = new
            {
                name = "Shatrughna Ambhore",
                email = "ambhoreshatrughna@gmail.com",
                phone = "+91 9604466334"
            },
            database = new
            {
                connected = dbHealthy,
                provider = dbProvider
            },
            uptime = new
            {
                days = uptime.Days,
                hours = uptime.Hours,
                minutes = uptime.Minutes,
                seconds = uptime.Seconds,
                totalSeconds = (long)uptime.TotalSeconds
            },
            system = new
            {
                memoryUsageMb = Process.GetCurrentProcess().WorkingSet64 / (1024 * 1024),
                threadCount = Process.GetCurrentProcess().Threads.Count
            },
            timestamp = DateTime.UtcNow
        };

        return dbHealthy ? Ok(response) : StatusCode(503, response);
    }

    [HttpGet("health/ready")]
    [HttpGet("healthz/ready")]
    [HttpGet("api/health/ready")]
    public async Task<IActionResult> GetReady(CancellationToken cancellationToken)
    {
        var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
        return canConnect
            ? Ok(new { status = "Ready", timestamp = DateTime.UtcNow })
            : StatusCode(503, new { status = "NotReady", timestamp = DateTime.UtcNow });
    }
}
