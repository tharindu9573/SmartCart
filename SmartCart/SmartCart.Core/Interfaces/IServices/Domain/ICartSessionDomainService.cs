using SmartCart.Core.Domain.Enums;
using SmartCart.Core.Domain.Models;

namespace SmartCart.Core.Interfaces.IServices.Domain;

public interface ICartSessionDomainService
{
    Task<CartSession> CreateSessionAsync(int cartId, int userId);
    Task<CartSession?> GetActiveSessionAsync(int cartId);
    Task UpdateStatusAsync(int sessionId, CartSessionStatus status);
    Task DeactivateSessionAsync(int sessionId);
}
