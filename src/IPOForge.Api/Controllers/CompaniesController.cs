using IPOForge.Application.Interfaces;
using IPOForge.Contracts.Common;
using IPOForge.Contracts.Company;
using Microsoft.AspNetCore.Mvc;

namespace IPOForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _companyService;

    public CompaniesController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<CompanySummaryDto>>>> GetCompanies(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _companyService.GetCompaniesAsync(page, pageSize, search, cancellationToken);
        return Ok(ApiResponse<PagedResult<CompanySummaryDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CompanyDetailDto>>> GetCompanyById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _companyService.GetCompanyByIdAsync(id, cancellationToken);
        if (result == null)
        {
            return NotFound(ApiResponse<CompanyDetailDto>.Fail("COMPANY_NOT_FOUND", $"Company with ID {id} was not found."));
        }
        return Ok(ApiResponse<CompanyDetailDto>.Ok(result));
    }
}
