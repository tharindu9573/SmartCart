using SmartCart.Core.Domain.Models;

namespace SmartCart.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Company> Companies { get; }
    IRepository<Branch> Branches { get; }
    IRepository<Cart> Carts { get; }
    IRepository<User> Users { get; }
    IRepository<ProductCategory> ProductCategories { get; }
    IRepository<Product> Products { get; }
    IRepository<ProductRfidItem> ProductRfidItems { get; }
    IRepository<CartSession> CartSessions { get; }
    IRepository<CartEvent> CartEvents { get; }
    IRepository<CartLineItem> CartLineItems { get; }
    IRepository<Transaction> Transactions { get; }

    Task<int> SaveChangesAsync();
}
