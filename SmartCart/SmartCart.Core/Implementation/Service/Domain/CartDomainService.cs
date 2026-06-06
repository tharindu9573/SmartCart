using SmartCart.Core.Domain.Enums;
using SmartCart.Core.Domain.Models;
using SmartCart.Core.Interfaces;
using SmartCart.Core.Interfaces.IServices.Domain;

namespace SmartCart.Core.Implementation.Service.Domain;

public class CartDomainService : ICartDomainService
{
    private readonly IUnitOfWork _unitOfWork;

    public CartDomainService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<(CartEventAction action, CartEvent cartEvent)> ProcessScanAsync(
        string uid, int cartId, int sessionId, DateTime timestamp)
    {
        bool isInCart = await IsUidInActiveCartAsync(uid, sessionId);

        CartEventAction action = isInCart ? CartEventAction.Remove : CartEventAction.Add;

        var cartEvent = new CartEvent
        {
            Uid = uid,
            CartId = cartId,
            CartSessionId = sessionId,
            Action = action,
            Timestamp = timestamp,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.CartEvents.AddAsync(cartEvent);
        await _unitOfWork.SaveChangesAsync();

        return (action, cartEvent);
    }

    public async Task<bool> IsUidInActiveCartAsync(string uid, int sessionId)
    {
        var events = await _unitOfWork.CartEvents.FindAsync(
            e => e.Uid == uid && e.CartSessionId == sessionId);

        var eventList = events.OrderBy(e => e.Timestamp).ToList();
        if (!eventList.Any()) return false;

        var lastEvent = eventList.Last();
        return lastEvent.Action == CartEventAction.Add;
    }
}
