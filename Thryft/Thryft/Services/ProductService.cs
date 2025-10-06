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

    public async Task AddProductAsync(Product product)
    {
        using var context = _contextFactory.CreateDbContext();
        context.Products.Add(product);
        await context.SaveChangesAsync();
    }
}
