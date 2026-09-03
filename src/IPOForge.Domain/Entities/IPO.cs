using IPOForge.Domain.Common;
using IPOForge.Domain.Enums;

namespace IPOForge.Domain.Entities;

public class IPO : AuditableEntity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public IpoType IpoType { get; set; } = IpoType.Mainboard;
    public IpoStatus Status { get; set; } = IpoStatus.Upcoming;

    // Key Dates
    public DateTime? OpenDate { get; set; }
    public DateTime? CloseDate { get; set; }
    public DateTime? AllotmentDate { get; set; }
    public DateTime? RefundDate { get; set; }
    public DateTime? CreditOfSharesDate { get; set; }
    public DateTime? ListingDate { get; set; }

    // Issue Pricing & Sizing
    public decimal PriceBandLow { get; set; }
    public decimal PriceBandHigh { get; set; }
    public int LotSize { get; set; }
    public decimal MinimumInvestment { get; set; } // PriceBandHigh * LotSize
    public decimal IssueSize { get; set; } // ₹ Crores
    public decimal FreshIssueAmount { get; set; } // ₹ Crores
    public decimal OFSAmount { get; set; } // ₹ Crores
    public decimal FaceValue { get; set; } = 10; // ₹ per share

    // Intermediaries & Exchange
    public string Registrar { get; set; } = string.Empty;
    public string LeadManagers { get; set; } = string.Empty;
    public string Exchange { get; set; } = "BSE, NSE";

    // Listing Performance (populated once listed)
    public decimal? ListingPrice { get; set; }
    public decimal? ListingGainPercent { get; set; }
    public decimal? Day1ClosePrice { get; set; }

    // Relationships
    public ICollection<IPOGmpHistory> GmpHistories { get; set; } = new List<IPOGmpHistory>();
    public ICollection<IPOSubscriptionHistory> SubscriptionHistories { get; set; } = new List<IPOSubscriptionHistory>();
    public ICollection<IPOObjective> Objectives { get; set; } = new List<IPOObjective>();
    public ICollection<IPORisk> Risks { get; set; } = new List<IPORisk>();
    public ICollection<IPOScore> Scores { get; set; } = new List<IPOScore>();
}
