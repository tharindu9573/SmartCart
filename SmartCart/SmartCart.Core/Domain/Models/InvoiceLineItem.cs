namespace SmartCart.Core.Domain.Models;

public record InvoiceLineItem(string Name, string Category, decimal UnitPrice, int Quantity)
{
    public decimal LineTotal => UnitPrice * Quantity;
}
