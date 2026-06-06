using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SmartCart.Core.Domain.Enums;
using SmartCart.Core.Domain.Models;
using SmartCart.Core.Interfaces;
using SmartCart.Core.Interfaces.IServices.Application;

namespace SmartCart.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _config;
    private readonly ILogger<PaymentService> _logger;

    private static readonly Dictionary<string, MockCardDetails> _mockCards = new()
    {
        ["CA:FE:BA:BE"] = new MockCardDetails("**** **** **** 1234", "Tharindu Senevirathna", "12/27", 500.00m),
        ["AA:BB:CC:DD"] = new MockCardDetails("**** **** **** 5678", "Tharindu Senevirathna", "06/27", 1000.00m),
    };

    public PaymentService(IUnitOfWork unitOfWork, IConfiguration config, ILogger<PaymentService> logger)
    {
        _unitOfWork = unitOfWork;
        _config = config;
        _logger = logger;
    }

    public async Task<MockCardDetails?> ResolvePaymentTokenAsync(string paymentUid)
    {
        // Try Azure Key Vault first if configured
        var keyVaultUri = _config["AzureKeyVault:Uri"];
        if (!string.IsNullOrEmpty(keyVaultUri))
        {
            try
            {
                var client = new SecretClient(new Uri(keyVaultUri), new Azure.Identity.DefaultAzureCredential());
                var secretName = $"payment-{paymentUid.Replace("-", "-").ToLower()}";
                var secret = await client.GetSecretAsync(secretName);
                return JsonConvert.DeserializeObject<MockCardDetails>(secret.Value.Value);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Key Vault lookup failed for {Uid}, falling back to mock data", paymentUid);
            }
        }

        // Fallback to in-memory mock
        return _mockCards.TryGetValue(paymentUid.ToUpper(), out var card) ? card : null;
    }

    public async Task<(bool success, string invoiceNumber)> ProcessPaymentAsync(
        int cartId, int sessionId, int userId,
        string paymentMethod, string paymentToken, decimal amount)
    {
        var card = await ResolvePaymentTokenAsync(paymentToken);
        if (card == null || card.Balance < amount)
            return (false, string.Empty);

        var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

        var transaction = new Transaction
        {
            CartId = cartId,
            CartSessionId = sessionId,
            UserId = userId,
            PaymentMethod = paymentMethod,
            PaymentToken = paymentToken,
            Amount = amount,
            Currency = "GBP",
            Status = TransactionStatus.Completed,
            InvoiceNumber = invoiceNumber,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Transactions.AddAsync(transaction);

        // Deduct inventory
        var lineItems = await _unitOfWork.CartLineItems.FindAsync(li => li.CartSessionId == sessionId);
        foreach (var item in lineItems)
        {
            var rfidItem = await _unitOfWork.ProductRfidItems.GetByIdAsync(item.Uid);
            if (rfidItem != null)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(rfidItem.ProductId);
                if (product != null && product.AvailableQuantity > 0)
                {
                    product.AvailableQuantity--;
                    product.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Products.Update(product);
                }
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return (true, invoiceNumber);
    }
}
