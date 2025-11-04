using Microsoft.EntityFrameworkCore;
using Moq;
using Thryft.Data;
using Thryft.Models;
using Thryft.Services;
using Thryft.Tests.TestHelpers;
using Xunit;

namespace Thryft.Tests.Services;

public class ProductServiceTests : IDisposable
{
    private readonly Mock<IDbContextFactory<AppDbContext>> _contextFactoryMock;
    private readonly List<AppDbContext> _contexts = new();

    public ProductServiceTests()
    {
        _contextFactoryMock = new Mock<IDbContextFactory<AppDbContext>>();
        _contextFactoryMock.Setup(f => f.CreateDbContext())
            .Returns(() =>
            {
                var context = CreateFreshContext();
                _contexts.Add(context);
                return context;
            });
    }

    private AppDbContext CreateFreshContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        SeedTestData(context);
        return context;
    }

    private void SeedTestData(AppDbContext context)
    {
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
                Stock = 26,
                Category = "Clothing",
                Colours = new[] { Colour.Blue, Colour.Black },
                Sizes = new[] { Size.M, Size.L, Size.XL }
            }
        };

        context.Products.AddRange(products);
        context.SaveChanges();
    }

    [Fact]
    public async Task GetAllProducts_ReturnsAllProducts()
    {
        // Arrange
        var productService = new ProductService(_contextFactoryMock.Object);

        // Act
        var result = await productService.GetProductsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetProductById_ExistingId_ReturnsProduct()
    {
        // Arrange
        var productService = new ProductService(_contextFactoryMock.Object);

        // Act
        var result = await productService.GetProductByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Vintage T-Shirt", result.ProductName);
        Assert.Equal(29.99m, result.Price);
        Assert.Contains(Colour.Red, result.Colours);
        Assert.Contains(Size.M, result.Sizes);
    }

    [Fact]
    public async Task GetProductById_NonExistingId_ReturnsNull()
    {
        // Arrange
        var productService = new ProductService(_contextFactoryMock.Object);

        // Act
        var result = await productService.GetProductByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetProductsByCategory_ValidCategory_ReturnsFilteredProducts()
    {
        // Arrange
        var productService = new ProductService(_contextFactoryMock.Object);

        // Act
        var result = await productService.GetProductsByCategoryAsync("Clothing");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, product => Assert.Equal("Clothing", product.Category));
    }

    [Fact]
    public async Task SearchProducts_ValidQuery_ReturnsMatchingProducts()
    {
        // Arrange
        var productService = new ProductService(_contextFactoryMock.Object);

        // Act
        var result = await productService.SearchProductsAsync("Vintage");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Contains("Vintage", result.First().ProductName);
    }

    [Fact]
    public async Task AddProduct_ValidProduct_AddsSuccessfully()
    {
        // Arrange
        var productService = new ProductService(_contextFactoryMock.Object);
        var newProduct = new Product
        {
            ProductName = "New Product",
            Price = 49.99m,
            Stock = 13,
            Category = "Clothing",
            Colours = Array.Empty<Colour>(),
            Sizes = Array.Empty<Size>()
        };

        // Act
        var result = await productService.AddProductAsync(newProduct);

        // Assert - Use a fresh context to verify the data was persisted
        using var verificationContext = CreateFreshContext();
        var allProducts = await verificationContext.Products.ToListAsync();

        Assert.True(result);
        Assert.Equal(3, allProducts.Count);

        var addedProduct = allProducts.FirstOrDefault(p => p.ProductName == "New Product");
        Assert.NotNull(addedProduct);
    }

    public void Dispose()
    {
        foreach (var context in _contexts)
        {
            context?.Dispose();
        }
    }
}