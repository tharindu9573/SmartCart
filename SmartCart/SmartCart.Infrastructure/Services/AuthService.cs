using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartCart.Core.Interfaces;
using SmartCart.Core.Interfaces.IServices.Application;

namespace SmartCart.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _config;
    private readonly ICacheService _cache;

    public AuthService(IUnitOfWork unitOfWork, IConfiguration config, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _config = config;
        _cache = cache;
    }

    public async Task<TokenResult?> AuthenticateAsync(int cartId, string clientSecret)
    {
        var cart = await _unitOfWork.Carts.GetByIdAsync(cartId);
        if (cart == null || cart.ClientSecret != clientSecret) return null;

        return await GenerateTokensAsync(cartId);
    }

    public async Task<TokenResult?> RefreshTokenAsync(string refreshToken)
    {
        var cacheKey = $"refresh:{refreshToken}";
        var cartIdStr = await _cache.GetAsync<string>(cacheKey);
        if (cartIdStr == null) return null;

        await _cache.RemoveAsync(cacheKey);

        if (!int.TryParse(cartIdStr, out var cartId)) return null;
        return await GenerateTokensAsync(cartId);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        await _cache.RemoveAsync($"refresh:{refreshToken}");
    }

    private async Task<TokenResult> GenerateTokensAsync(int cartId)
    {
        var jwtKey = _config["Jwt:Key"]!;
        var issuer = _config["Jwt:Issuer"]!;
        var audience = _config["Jwt:Audience"]!;
        var expiryMinutes = int.Parse(_config["Jwt:ExpiryMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new[]
        {
            new Claim("cartId", cartId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(issuer, audience, claims, expires: expires, signingCredentials: creds);
        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        await _cache.SetAsync($"refresh:{refreshToken}", cartId.ToString(), TimeSpan.FromDays(7));

        return new TokenResult(accessToken, refreshToken, expires);
    }
}
