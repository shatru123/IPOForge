using IPOForge.Application.Interfaces;
using IPOForge.Contracts.Analysis;
using IPOForge.Contracts.Common;
using IPOForge.Contracts.Financial;
using IPOForge.Contracts.Gmp;
using IPOForge.Contracts.Ipo;
using IPOForge.Contracts.Subscription;
using Microsoft.AspNetCore.Mvc;

namespace IPOForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IposController : ControllerBase
{
    private readonly IIpoService _ipoService;

    public IposController(IIpoService ipoService)
    {
        _ipoService = ipoService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<IpoSummaryDto>>>> GetIpos(
        [FromQuery] IpoFilterRequest filter,
        CancellationToken cancellationToken)
    {
        var result = await _ipoService.GetIposAsync(filter, cancellationToken);
        return Ok(ApiResponse<PagedResult<IpoSummaryDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<IpoDetailDto>>> GetIpoById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _ipoService.GetIpoByIdAsync(id, cancellationToken);
        if (result == null)
        {
            return NotFound(ApiResponse<IpoDetailDto>.Fail("IPO_NOT_FOUND", $"IPO with ID {id} was not found."));
        }
        return Ok(ApiResponse<IpoDetailDto>.Ok(result));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<IpoSearchDto>>>> SearchIpos(
        [FromQuery] string q,
        CancellationToken cancellationToken)
    {
        var result = await _ipoService.SearchIposAsync(q, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<IpoSearchDto>>.Ok(result));
    }

    [HttpGet("upcoming")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<IpoSummaryDto>>>> GetUpcomingIpos(CancellationToken cancellationToken)
    {
        var result = await _ipoService.GetUpcomingIposAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<IpoSummaryDto>>.Ok(result));
    }

    [HttpGet("open")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<IpoSummaryDto>>>> GetOpenIpos(CancellationToken cancellationToken)
    {
        var result = await _ipoService.GetOpenIposAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<IpoSummaryDto>>.Ok(result));
    }

    [HttpGet("listed")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<IpoSummaryDto>>>> GetListedIpos(CancellationToken cancellationToken)
    {
        var result = await _ipoService.GetListedIposAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<IpoSummaryDto>>.Ok(result));
    }

    [HttpGet("{id:guid}/gmp")]
    public async Task<ActionResult<ApiResponse<GmpHistoryDto>>> GetGmp(Guid id, CancellationToken cancellationToken)
    {
        var result = await _ipoService.GetGmpHistoryAsync(id, cancellationToken);
        if (result == null)
        {
            return NotFound(ApiResponse<GmpHistoryDto>.Fail("GMP_NOT_FOUND", "GMP history is not available for this IPO."));
        }
        return Ok(ApiResponse<GmpHistoryDto>.Ok(result));
    }

    [HttpGet("{id:guid}/subscriptions")]
    public async Task<ActionResult<ApiResponse<SubscriptionBreakdownDto>>> GetSubscriptions(Guid id, CancellationToken cancellationToken)
    {
        var result = await _ipoService.GetSubscriptionAsync(id, cancellationToken);
        if (result == null)
        {
            return NotFound(ApiResponse<SubscriptionBreakdownDto>.Fail("SUBSCRIPTION_NOT_FOUND", "Subscription data is not available for this IPO."));
        }
        return Ok(ApiResponse<SubscriptionBreakdownDto>.Ok(result));
    }

    [HttpGet("{id:guid}/financials")]
    public async Task<ActionResult<ApiResponse<CompanyFinancialReportDto>>> GetFinancials(Guid id, CancellationToken cancellationToken)
    {
        var result = await _ipoService.GetFinancialsAsync(id, cancellationToken);
        if (result == null)
        {
            return NotFound(ApiResponse<CompanyFinancialReportDto>.Fail("FINANCIALS_NOT_FOUND", "Financial report is not available for this IPO."));
        }
        return Ok(ApiResponse<CompanyFinancialReportDto>.Ok(result));
    }

    [HttpGet("{id:guid}/analysis")]
    public async Task<ActionResult<ApiResponse<ComprehensiveAnalysisDto>>> GetAnalysis(Guid id, CancellationToken cancellationToken)
    {
        var result = await _ipoService.GetAnalysisAsync(id, cancellationToken);
        if (result == null)
        {
            return NotFound(ApiResponse<ComprehensiveAnalysisDto>.Fail("ANALYSIS_NOT_FOUND", "Analytical report is not available for this IPO."));
        }
        return Ok(ApiResponse<ComprehensiveAnalysisDto>.Ok(result));
    }

    [HttpGet("{id:guid}/score")]
    public async Task<ActionResult<ApiResponse<ScoreBreakdownDto>>> GetScore(Guid id, CancellationToken cancellationToken)
    {
        var result = await _ipoService.GetScoreAsync(id, cancellationToken);
        if (result == null)
        {
            return NotFound(ApiResponse<ScoreBreakdownDto>.Fail("SCORE_NOT_FOUND", "Score breakdown is not available for this IPO."));
        }
        return Ok(ApiResponse<ScoreBreakdownDto>.Ok(result));
    }
}
