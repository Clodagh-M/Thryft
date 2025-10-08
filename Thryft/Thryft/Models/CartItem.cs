namespace Thryft.Models;

public class CartItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public Colour? SelectedColor { get; set; }
    public Size? SelectedSize { get; set; }

    public decimal TotalPrice => Price * Quantity;
}