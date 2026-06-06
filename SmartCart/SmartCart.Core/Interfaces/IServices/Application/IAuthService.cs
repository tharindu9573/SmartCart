namespace SmartCart.Core.Interfaces.IServices.Application;

public record TokenResult(string AccessToken, string RefreshToken, DateTime ExpiresAt);

public interface IAuthService
{
    Task<TokenResult?> AuthenticateAsync(int cartId, string clientSecret);
    Task<TokenResult?> RefreshTokenAsync(string refreshToken);
    Task RevokeRefreshTokenAsync(string refreshToken);
}
