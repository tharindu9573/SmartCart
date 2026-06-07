using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCart.Core.Domain.Enums;
using SmartCart.Core.Interfaces;
using SmartCart.Core.Interfaces.IServices.Application;
using SmartCart.Core.Interfaces.IServices.Domain;

namespace SmartCart.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartSessionController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICartSessionDomainService _sessionService;
    private readonly ICartNotificationService _notificationService;

    public CartSessionController(
        IUnitOfWork unitOfWork,
        ICartSessionDomainService sessionService,
        ICartNotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _sessionService = sessionService;
        _notificationService = notificationService;
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartSession([FromBody] StartSessionRequest request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
        if (user == null) return NotFound(new { message = "User not found." });

        var cart = await _unitOfWork.Carts.GetByIdAsync(request.CartId);
        if (cart == null) return NotFound(new { message = "Cart not found." });

        var session = await _sessionService.CreateSessionAsync(request.CartId, request.UserId);

        await _notificationService.SendSessionUpdateAsync(request.CartId, new
        {
            type = "session_started",
            sessionId = session.SessionId,
            status = session.Status.ToString()
        });

        return Ok(new { sessionId = session.SessionId, status = session.Status.ToString() });
    }

    [HttpGet("{sessionId}")]
    public async Task<IActionResult> GetSession(int sessionId)
    {
        var session = await _unitOfWork.CartSessions
            .Query()
            .Include(s => s.User)
            .Include(s => s.CartLineItems)
                .ThenInclude(li => li.RfidItem)
                .ThenInclude(r => r.Product)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);

        if (session == null) return NotFound();

        return Ok(new
        {
            sessionId = session.SessionId,
            cartId = session.CartId,
            userId = session.UserId,
            userName = session.User.Name,
            status = session.Status.ToString(),
            isActive = session.IsActive,
            items = session.CartLineItems.Select(li => new
            {
                uid = li.Uid,
                productId = li.RfidItem.ProductId,
                name = li.RfidItem.Product.Name,
                price = li.RfidItem.Product.Price,
                imageUrl = li.RfidItem.Product.ImageUrl
            })
        });
    }

    [HttpPost("{sessionId}/confirm")]
    public async Task<IActionResult> ConfirmCart(int sessionId, [FromBody] ConfirmCartRequest request)
    {
        var session = await _unitOfWork.CartSessions.GetByIdAsync(sessionId);
        if (session == null || !session.IsActive) return NotFound(new { message = "Active session not found." });

        // Persist cart line items
        foreach (var uid in request.Uids.Distinct())
        {
            var exists = await _unitOfWork.CartLineItems.AnyAsync(li => li.CartSessionId == sessionId && li.Uid == uid);
            if (!exists)
            {
                await _unitOfWork.CartLineItems.AddAsync(new Core.Domain.Models.CartLineItem
                {
                    Uid = uid,
                    CartId = session.CartId,
                    CartSessionId = sessionId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        await _sessionService.UpdateStatusAsync(sessionId, CartSessionStatus.Checkout);
        await _unitOfWork.SaveChangesAsync();

        await _notificationService.SendSessionUpdateAsync(session.CartId, new
        {
            type = "checkout_initiated",
            sessionId,
            status = CartSessionStatus.Checkout.ToString()
        });

        return Ok(new { message = "Cart confirmed.", status = CartSessionStatus.Checkout.ToString() });
    }

    [HttpPost("{cartId}/reset")]
    public async Task<IActionResult> ResetSession(int cartId)
    {
        await _notificationService.SendSessionUpdateAsync(cartId, new
        {
            type = "session_reset",
            cartId
        });
        return Ok();
    }

    [HttpGet("active/{cartId}")]
    public async Task<IActionResult> GetActiveSession(int cartId)
    {
        var session = await _unitOfWork.CartSessions
            .Query()
            .Include(s => s.CartEvents)
                .ThenInclude(e => e.RfidItem)
                .ThenInclude(r => r.Product)
                    .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(s => s.CartId == cartId && s.IsActive);

        if (session == null) return NotFound(new { message = "No active session." });

        // Replay events per UID — keep only those where last action was Add
        var currentItems = session.CartEvents
            .GroupBy(e => e.Uid)
            .Where(g => g.OrderBy(e => e.Timestamp).Last().Action == CartEventAction.Add)
            .Select(g =>
            {
                var rfid = g.First().RfidItem;
                return new
                {
                    uid = g.Key,
                    productId = rfid.ProductId,
                    name = rfid.Product.Name,
                    price = rfid.Product.Price,
                    imageUrl = rfid.Product.ImageUrl,
                    categoryName = rfid.Product.Category.Name,
                    quantity = g.Count(e => e.Action == CartEventAction.Add) - g.Count(e => e.Action == CartEventAction.Remove)
                };
            })
            .GroupBy(i => i.productId)
            .Select(g => new
            {
                productId = g.Key,
                name = g.First().name,
                price = g.First().price,
                imageUrl = g.First().imageUrl,
                categoryName = g.First().categoryName,
                quantity = g.Sum(i => i.quantity),
                uids = g.Select(i => i.uid).ToList()
            });

        return Ok(new
        {
            sessionId = session.SessionId,
            status = session.Status.ToString(),
            items = currentItems
        });
    }
}

public record StartSessionRequest(int CartId, int UserId);
public record ConfirmCartRequest(List<string> Uids);
