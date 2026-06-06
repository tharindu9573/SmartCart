using SmartCart.Core.Domain.Enums;
using SmartCart.Core.Domain.Models;
using SmartCart.Core.Interfaces;
using SmartCart.Core.Interfaces.IServices.Domain;

namespace SmartCart.Core.Implementation.Service.Domain;

public class CartSessionDomainService : ICartSessionDomainService
{
    private readonly IUnitOfWork _unitOfWork;

    public CartSessionDomainService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CartSession> CreateSessionAsync(int cartId, int userId)
    {
        var existingActive = await GetActiveSessionAsync(cartId);
        if (existingActive != null)
        {
            existingActive.IsActive = false;
            existingActive.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.CartSessions.Update(existingActive);
        }

        var session = new CartSession
        {
            CartId = cartId,
            UserId = userId,
            IsActive = true,
            Status = CartSessionStatus.Started,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.CartSessions.AddAsync(session);
        await _unitOfWork.SaveChangesAsync();
        return session;
    }

    public async Task<CartSession?> GetActiveSessionAsync(int cartId)
    {
        return await _unitOfWork.CartSessions.FirstOrDefaultAsync(
            s => s.CartId == cartId && s.IsActive);
    }

    public async Task UpdateStatusAsync(int sessionId, CartSessionStatus status)
    {
        var session = await _unitOfWork.CartSessions.GetByIdAsync(sessionId);
        if (session == null) return;

        session.Status = status;
        session.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.CartSessions.Update(session);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeactivateSessionAsync(int sessionId)
    {
        var session = await _unitOfWork.CartSessions.GetByIdAsync(sessionId);
        if (session == null) return;

        session.IsActive = false;
        session.Status = CartSessionStatus.Completed;
        session.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.CartSessions.Update(session);
        await _unitOfWork.SaveChangesAsync();
    }
}
