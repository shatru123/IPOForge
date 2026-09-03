using IPOForge.Domain.Common;
using IPOForge.Domain.Enums;

namespace IPOForge.Domain.Entities;

public class IPOObjective : AuditableEntity
{
    public Guid IpoId { get; set; }
    public IPO IPO { get; set; } = null!;

    public ObjectiveCategory Category { get; set; } = ObjectiveCategory.GeneralCorporate;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal AmountInCrores { get; set; }
    public decimal PercentageOfTotal { get; set; }
}
