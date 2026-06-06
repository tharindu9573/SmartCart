using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartCart.Core.Domain.Enums;
using SmartCart.Core.Domain.Models;
using SmartCart.Core.Interfaces;
using SmartCart.Core.Interfaces.IServices.Application;
using SmartCart.Core.Interfaces.IServices.Domain;

namespace SmartCart.Infrastructure.Services;

public class CartScanService : ICartScanService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ICartDomainService _cartDomainService;
    private readonly ICartSessionDomainService _sessionDomainService;
    private readonly ICartNotificationService _notificationService;
    private readonly ILogger<CartScanService> _logger;

    public CartScanService(
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ICartDomainService cartDomainService,
        ICartSessionDomainService sessionDomainService,
        ICartNotificationService notificationService,
        ILogger<CartScanService> logger)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _cartDomainService = cartDomainService;
        _sessionDomainService = sessionDomainService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task ProcessScanEventAsync(ScanEventPayload payload)
    {
        _logger.LogInformation("Processing scan: UID={Uid} CartId={CartId}", payload.Uid, payload.CartId);

        var session = await _sessionDomainService.GetActiveSessionAsync(payload.CartId);
        if (session == null)
        {
            _logger.LogWarning("No active session for CartId={CartId}", payload.CartId);
            return;
        }

        if (session.Status == CartSessionStatus.Started)
            await _sessionDomainService.UpdateStatusAsync(session.SessionId, CartSessionStatus.Scanning);

        if (payload.EventType == EventType.Product)
        {
            // Resolve product — cache first, then DB
            var cacheKey = $"rfid:{payload.Uid}";
            var cachedProduct = await _cache.GetAsync<CachedProductInfo>(cacheKey);

            if (cachedProduct == null)
            {
                var rfidItem = await _unitOfWork.ProductRfidItems
                    .Query()
                    .Include(r => r.Product)
                    .ThenInclude(p => p.Category)
                    .FirstOrDefaultAsync(r => r.Uid == payload.Uid && r.Status == RfidItemStatus.Active);

                if (rfidItem == null)
                {
                    _logger.LogWarning("RFID UID not found or disabled: {Uid}", payload.Uid);
                    return;
                }

                cachedProduct = new CachedProductInfo
                {
                    Uid = rfidItem.Uid,
                    ProductId = rfidItem.ProductId,
                    Name = rfidItem.Product.Name,
                    Price = rfidItem.Product.Price,
                    ImageUrl = rfidItem.Product.ImageUrl,
                    CategoryName = rfidItem.Product.Category.Name
                };

                await _cache.SetAsync(cacheKey, cachedProduct, TimeSpan.FromHours(24));
            }

            var (action, _) = await _cartDomainService.ProcessScanAsync(
                payload.Uid, payload.CartId, session.SessionId, payload.Timestamp);

            await _notificationService.SendCartUpdateAsync(payload.CartId, new
            {
                type = action == CartEventAction.Add ? "item_added" : "item_removed",
                uid = payload.Uid,
                productId = cachedProduct.ProductId,
                name = cachedProduct.Name,
                price = cachedProduct.Price,
                imageUrl = cachedProduct.ImageUrl,
                categoryName = cachedProduct.CategoryName,
                action = action.ToString(),
                timestamp = payload.Timestamp
            });

            _logger.LogInformation("CartUpdated sent to cart-{CartId}: {Action}", payload.CartId, action);
        } else if (payload.EventType == EventType.Payment)
        {
            // Push payment UID to the Angular client via SignalR.
            // Client will call /resolve-token then /payment/process.
            await _notificationService.SendCartUpdateAsync(payload.CartId, new
            {
                type = "payment_card_tapped",
                uid = payload.Uid,
                timestamp = payload.Timestamp
            });

            _logger.LogInformation("Payment card tapped sent to cart-{CartId}: UID={Uid}", payload.CartId, payload.Uid);
        }
    }

    private class CachedProductInfo
    {
        public string Uid { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
}
