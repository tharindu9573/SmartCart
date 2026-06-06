using SmartCart.Core.Domain.Enums;

namespace SmartCart.Core.Domain.Models;

public class ProductRfidItem
{
    public string Uid { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public RfidItemStatus Status { get; set; } = RfidItemStatus.Active;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Product Product { get; set; } = null!;
    public ICollection<CartEvent> CartEvents { get; set; } = new List<CartEvent>();
    public ICollection<CartLineItem> CartLineItems { get; set; } = new List<CartLineItem>();
}
