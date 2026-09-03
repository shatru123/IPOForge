using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using IPOForge.Contracts.Auth;
using IPOForge.Contracts.Common;
using Xunit;

namespace IPOForge.IntegrationTests;

public class AuthApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public AuthApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_And_Login_Flow_Succeeds()
    {
        var uniqueEmail = $"investor_{Guid.NewGuid():N}@ipoforge.com";
        var regRequest = new RegisterRequest
        {
            FullName = "Alpha Investor",
            Email = uniqueEmail,
            Password = "SecurePassword123!"
        };

        // 1. Register
        var regResponse = await _client.PostAsJsonAsync("/api/auth/register", regRequest);
        var regBody = await regResponse.Content.ReadAsStringAsync();
        regResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var regContent = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(regBody, JsonOptions);
        regContent.Should().NotBeNull();
        regContent!.Success.Should().BeTrue();
        regContent.Data!.Token.Should().NotBeNullOrWhiteSpace();

        // 2. Login
        var loginRequest = new LoginRequest
        {
            Email = uniqueEmail,
            Password = "SecurePassword123!"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginBody = await loginResponse.Content.ReadAsStringAsync();
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginContent = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(loginBody, JsonOptions);
        loginContent.Should().NotBeNull();
        loginContent!.Success.Should().BeTrue();
        loginContent.Data!.Token.Should().NotBeNullOrWhiteSpace();
        loginContent.Data!.User.Email.Should().Be(uniqueEmail);
    }
}
