using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace SmartCart.Hubs;

[Authorize]
public class CartHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var cartId = Context.User?.FindFirst("cartId")?.Value;
        if (!string.IsNullOrEmpty(cartId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"cart-{cartId}");
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var cartId = Context.User?.FindFirst("cartId")?.Value;
        if (!string.IsNullOrEmpty(cartId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"cart-{cartId}");
        }
        await base.OnDisconnectedAsync(exception);
    }
}
