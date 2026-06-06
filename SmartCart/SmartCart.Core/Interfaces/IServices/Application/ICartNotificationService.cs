namespace SmartCart.Core.Interfaces.IServices.Application;

public interface ICartNotificationService
{
    Task SendCartUpdateAsync(int cartId, object payload);
    Task SendSessionUpdateAsync(int cartId, object payload);
    Task SendPaymentUpdateAsync(int cartId, object payload);
}
