using SmartCart.Core.Domain.Enums;

namespace SmartCart.Core.Domain.Models;

public class CartEvent
{
    public int EventId { get; set; }
    public string Uid { get; set; } = string.Empty;
    public int CartId { get; set; }
    public int CartSessionId { get; set; }
    public CartEventAction Action { get; set; }
    public DateTime Timestamp { get; set; }
    public DateTime CreatedAt { get; set; }

    public ProductRfidItem RfidItem { get; set; } = null!;
    public CartSession CartSession { get; set; } = null!;
}
