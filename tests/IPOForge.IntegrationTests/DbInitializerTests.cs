using FluentAssertions;
using IPOForge.Application.Services;
using IPOForge.Domain.Entities;
using IPOForge.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace IPOForge.IntegrationTests;

public class DbInitializerTests
{
    [Fact]
    public async Task InitializeAsync_Seeds_All_Companies_And_IPOs()
    {
        var options = new DbContextOptionsBuilder<IpoForgeDbContext>()
            .UseInMemoryDatabase($"TestSeedDb_{Guid.NewGuid()}")
            .Options;

        using var context = new IpoForgeDbContext(options);

        var userStore = new UserStore<IdentityUser>(context);
        var roleStore = new RoleStore<IdentityRole>(context);

        var userManager = new UserManager<IdentityUser>(
            userStore,
            Options.Create(new IdentityOptions()),
            new PasswordHasher<IdentityUser>(),
            Array.Empty<IUserValidator<IdentityUser>>(),
            Array.Empty<IPasswordValidator<IdentityUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!,
            new NullLogger<UserManager<IdentityUser>>());

        var roleManager = new RoleManager<IdentityRole>(
            roleStore,
            Array.Empty<IRoleValidator<IdentityRole>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            new NullLogger<RoleManager<IdentityRole>>());

        var scoring = new IpoScoringEngine();
        var financial = new FinancialAnalysisEngine();
        var valuation = new ValuationEngine();
        var gmp = new GmpAnalyticsService();
        var risk = new RiskEngine();

        await DbInitializer.InitializeAsync(
            context,
            userManager,
            roleManager,
            scoring,
            financial,
            valuation,
            gmp,
            risk,
            new NullLogger<DbInitializerTests>());

        var companies = await context.Companies.Include(c => c.Ipos).ToListAsync();
        companies.Should().HaveCount(7);

        var ipos = await context.IPOs.Include(i => i.Scores).ToListAsync();
        ipos.Should().HaveCount(7);
        ipos.All(i => i.Scores.Any()).Should().BeTrue();
    }
}
