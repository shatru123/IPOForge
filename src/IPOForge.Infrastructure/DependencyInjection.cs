using System.Text;
using IPOForge.Application.Interfaces;
using IPOForge.Application.Services;
using IPOForge.Infrastructure.DataProviders;
using IPOForge.Infrastructure.Persistence;
using IPOForge.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace IPOForge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var isInMemory = string.IsNullOrWhiteSpace(connectionString) ||
                         connectionString.StartsWith("InMemory", StringComparison.OrdinalIgnoreCase);

        if (isInMemory)
        {
            var dbName = connectionString != null && connectionString.StartsWith("InMemory:")
                ? connectionString.Substring("InMemory:".Length)
                : "IPOForgeDb";

            services.AddDbContext<IpoForgeDbContext>(options =>
                options.UseInMemoryDatabase(dbName));
        }
        else
        {
            services.AddDbContext<IpoForgeDbContext>(options =>
                options.UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(IpoForgeDbContext).Assembly.FullName)));
        }

        // ASP.NET Core Identity
        services.AddIdentityCore<IdentityUser>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 6;
        })
        .AddRoles<IdentityRole>()
        .AddRoleManager<RoleManager<IdentityRole>>()
        .AddEntityFrameworkStores<IpoForgeDbContext>()
        .AddDefaultTokenProviders();

        // JWT Authentication
        var secretKey = configuration["Jwt:SecretKey"] ?? "IPOForge_Super_Secret_Production_Grade_JWT_Key_2025_Min32Bytes!";
        var issuer = configuration["Jwt:Issuer"] ?? "IPOForge.Api";
        var audience = configuration["Jwt:Audience"] ?? "IPOForge.Client";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
            };
        });

        services.AddAuthorization();

        // Caching
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();

        // Application Calculation Engines
        services.AddScoped<IIpoScoringEngine, IpoScoringEngine>();
        services.AddScoped<IFinancialAnalysisEngine, FinancialAnalysisEngine>();
        services.AddScoped<IValuationEngine, ValuationEngine>();
        services.AddScoped<IGmpAnalyticsService, GmpAnalyticsService>();
        services.AddScoped<IRiskEngine, RiskEngine>();
        services.AddScoped<IFundUtilizationService, FundUtilizationService>();
        services.AddScoped<IAiAnalysisProvider, AiAnalysisProvider>();

        // Business Services
        services.AddScoped<IIpoService, IpoService>();
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IWatchlistService, WatchlistService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDataRefreshService, DataRefreshService>();

        // External / Public Data Providers & HttpClient with resilience
        services.AddHttpClient<PublicScraperDataProvider>();
        services.AddScoped<IGmpDataProvider, PublicScraperDataProvider>();
        services.AddScoped<ISubscriptionDataProvider, PublicScraperDataProvider>();
        services.AddScoped<IIpoDataProvider, PublicScraperDataProvider>();

        return services;
    }
}
