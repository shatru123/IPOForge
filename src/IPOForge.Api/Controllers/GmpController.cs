using IPOForge.Application.Interfaces;
using IPOForge.Contracts.Common;
using IPOForge.Contracts.Gmp;
using IPOForge.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IPOForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GmpController : ControllerBase
{
    private readonly IGmpAnalyticsService _gmpAnalyticsService;
    private readonly IpoForgeDbContext _context;

    public GmpController(IGmpAnalyticsService gmpAnalyticsService, IpoForgeDbContext context)
    {
        _gmpAnalyticsService = gmpAnalyticsService;
        _context = context;
    }

    [HttpGet("movers")]
    [HttpGet("top-movers")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<GmpMoverDto>>>> GetGmpMovers(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        var allIpos = await _context.IPOs
            .AsNoTracking()
            .Include(i => i.GmpHistories)
            .ToListAsync(cancellationToken);

        var movers = _gmpAnalyticsService.GetTopMovers(allIpos, count);
        return Ok(ApiResponse<IReadOnlyList<GmpMoverDto>>.Ok(movers));
    }

    [HttpGet("analytics")]
    [HttpGet("accuracy")]
    public async Task<ActionResult<ApiResponse<GmpAccuracyAnalyticsDto>>> GetAccuracyAnalytics(CancellationToken cancellationToken = default)
    {
        var listedIpos = await _context.IPOs
            .AsNoTracking()
            .Include(i => i.GmpHistories)
            .Where(i => i.Status == Domain.Enums.IpoStatus.Listed)
            .ToListAsync(cancellationToken);

        var analytics = _gmpAnalyticsService.GetAccuracyAnalytics(listedIpos);
        return Ok(ApiResponse<GmpAccuracyAnalyticsDto>.Ok(analytics));
    }
}
