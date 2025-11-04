using Microsoft.EntityFrameworkCore;
using Moq;
using Thryft.Data;
using Thryft.Models;

namespace Thryft.Tests.TestHelpers;

public static class TestContextFactory
{
    public static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    public static IDbContextFactory<AppDbContext> CreateContextFactory()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var contextFactory = new Mock<IDbContextFactory<AppDbContext>>();
        contextFactory.Setup(f => f.CreateDbContext())
            .Returns(() => new AppDbContext(options));

        return contextFactory.Object;
    }

    public static void SeedTestData(AppDbContext context)
    {
        // Create users
        var users = new List<User>
        {
            new User
            {
                UserId = 1,
                Name = "testuser",
                Email = "test@example.com",
                Password = "test1234",
                CreatedAt = DateTime.UtcNow,
                
            }
        };

        // Create products
        var products = new List<Product>
        {
            new Product
            {
                ProductId = 1,
                ProductName = "Vintage T-Shirt",
                Price = 29.99m,
                Category = "Clothing",
                Stock = 10,
                Colours = new[] { Colour.Red, Colour.Blue },
                Sizes = new[] { Size.S, Size.M, Size.L }
            },
            new Product
            {
                ProductId = 2,
                ProductName = "Designer Jeans",
                Price = 79.99m,
                Stock = 25,
                Category = "Clothing",
                Colours = new[] { Colour.Blue, Colour.Black },
                Sizes = new[] { Size.M, Size.L, Size.XL }
            }
        };

        // Create addresses
        var addresses = new List<Address>
        {
            new Address
            {
                AddressId = 1,
                UserId = 1,
                AddressLine1 = "123 Test St",
                City = "Test City",
                County = "Kerry",
                Eircode = "12345"
            }
        };

        context.Users.AddRange(users);
        context.Products.AddRange(products);
        context.Addresses.AddRange(addresses);
        context.SaveChanges();
    }
}