using IPOForge.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IPOForge.Infrastructure.Persistence;

public class IpoForgeDbContext : IdentityDbContext<IdentityUser>
{
    public IpoForgeDbContext(DbContextOptions<IpoForgeDbContext> options) : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<CompanyFinancial> CompanyFinancials => Set<CompanyFinancial>();
    public DbSet<IPO> IPOs => Set<IPO>();
    public DbSet<IPOGmpHistory> IPOGmpHistories => Set<IPOGmpHistory>();
    public DbSet<IPOSubscriptionHistory> IPOSubscriptionHistories => Set<IPOSubscriptionHistory>();
    public DbSet<IPOObjective> IPOObjectives => Set<IPOObjective>();
    public DbSet<IPORisk> IPORisks => Set<IPORisk>();
    public DbSet<IPOScore> IPOScores => Set<IPOScore>();
    public DbSet<IndustryMetric> IndustryMetrics => Set<IndustryMetric>();
    public DbSet<Watchlist> Watchlists => Set<Watchlist>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<DataSource> DataSources => Set<DataSource>();
    public DbSet<DataRefreshLog> DataRefreshLogs => Set<DataRefreshLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Company
        builder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(250);
            entity.Property(e => e.LegalName).HasMaxLength(300);
            entity.Property(e => e.Sector).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Industry).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Symbol).HasMaxLength(50);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Sector);
            entity.HasIndex(e => e.Symbol);
        });

        // CompanyFinancial
        builder.Entity<CompanyFinancial>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FiscalYear).IsRequired().HasMaxLength(20);
            entity.HasOne(e => e.Company)
                .WithMany(c => c.Financials)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.CompanyId, e.FiscalYear }).IsUnique();
        });

        // IPO
        builder.Entity<IPO>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(250);
            entity.Property(e => e.Symbol).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PriceBandLow).HasPrecision(18, 2);
            entity.Property(e => e.PriceBandHigh).HasPrecision(18, 2);
            entity.Property(e => e.MinimumInvestment).HasPrecision(18, 2);
            entity.Property(e => e.IssueSize).HasPrecision(18, 2);
            entity.Property(e => e.FreshIssueAmount).HasPrecision(18, 2);
            entity.Property(e => e.OFSAmount).HasPrecision(18, 2);
            entity.Property(e => e.ListingPrice).HasPrecision(18, 2);
            entity.Property(e => e.ListingGainPercent).HasPrecision(18, 2);

            entity.HasOne(e => e.Company)
                .WithMany(c => c.Ipos)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.IpoType);
            entity.HasIndex(e => e.OpenDate);
            entity.HasIndex(e => e.CloseDate);
            entity.HasIndex(e => e.Symbol);
        });

        // IPOGmpHistory
        builder.Entity<IPOGmpHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.GMP).HasPrecision(18, 2);
            entity.Property(e => e.GMPPercentage).HasPrecision(18, 2);
            entity.Property(e => e.EstimatedListingPrice).HasPrecision(18, 2);
            entity.Property(e => e.KostakRate).HasPrecision(18, 2);
            entity.Property(e => e.SubjectToSauda).HasPrecision(18, 2);
            entity.Property(e => e.Confidence).HasPrecision(5, 2);

            entity.HasOne(e => e.IPO)
                .WithMany(i => i.GmpHistories)
                .HasForeignKey(e => e.IpoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.IpoId, e.ObservedAt });
        });

        // IPOSubscriptionHistory
        builder.Entity<IPOSubscriptionHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RetailSubscription).HasPrecision(18, 2);
            entity.Property(e => e.QibSubscription).HasPrecision(18, 2);
            entity.Property(e => e.NiiSubscription).HasPrecision(18, 2);
            entity.Property(e => e.TotalSubscription).HasPrecision(18, 2);

            entity.HasOne(e => e.IPO)
                .WithMany(i => i.SubscriptionHistories)
                .HasForeignKey(e => e.IpoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.IpoId, e.DayNumber });
        });

        // IPOObjective
        builder.Entity<IPOObjective>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(250);
            entity.Property(e => e.AmountInCrores).HasPrecision(18, 2);
            entity.Property(e => e.PercentageOfTotal).HasPrecision(18, 2);

            entity.HasOne(e => e.IPO)
                .WithMany(i => i.Objectives)
                .HasForeignKey(e => e.IpoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // IPORisk
        builder.Entity<IPORisk>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(250);

            entity.HasOne(e => e.IPO)
                .WithMany(i => i.Risks)
                .HasForeignKey(e => e.IpoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // IPOScore
        builder.Entity<IPOScore>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.IPO)
                .WithMany(i => i.Scores)
                .HasForeignKey(e => e.IpoId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.IpoId, e.CalculatedAt });
        });

        // IndustryMetric
        builder.Entity<IndustryMetric>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Sector).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Industry).IsRequired().HasMaxLength(150);
            entity.Property(e => e.MedianPE).HasPrecision(18, 2);
            entity.Property(e => e.MedianPB).HasPrecision(18, 2);
            entity.HasIndex(e => new { e.Sector, e.Industry }).IsUnique();
        });

        // Watchlist
        builder.Entity<Watchlist>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.IPO)
                .WithMany()
                .HasForeignKey(e => e.IpoId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserId, e.IpoId }).IsUnique();
        });
    }
}
