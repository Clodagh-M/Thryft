namespace Thryft.Models;

public class Order
{
    public int OrderId { get; set; }
    public int UserId { get; set; } // Fixed casing from UserID to UserId
    public decimal Total { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending"; 
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public User User { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}