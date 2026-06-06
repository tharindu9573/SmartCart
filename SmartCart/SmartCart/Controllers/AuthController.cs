using Microsoft.AspNetCore.Mvc;
using SmartCart.Core.Interfaces.IServices.Application;

namespace SmartCart.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("token")]
    public async Task<IActionResult> GetToken([FromBody] TokenRequest request)
    {
        var result = await _authService.AuthenticateAsync(request.CartId, request.ClientSecret);
        if (result == null) return Unauthorized(new { message = "Invalid CartId or ClientSecret." });

        return Ok(new
        {
            accessToken = result.AccessToken,
            refreshToken = result.RefreshToken,
            expiresAt = result.ExpiresAt
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);
        if (result == null) return Unauthorized(new { message = "Invalid or expired refresh token." });

        return Ok(new
        {
            accessToken = result.AccessToken,
            refreshToken = result.RefreshToken,
            expiresAt = result.ExpiresAt
        });
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromBody] RefreshRequest request)
    {
        await _authService.RevokeRefreshTokenAsync(request.RefreshToken);
        return NoContent();
    }
}

public record TokenRequest(int CartId, string ClientSecret);
public record RefreshRequest(string RefreshToken);
