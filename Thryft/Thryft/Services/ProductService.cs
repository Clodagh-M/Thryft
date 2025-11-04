using Microsoft.EntityFrameworkCore;
using Thryft.Data;
using Thryft.Models;

namespace Thryft.Services;

public class ProductService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public ProductService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Products.ToListAsync();
    }

    public async Task<Product?> GetProductByIdAsync(int productId)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Products
                .FirstOrDefaultAsync(p => p.ProductId == productId);
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error getting product by ID {productId}: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> AddProductAsync(Product product)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            context.Products.Add(product);
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error adding product: {ex.Message}");
            return false;
        }
    }

    public async Task<List<Product>> GetProductsByCategoryAsync(string category)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Products
                .Where(p => p.Category == category)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error getting products by category {category}: {ex.Message}");
            return new List<Product>();
        }
    }

    public async Task<List<Product>> SearchProductsAsync(string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
                return await GetProductsAsync();

            using var context = _contextFactory.CreateDbContext();
            return await context.Products
                .Where(p => p.ProductName.Contains(query) ||
                           p.Category.Contains(query))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error searching products with query {query}: {ex.Message}");
            return new List<Product>();
        }
    }

}