using IPOForge.Application.Interfaces;
using IPOForge.Contracts.Admin;
using IPOForge.Contracts.Common;
using Microsoft.AspNetCore.Mvc;

namespace IPOForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IDataRefreshService _refreshService;

    public AdminController(IDataRefreshService refreshService)
    {
        _refreshService = refreshService;
    }

    [HttpPost("data-refresh")]
    public async Task<ActionResult<ApiResponse<DataRefreshStatusDto>>> TriggerDataRefresh(
        [FromBody] DataRefreshRequest? request,
        CancellationToken cancellationToken)
    {
        request ??= new DataRefreshRequest();
        var status = await _refreshService.RefreshMarketDataAsync(request, cancellationToken);
        return Ok(ApiResponse<DataRefreshStatusDto>.Ok(status));
    }

    [HttpGet("refresh-logs")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<Domain.Entities.DataRefreshLog>>>> GetRefreshLogs(
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var logs = await _refreshService.GetRefreshLogsAsync(limit, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<Domain.Entities.DataRefreshLog>>.Ok(logs));
    }

    [HttpGet("data-sources")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DataSourceStatusDto>>>> GetDataSources(CancellationToken cancellationToken)
    {
        var sources = await _refreshService.GetDataSourcesAsync(cancellationToken);
        var dtos = sources.Select(s => new DataSourceStatusDto
        {
            Id = s.Id,
            Name = s.Name,
            ProviderKey = s.ProviderKey,
            SourceType = s.SourceType,
            IsActive = s.IsActive,
            LastSyncAt = s.LastSyncAt,
            HealthStatus = s.HealthStatus,
            ErrorCount = s.ErrorCount
        }).ToList();

        return Ok(ApiResponse<IReadOnlyList<DataSourceStatusDto>>.Ok(dtos));
    }
}
