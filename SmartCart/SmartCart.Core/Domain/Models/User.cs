namespace SmartCart.Core.Domain.Models;

public class User
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<CartSession> CartSessions { get; set; } = new List<CartSession>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
