using Microsoft.EntityFrameworkCore;
using SmartCart.Core.Domain.Enums;
using SmartCart.Core.Domain.Models;

namespace SmartCart.Infrastructure.Data.Seed;

public static class DataSeeder
{
    public static async Task SeedAsync(SmartCartDbContext context)
    {
        await context.Database.MigrateAsync();

        if (await context.Companies.AnyAsync()) return;

        var company = new Company
        {
            Name = "SmartCart Ltd",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Companies.Add(company);
        await context.SaveChangesAsync();

        var branch = new Branch
        {
            CompanyId = company.CompanyId,
            Name = "London Central",
            Location = "123 High Street, London, EC1A 1BB",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Branches.Add(branch);
        await context.SaveChangesAsync();

        var cart = new Cart
        {
            BranchId = branch.BranchId,
            ClientSecret = "smartcart-secret-2024-secure-key",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Carts.Add(cart);
        await context.SaveChangesAsync();

        var user = new User
        {
            Name = "Tharindu",
            MobileNumber = "+94774929226",
            Email = "tharindu9573@gmail.com",
            IsVerified = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var groceries = new ProductCategory { Name = "Groceries", Description = "Fresh and packaged groceries", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var beverages = new ProductCategory { Name = "Beverages", Description = "Drinks and refreshments", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var snacks = new ProductCategory { Name = "Snacks", Description = "Crisps, nuts and snack items", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        context.ProductCategories.AddRange(groceries, beverages, snacks);
        await context.SaveChangesAsync();

        var products = new List<Product>
        {
            new() { Name = "Organic Whole Milk 2L", Price = 1.89m, AvailableQuantity = 50, CategoryId = groceries.CategoryId, ImageUrl = "/images/milk.jpg", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Name = "Brown Bread Loaf", Price = 1.25m, AvailableQuantity = 40, CategoryId = groceries.CategoryId, ImageUrl = "/images/bread.jpg", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Name = "Free Range Eggs (12)", Price = 3.49m, AvailableQuantity = 30, CategoryId = groceries.CategoryId, ImageUrl = "/images/eggs.jpg", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Name = "Cheddar Cheese 400g", Price = 2.75m, AvailableQuantity = 25, CategoryId = groceries.CategoryId, ImageUrl = "/images/cheese.jpg", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Name = "Sparkling Water 1.5L", Price = 0.65m, AvailableQuantity = 80, CategoryId = beverages.CategoryId, ImageUrl = "/images/water.jpg", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Name = "Orange Juice 1L", Price = 1.99m, AvailableQuantity = 60, CategoryId = beverages.CategoryId, ImageUrl = "/images/oj.jpg", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Name = "Cola 330ml Can", Price = 0.99m, AvailableQuantity = 100, CategoryId = beverages.CategoryId, ImageUrl = "/images/cola.jpg", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Name = "Salted Crisps 150g", Price = 1.49m, AvailableQuantity = 70, CategoryId = snacks.CategoryId, ImageUrl = "/images/crisps.jpg", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Name = "Mixed Nuts 200g", Price = 2.99m, AvailableQuantity = 45, CategoryId = snacks.CategoryId, ImageUrl = "/images/nuts.jpg", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Name = "Dark Chocolate Bar 100g", Price = 1.79m, AvailableQuantity = 55, CategoryId = snacks.CategoryId, ImageUrl = "/images/chocolate.jpg", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
        };
        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        var rfidItems = new List<ProductRfidItem>();
        var rfidUids = new[]
        {
            ("DE:AD:BE:EF",          6),  // Cola 330ml Can
            ("11:22:33:44",          1),  // Brown Bread Loaf
            ("55:66:77:88",          2),  // Free Range Eggs (12)
            ("AA:BB:CC:DD",          6),  // Cola 330ml Can
            ("04:11:22:33:44:55:66", 4),  // Sparkling Water 1.5L
            ("C0:FF:EE:99",          5),  // Orange Juice 1L
        };

        foreach (var (uid, productIndex) in rfidUids)
        {
            rfidItems.Add(new ProductRfidItem
            {
                Uid = uid,
                ProductId = products[productIndex].ProductId,
                Status = RfidItemStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        context.ProductRfidItems.AddRange(rfidItems);
        await context.SaveChangesAsync();
    }
}
