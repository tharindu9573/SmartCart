namespace SmartCart.Core.Domain.Models;

public class Product
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int AvailableQuantity { get; set; }
    public int CategoryId { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ProductCategory Category { get; set; } = null!;
    public ICollection<ProductRfidItem> RfidItems { get; set; } = new List<ProductRfidItem>();
}
