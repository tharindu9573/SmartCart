using Microsoft.EntityFrameworkCore;
using SmartCart.Core.Domain.Enums;
using SmartCart.Core.Domain.Models;

namespace SmartCart.Infrastructure.Data;

public class SmartCartDbContext : DbContext
{
    public SmartCartDbContext(DbContextOptions<SmartCartDbContext> options) : base(options) { }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<User> Users => Set<User>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductRfidItem> ProductRfidItems => Set<ProductRfidItem>();
    public DbSet<CartSession> CartSessions => Set<CartSession>();
    public DbSet<CartEvent> CartEvents => Set<CartEvent>();
    public DbSet<CartLineItem> CartLineItems => Set<CartLineItem>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Company>(e =>
        {
            e.HasKey(x => x.CompanyId);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<Branch>(e =>
        {
            e.HasKey(x => x.BranchId);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Location).HasMaxLength(500);
            e.HasOne(x => x.Company)
             .WithMany(x => x.Branches)
             .HasForeignKey(x => x.CompanyId);
        });

        modelBuilder.Entity<Cart>(e =>
        {
            e.HasKey(x => x.CartId);
            e.Property(x => x.ClientSecret).IsRequired().HasMaxLength(256);
            e.HasOne(x => x.Branch)
             .WithMany(x => x.Carts)
             .HasForeignKey(x => x.BranchId);
        });

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.UserId);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.MobileNumber).IsRequired().HasMaxLength(20);
            e.HasIndex(x => x.MobileNumber).IsUnique();
            e.Property(x => x.Email).HasMaxLength(300);
        });

        modelBuilder.Entity<ProductCategory>(e =>
        {
            e.HasKey(x => x.CategoryId);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.HasKey(x => x.ProductId);
            e.Property(x => x.Name).IsRequired().HasMaxLength(300);
            e.Property(x => x.Price).HasColumnType("decimal(18,2)");
            e.Property(x => x.ImageUrl).HasMaxLength(1000);
            e.HasOne(x => x.Category)
             .WithMany(x => x.Products)
             .HasForeignKey(x => x.CategoryId);
        });

        modelBuilder.Entity<ProductRfidItem>(e =>
        {
            e.HasKey(x => x.Uid);
            e.Property(x => x.Uid).HasMaxLength(100);
            e.Property(x => x.Status).HasConversion<string>();
            e.HasOne(x => x.Product)
             .WithMany(x => x.RfidItems)
             .HasForeignKey(x => x.ProductId);
        });

        modelBuilder.Entity<CartSession>(e =>
        {
            e.HasKey(x => x.SessionId);
            e.Property(x => x.Status).HasConversion<string>();
            e.HasOne(x => x.Cart)
             .WithMany(x => x.CartSessions)
             .HasForeignKey(x => x.CartId);
            e.HasOne(x => x.User)
             .WithMany(x => x.CartSessions)
             .HasForeignKey(x => x.UserId);
        });

        modelBuilder.Entity<CartEvent>(e =>
        {
            e.HasKey(x => x.EventId);
            e.Property(x => x.Uid).HasMaxLength(100);
            e.Property(x => x.Action).HasConversion<string>();
            e.HasOne(x => x.RfidItem)
             .WithMany(x => x.CartEvents)
             .HasForeignKey(x => x.Uid);
            e.HasOne(x => x.CartSession)
             .WithMany(x => x.CartEvents)
             .HasForeignKey(x => x.CartSessionId);
        });

        modelBuilder.Entity<CartLineItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Uid).HasMaxLength(100);
            e.HasOne(x => x.RfidItem)
             .WithMany(x => x.CartLineItems)
             .HasForeignKey(x => x.Uid);
            e.HasOne(x => x.CartSession)
             .WithMany(x => x.CartLineItems)
             .HasForeignKey(x => x.CartSessionId);
        });

        modelBuilder.Entity<Transaction>(e =>
        {
            e.HasKey(x => x.TransactionId);
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.Currency).HasMaxLength(10);
            e.Property(x => x.Status).HasConversion<string>();
            e.Property(x => x.PaymentMethod).HasMaxLength(100);
            e.Property(x => x.PaymentToken).HasMaxLength(500);
            e.Property(x => x.InvoiceNumber).HasMaxLength(100);
            e.HasOne(x => x.Cart)
             .WithMany()
             .HasForeignKey(x => x.CartId)
             .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.CartSession)
             .WithMany(x => x.Transactions)
             .HasForeignKey(x => x.CartSessionId)
             .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.User)
             .WithMany(x => x.Transactions)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.NoAction);
        });
    }
}
