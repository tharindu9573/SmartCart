using SmartCart.Core.Domain.Enums;
using SmartCart.Core.Domain.Models;

namespace SmartCart.Core.Interfaces.IServices.Domain;

public interface ICartDomainService
{
    Task<(CartEventAction action, CartEvent cartEvent)> ProcessScanAsync(
        string uid, int cartId, int sessionId, DateTime timestamp);

    Task<bool> IsUidInActiveCartAsync(string uid, int sessionId);
}
