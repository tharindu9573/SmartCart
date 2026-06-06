using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCart.Core.Domain.Enums;
using SmartCart.Core.Domain.Models;
using SmartCart.Core.Interfaces;
using SmartCart.Core.Interfaces.IServices.Application;
using SmartCart.Core.Interfaces.IServices.Domain;

namespace SmartCart.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentService _paymentService;
    private readonly INotificationService _notificationService;
    private readonly ICartNotificationService _cartNotification;
    private readonly ICartSessionDomainService _sessionService;

    public PaymentController(
        IUnitOfWork unitOfWork,
        IPaymentService paymentService,
        INotificationService notificationService,
        ICartNotificationService cartNotification,
        ICartSessionDomainService sessionService)
    {
        _unitOfWork = unitOfWork;
        _paymentService = paymentService;
        _notificationService = notificationService;
        _cartNotification = cartNotification;
        _sessionService = sessionService;
    }

    [HttpPost("resolve-token")]
    public async Task<IActionResult> ResolveToken([FromBody] ResolveTokenRequest request)
    {
        var card = await _paymentService.ResolvePaymentTokenAsync(request.PaymentUid);
        if (card == null) return NotFound(new { message = "Payment token not found." });

        return Ok(new
        {
            maskedCardNumber = card.MaskedCardNumber,
            holderName = card.HolderName,
            expiry = card.Expiry
        });
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequest request)
    {
        var session = await _unitOfWork.CartSessions
            .Query()
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.SessionId == request.SessionId && s.IsActive);

        if (session == null)
            return NotFound(new { message = "Active session not found." });

        if (session.Status != CartSessionStatus.Checkout)
            return BadRequest(new { message = "Session is not in Checkout status." });

        await _sessionService.UpdateStatusAsync(request.SessionId, CartSessionStatus.PaymentProcessing);
        await _cartNotification.SendPaymentUpdateAsync(session.CartId, new
        {
            type = "payment_processing",
            sessionId = request.SessionId
        });

        var (success, invoiceNumber) = await _paymentService.ProcessPaymentAsync(
            session.CartId, request.SessionId, session.UserId,
            request.PaymentMethod, request.PaymentToken, request.Amount);

        if (!success)
        {
            await _sessionService.UpdateStatusAsync(request.SessionId, CartSessionStatus.Checkout);
            await _cartNotification.SendPaymentUpdateAsync(session.CartId, new
            {
                type = "payment_failed",
                sessionId = request.SessionId,
                message = "Payment failed. Please retry."
            });
            return BadRequest(new { message = "Payment failed. Insufficient balance or invalid token." });
        }

        await _sessionService.DeactivateSessionAsync(request.SessionId);

        // Build invoice body — group RFID line items by product for quantity display
        var lineItems = await _unitOfWork.CartLineItems
            .Query()
            .Include(li => li.RfidItem)
                .ThenInclude(r => r.Product)
                    .ThenInclude(p => p.Category)
            .Where(li => li.CartSessionId == request.SessionId)
            .ToListAsync();

        var groupedItems = lineItems
            .GroupBy(li => li.RfidItem.ProductId)
            .Select(g => new InvoiceLineItem(
                Name: g.First().RfidItem.Product.Name,
                Category: g.First().RfidItem.Product.Category?.Name ?? "",
                UnitPrice: g.First().RfidItem.Product.Price,
                Quantity: g.Count()
            ))
            .ToList();

        // Send notifications
        var user = session.User;
        await _notificationService.SendInvoiceEmailAsync(
            user.Email, user.Name, invoiceNumber, request.Amount, groupedItems);
        await _notificationService.SendSmsGreetingAsync(
            user.MobileNumber, user.Name, invoiceNumber);

        await _cartNotification.SendPaymentUpdateAsync(session.CartId, new
        {
            type = "payment_success",
            sessionId = request.SessionId,
            invoiceNumber,
            amount = request.Amount
        });

        return Ok(new { success = true, invoiceNumber, amount = request.Amount });
    }
}

public record ResolveTokenRequest(string PaymentUid);
public record ProcessPaymentRequest(int SessionId, string PaymentMethod, string PaymentToken, decimal Amount);
