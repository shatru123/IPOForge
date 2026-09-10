using System.Text.Json.Serialization;
using IPOForge.Api.Middleware;
using IPOForge.Application.Interfaces;
using IPOForge.Contracts.Admin;
using IPOForge.Infrastructure;
using IPOForge.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Serilog;

// Prevent inotify limit issues in containerized environments (Render / Linux Docker)
Environment.SetEnvironmentVariable("DOTNET_USE_POLLING_FILE_WATCHER", "true");

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Configure Dynamic Port for Render Free Tier (0.0.0.0:${PORT:-5000})
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Add Infrastructure & Application Services
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add Controllers with Enum String formatting
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var frontendUrl = builder.Configuration["FrontendUrl"] ?? "http://localhost:5173";
        policy.WithOrigins(frontendUrl, "http://localhost:5173", "http://localhost:3000")
              .SetIsOriginAllowed(_ => true) // Allow Render dynamic preview domains
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configure Swagger/OpenAPI with Author Contact Info
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "IPOForge API — Indian IPO Intelligence Platform",
        Version = "v1",
        Description = "Research, analyze, and evaluate Indian Mainboard and SME IPOs with deterministic scoring, GMP analytics, and multi-year financials.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Shatrughna Ambhore",
            Email = "ambhoreshatrughna@gmail.com",
            Url = new Uri("https://github.com/shatru123/IPOForge")
        }
    });
});

var app = builder.Build();

// Seed database on startup and run live public data scanner
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<IpoForgeDbContext>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var scoring = services.GetRequiredService<IIpoScoringEngine>();
        var financial = services.GetRequiredService<IFinancialAnalysisEngine>();
        var valuation = services.GetRequiredService<IValuationEngine>();
        var gmp = services.GetRequiredService<IGmpAnalyticsService>();
        var risk = services.GetRequiredService<IRiskEngine>();

        await DbInitializer.InitializeAsync(
            context,
            userManager,
            roleManager,
            scoring,
            financial,
            valuation,
            gmp,
            risk,
            logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during database initialization.");
    }
}

// Global Exception Handling Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// Enable Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "IPOForge API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowFrontend");

// Serve SPA Static Files if present in wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Fallback for SPA Client-Side Routing
app.MapFallbackToFile("index.html");

Log.Information("IPOForge API is starting up on port {Port}...", port);
app.Run();

public partial class Program { }
