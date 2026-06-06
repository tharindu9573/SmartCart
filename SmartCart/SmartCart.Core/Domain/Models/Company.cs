namespace SmartCart.Core.Domain.Models;

public class Company
{
    public int CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Branch> Branches { get; set; } = new List<Branch>();
}
