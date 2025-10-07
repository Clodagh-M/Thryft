namespace Thryft.Models;

public class CartItem
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public Colour? Colour { get; set; }
    public Size? Size { get; set; }
    public int Quantity { get; set; } = 1;
}
