using SmartCart.Core.Domain.Models;
using SmartCart.Core.Interfaces;
using SmartCart.Infrastructure.Data;
using SmartCart.Infrastructure.Repository;

namespace SmartCart.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly SmartCartDbContext _context;
    private bool _disposed;

    public IRepository<Company> Companies { get; }
    public IRepository<Branch> Branches { get; }
    public IRepository<Cart> Carts { get; }
    public IRepository<User> Users { get; }
    public IRepository<ProductCategory> ProductCategories { get; }
    public IRepository<Product> Products { get; }
    public IRepository<ProductRfidItem> ProductRfidItems { get; }
    public IRepository<CartSession> CartSessions { get; }
    public IRepository<CartEvent> CartEvents { get; }
    public IRepository<CartLineItem> CartLineItems { get; }
    public IRepository<Transaction> Transactions { get; }

    public UnitOfWork(SmartCartDbContext context)
    {
        _context = context;
        Companies = new Repository<Company>(context);
        Branches = new Repository<Branch>(context);
        Carts = new Repository<Cart>(context);
        Users = new Repository<User>(context);
        ProductCategories = new Repository<ProductCategory>(context);
        Products = new Repository<Product>(context);
        ProductRfidItems = new Repository<ProductRfidItem>(context);
        CartSessions = new Repository<CartSession>(context);
        CartEvents = new Repository<CartEvent>(context);
        CartLineItems = new Repository<CartLineItem>(context);
        Transactions = new Repository<Transaction>(context);
    }

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
            _context.Dispose();
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
