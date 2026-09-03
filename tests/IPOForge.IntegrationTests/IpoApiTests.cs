using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using IPOForge.Contracts.Common;
using IPOForge.Contracts.Dashboard;
using IPOForge.Contracts.Ipo;
using Xunit;

namespace IPOForge.IntegrationTests;

public class IpoApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public IpoApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_Returns_Success()
    {
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetDashboard_Returns_Populated_Summary()
    {
        var response = await _client.GetAsync("/api/dashboard");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var content = JsonSerializer.Deserialize<ApiResponse<DashboardSummaryDto>>(json, JsonOptions);
        content.Should().NotBeNull();
        content!.Success.Should().BeTrue();
        content.Data.Should().NotBeNull();
        content.Data!.TotalIposThisYear.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetIpos_Returns_Paged_Result_With_Seed_Data()
    {
        var response = await _client.GetAsync("/api/ipos?pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var content = JsonSerializer.Deserialize<ApiResponse<PagedResult<IpoSummaryDto>>>(json, JsonOptions);
        content.Should().NotBeNull();
        content!.Success.Should().BeTrue();
        content.Data.Should().NotBeNull();
        content.Data!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SearchIpos_Finds_Matching_Records()
    {
        var response = await _client.GetAsync("/api/ipos/search?q=Tata");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var content = JsonSerializer.Deserialize<ApiResponse<IReadOnlyList<IpoSearchDto>>>(json, JsonOptions);
        content.Should().NotBeNull();
        content!.Success.Should().BeTrue();
        content.Data.Should().NotBeEmpty();
        content.Data!.First().Name.Should().Contain("Tata");
    }
}
