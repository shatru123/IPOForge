using IPOForge.Application.Interfaces;
using IPOForge.Contracts.Common;
using IPOForge.Contracts.Company;
using IPOForge.Contracts.Ipo;
using IPOForge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IPOForge.Infrastructure.Services;

public class CompanyService : ICompanyService
{
    private readonly IpoForgeDbContext _context;
    private readonly IFinancialAnalysisEngine _financialEngine;

    public CompanyService(IpoForgeDbContext context, IFinancialAnalysisEngine financialEngine)
    {
        _context = context;
        _financialEngine = financialEngine;
    }

    public async Task<PagedResult<CompanySummaryDto>> GetCompaniesAsync(int page = 1, int pageSize = 20, string? search = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Companies
            .AsNoTracking()
            .Include(c => c.Financials)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(s) || c.Sector.ToLower().Contains(s) || c.Industry.ToLower().Contains(s));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(c =>
        {
            var latestFin = c.Financials.OrderBy(f => f.PeriodEnding).LastOrDefault();
            return new CompanySummaryDto
            {
                Id = c.Id,
                Name = c.Name,
                LegalName = c.LegalName,
                CIN = c.CIN,
                Symbol = c.Symbol,
                Sector = c.Sector,
                Industry = c.Industry,
                Description = c.Description,
                Website = c.Website,
                FoundedYear = c.FoundedYear,
                Headquarters = c.Headquarters,
                PromoterInformation = c.PromoterInformation,
                LatestRevenue = latestFin?.Revenue,
                LatestPAT = latestFin?.PAT,
                LatestROE = latestFin?.ROE
            };
        }).ToList();

        return new PagedResult<CompanySummaryDto>
        {
            Items = dtos,
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<CompanyDetailDto?> GetCompanyByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var company = await _context.Companies
            .AsNoTracking()
            .Include(c => c.Financials)
            .Include(c => c.Ipos)
                .ThenInclude(i => i.GmpHistories)
            .Include(c => c.Ipos)
                .ThenInclude(i => i.Scores)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (company == null) return null;

        var finReport = _financialEngine.AnalyzeFinancials(company);
        var latestFin = company.Financials.OrderBy(f => f.PeriodEnding).LastOrDefault();

        var ipoDtos = company.Ipos.Select(i =>
        {
            var latestGmp = i.GmpHistories.OrderBy(g => g.ObservedAt).LastOrDefault();
            var latestScore = i.Scores.OrderBy(sc => sc.CalculatedAt).LastOrDefault();
            return new IpoSummaryDto
            {
                Id = i.Id,
                CompanyId = company.Id,
                Name = i.Name,
                Symbol = i.Symbol,
                Sector = company.Sector,
                Industry = company.Industry,
                IpoType = i.IpoType,
                Status = i.Status,
                OpenDate = i.OpenDate,
                CloseDate = i.CloseDate,
                AllotmentDate = i.AllotmentDate,
                ListingDate = i.ListingDate,
                PriceBandLow = i.PriceBandLow,
                PriceBandHigh = i.PriceBandHigh,
                LotSize = i.LotSize,
                MinimumInvestment = i.MinimumInvestment,
                IssueSize = i.IssueSize,
                FreshIssueAmount = i.FreshIssueAmount,
                OFSAmount = i.OFSAmount,
                LatestGmp = latestGmp?.GMP,
                LatestGmpPercentage = latestGmp?.GMPPercentage,
                ListingGainScore = latestScore?.ListingGainScore,
                ListingRecommendation = latestScore?.ListingRecommendation,
                LongTermScore = latestScore?.LongTermScore,
                LongTermRecommendation = latestScore?.LongTermRecommendation
            };
        }).ToList();

        return new CompanyDetailDto
        {
            Id = company.Id,
            Name = company.Name,
            LegalName = company.LegalName,
            CIN = company.CIN,
            Symbol = company.Symbol,
            Sector = company.Sector,
            Industry = company.Industry,
            Description = company.Description,
            Website = company.Website,
            FoundedYear = company.FoundedYear,
            Headquarters = company.Headquarters,
            PromoterInformation = company.PromoterInformation,
            ManagingDirector = company.ManagingDirector,
            PromoterHoldingPreIssue = company.PromoterHoldingPreIssue,
            PromoterHoldingPostIssue = company.PromoterHoldingPostIssue,
            LatestRevenue = latestFin?.Revenue,
            LatestPAT = latestFin?.PAT,
            LatestROE = latestFin?.ROE,
            Financials = finReport.Years,
            Growth = finReport.GrowthAnalysis,
            AssociatedIpos = ipoDtos
        };
    }
}
