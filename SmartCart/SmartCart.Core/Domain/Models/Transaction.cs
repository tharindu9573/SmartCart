using SmartCart.Core.Domain.Enums;

namespace SmartCart.Core.Domain.Models;

public class Transaction
{
    public int TransactionId { get; set; }
    public int CartId { get; set; }
    public int CartSessionId { get; set; }
    public int UserId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentToken { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "GBP";
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Cart Cart { get; set; } = null!;
    public CartSession CartSession { get; set; } = null!;
    public User User { get; set; } = null!;
}
