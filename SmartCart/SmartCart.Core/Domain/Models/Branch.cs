namespace SmartCart.Core.Domain.Models;

public class Branch
{
    public int BranchId { get; set; }
    public int CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Company Company { get; set; } = null!;
    public ICollection<Cart> Carts { get; set; } = new List<Cart>();
}
