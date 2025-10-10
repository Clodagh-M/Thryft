using Microsoft.Extensions.Logging;
using Thryft.Models;

namespace Thryft.Services
{
    public class InventoryService
    {
        private readonly ILogger<InventoryService> _logger;

        // In a real application, this would be a database context or API service
        private readonly Dictionary<string, int> _inventory = new();

        public InventoryService(ILogger<InventoryService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> ReduceInventory(int productId, Colour? color, Size? size, int quantity)
        {
            try
            {
                // Simulate API/database call
                await Task.Delay(100);

                var key = GetInventoryKey(productId, color, size);

                // For demo purposes, we'll assume items are always available
                // In a real app, you'd check current stock levels
                _logger.LogInformation(
                    "Reduced inventory for product {ProductId}, color: {Color}, size: {Size} by {Quantity}",
                    productId, color, size, quantity);

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
            // Simulate getting current stock levels
            await Task.Delay(50);

            var key = GetInventoryKey(productId, color, size);
            return _inventory.ContainsKey(key) ? _inventory[key] : new Random().Next(5, 20);
        }

        private static string GetInventoryKey(int productId, Colour? color, Size? size)
        {
            return $"{productId}-{color}-{size}";
        }
    }
}