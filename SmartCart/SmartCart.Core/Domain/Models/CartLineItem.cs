namespace SmartCart.Core.Domain.Models;

public class CartLineItem
{
    public int Id { get; set; }
    public string Uid { get; set; } = string.Empty;
    public int CartId { get; set; }
    public int CartSessionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ProductRfidItem RfidItem { get; set; } = null!;
    public CartSession CartSession { get; set; } = null!;
}
