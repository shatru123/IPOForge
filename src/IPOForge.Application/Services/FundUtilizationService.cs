using IPOForge.Application.Interfaces;
using IPOForge.Contracts.Analysis;
using IPOForge.Domain.Entities;
using IPOForge.Domain.Enums;

namespace IPOForge.Application.Services;

public class FundUtilizationService : IFundUtilizationService
{
    public FundUtilizationDto AnalyzeFundUtilization(IPO ipo)
    {
        var result = new FundUtilizationDto
        {
            IpoId = ipo.Id,
            TotalIssueSize = ipo.IssueSize,
            FreshIssueAmount = ipo.FreshIssueAmount,
            OfsAmount = ipo.OFSAmount
        };

        if (ipo.IssueSize > 0)
        {
            result.FreshIssuePercent = Math.Round((ipo.FreshIssueAmount / ipo.IssueSize) * 100, 2);
            result.OfsPercent = Math.Round((ipo.OFSAmount / ipo.IssueSize) * 100, 2);
        }

        var objList = ipo.Objectives.ToList();
        var totalAmount = objList.Sum(o => o.AmountInCrores);

        var allocationDtos = new List<ObjectiveAllocationItemDto>();

        foreach (var obj in objList)
        {
            var pct = totalAmount > 0 ? Math.Round((obj.AmountInCrores / totalAmount) * 100, 2) : obj.PercentageOfTotal;

            switch (obj.Category)
            {
                case ObjectiveCategory.DebtRepayment:
                    result.DebtRepaymentPercent += pct;
                    break;
                case ObjectiveCategory.Expansion:
                    result.ExpansionCapexPercent += pct;
                    break;
                case ObjectiveCategory.WorkingCapital:
                    result.WorkingCapitalPercent += pct;
                    break;
                case ObjectiveCategory.GeneralCorporate:
                    result.GeneralCorporatePercent += pct;
                    break;
                case ObjectiveCategory.OfferForSale:
                case ObjectiveCategory.Other:
                default:
                    result.OtherUtilizationPercent += pct;
                    break;
            }

            allocationDtos.Add(new ObjectiveAllocationItemDto
            {
                Id = obj.Id,
                Category = obj.Category,
                Title = obj.Title,
                Description = obj.Description,
                AmountInCrores = obj.AmountInCrores,
                Percentage = pct
            });
        }

        result.Objectives = allocationDtos;

        // Contextual assessment of OFS vs Fresh
        if (result.FreshIssuePercent >= 80)
        {
            result.OfsAssessment = "Primarily Fresh Capital (>80% Fresh Issue). Proceeds will directly expand manufacturing capacity, reduce debt, and strengthen the company balance sheet.";
        }
        else if (result.OfsPercent >= 75)
        {
            result.OfsAssessment = "Heavy Offer for Sale (>75% OFS). Early institutional / private equity investors or promoters are monetizing equity. Company operations receive only a minority portion of the capital raised.";
        }
        else
        {
            result.OfsAssessment = $"Balanced mix ({result.FreshIssuePercent:F0}% Fresh Issue, {result.OfsPercent:F0}% OFS). Provides partial liquidity for early investors while infusing growth equity into company operations.";
        }

        return result;
    }
}
