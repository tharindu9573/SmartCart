using SmartCart.Core.Domain.Enums;

namespace SmartCart.Core.Domain.Models;

public class CartSession
{
    public int SessionId { get; set; }
    public int CartId { get; set; }
    public int UserId { get; set; }
    public bool IsActive { get; set; }
    public CartSessionStatus Status { get; set; } = CartSessionStatus.Started;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Cart Cart { get; set; } = null!;
    public User User { get; set; } = null!;
    public ICollection<CartEvent> CartEvents { get; set; } = new List<CartEvent>();
    public ICollection<CartLineItem> CartLineItems { get; set; } = new List<CartLineItem>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
