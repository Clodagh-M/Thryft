using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Thryft.Data;
using Thryft.Models;

namespace Thryft.Services
{
    public class InventoryService
    {
        private readonly ILogger<InventoryService> _logger;
        private readonly AppDbContext _context;

        public InventoryService(ILogger<InventoryService> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<bool> ReduceInventory(int productId, Colour? color, Size? size, int quantity)
        {
            try
            {
                // Find the product in the database
                var product = await _context.Products.FindAsync(productId);

                if (product == null)
                {
                    _logger.LogWarning("Product {ProductId} not found in inventory", productId);
                    return false;
                }

                // Check if there's enough stock
                if (product.Stock < quantity)
                {
                    _logger.LogWarning(
                        "Insufficient stock for product {ProductId}. Requested: {Quantity}, Available: {Stock}",
                        productId, quantity, product.Stock);
                    return false;
                }

                // Reduce the stock
                product.Stock -= quantity;

                // Save changes to database
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Reduced inventory for product {ProductId} by {Quantity}. New stock: {NewStock}",
                    productId, quantity, product.Stock);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reducing inventory for product {ProductId}", productId);
                return false;
            }
        }

        public async Task<int> GetCurrentStock(int productId, Colour? color, Size? size)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                return product?.Stock ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current stock for product {ProductId}", productId);
                return 0;
            }
        }

        // Optional: Method to restore inventory if order fails
        public async Task<bool> RestoreInventory(int productId, Colour? color, Size? size, int quantity)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);

                if (product == null)
                {
                    _logger.LogWarning("Product {ProductId} not found when restoring inventory", productId);
                    return false;
                }

                // Restore the stock
                product.Stock += quantity;

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Restored inventory for product {ProductId} by {Quantity}. New stock: {NewStock}",
                    productId, quantity, product.Stock);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring inventory for product {ProductId}", productId);
                return false;
            }
        }
    }
}