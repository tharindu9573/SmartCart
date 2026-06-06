namespace SmartCart.Core.Domain.Models;

public class Cart
{
    public int CartId { get; set; }
    public int BranchId { get; set; }
    public string ClientSecret { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Branch Branch { get; set; } = null!;
    public ICollection<CartSession> CartSessions { get; set; } = new List<CartSession>();
}
