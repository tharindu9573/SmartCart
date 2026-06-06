using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using SmartCart.Core.Interfaces.IServices.Application;
using SmartCart.Hubs;

namespace SmartCart.Services;

public class CartNotificationService : ICartNotificationService
{
    private readonly IHubContext<CartHub> _hubContext;

    public CartNotificationService(IHubContext<CartHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendCartUpdateAsync(int cartId, object payload)
    {
        await _hubContext.Clients.Group($"cart-{cartId}")
            .SendAsync("CartUpdated", JsonConvert.SerializeObject(payload));
    }

    public async Task SendSessionUpdateAsync(int cartId, object payload)
    {
        await _hubContext.Clients.Group($"cart-{cartId}")
            .SendAsync("SessionUpdated", JsonConvert.SerializeObject(payload));
    }

    public async Task SendPaymentUpdateAsync(int cartId, object payload)
    {
        await _hubContext.Clients.Group($"cart-{cartId}")
            .SendAsync("PaymentUpdated", JsonConvert.SerializeObject(payload));
    }
}
