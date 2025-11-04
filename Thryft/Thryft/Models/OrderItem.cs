namespace Thryft.Models;

//@* orders branch *@


public class OrderItem
{
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; } // Added to store price at time of order

    public Colour? SelectedColour { get; set; }
    public Size? SelectedSize { get; set; }

    public Order Order { get; set; }
    public Product Product { get; set; }
}