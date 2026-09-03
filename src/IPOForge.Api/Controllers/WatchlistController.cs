using System.Security.Claims;
using IPOForge.Application.Interfaces;
using IPOForge.Contracts.Common;
using IPOForge.Contracts.Watchlist;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IPOForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WatchlistController : ControllerBase
{
    private readonly IWatchlistService _watchlistService;

    public WatchlistController(IWatchlistService watchlistService)
    {
        _watchlistService = watchlistService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<WatchlistItemDto>>>> GetWatchlist(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var list = await _watchlistService.GetUserWatchlistAsync(userId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<WatchlistItemDto>>.Ok(list));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<WatchlistItemDto>>> AddToWatchlist(
        [FromBody] AddToWatchlistRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var item = await _watchlistService.AddToWatchlistAsync(userId, request, cancellationToken);
        return Ok(ApiResponse<WatchlistItemDto>.Ok(item));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveFromWatchlist(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var success = await _watchlistService.RemoveFromWatchlistAsync(userId, id, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(success));
    }

    private string GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        return string.IsNullOrWhiteSpace(claim) ? "guest_user_session" : claim;
    }
}
