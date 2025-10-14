using Thryft.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Thryft.Data;

namespace Thryft.Services
{
    public class OrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<List<Order>> GetUserOrdersAsync(int userId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.Created)
                .ToListAsync();
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            try
            {
                // Update order status to "Cancelled"
                var order = await GetOrderByIdAsync(orderId);
                if (order != null && order.Status == "Processing")
                {
                    order.Status = "Cancelled";

                    // You'll need to implement this update method
                    await UpdateOrderAsync(order);

                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cancelling order: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateOrderAsync(Order order)
        {
            try
            {
                // If you're using Entity Framework
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating order: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders.ToListAsync();
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            try
            {
                // Find the order in the database
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    return false; // Order not found
                }

                // Update the status
                order.Status = newStatus;

                // If you have a LastUpdated field, update it too
                // order.LastUpdated = DateTime.UtcNow;

                // Save changes to database
                var result = await _context.SaveChangesAsync();
                return result > 0; // Return true if at least one row was affected
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error updating order status: {ex.Message}");
                return false;
            }
        }


    }
}