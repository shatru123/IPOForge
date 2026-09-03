using System.Security.Claims;
using IPOForge.Application.Interfaces;
using IPOForge.Contracts.Auth;
using IPOForge.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IPOForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _authService.RegisterAsync(request, cancellationToken);
        return Ok(ApiResponse<AuthResponse>.Ok(response));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(request, cancellationToken);
        return Ok(ApiResponse<AuthResponse>.Ok(response));
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetMe(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Ok(ApiResponse<UserProfileDto>.Ok(new UserProfileDto
            {
                Id = "guest",
                Email = "guest@ipoforge.com",
                FullName = "Guest Investor",
                Role = "Guest",
                CreatedAt = DateTime.UtcNow
            }));
        }

        var profile = await _authService.GetUserProfileAsync(userId, cancellationToken);
        if (profile == null)
        {
            return NotFound(ApiResponse<UserProfileDto>.Fail("USER_NOT_FOUND", "User profile could not be found."));
        }
        return Ok(ApiResponse<UserProfileDto>.Ok(profile));
    }
}
