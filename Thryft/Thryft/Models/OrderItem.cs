namespace Thryft.Models;

public class OrderItem
{
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }

    public Colour SelectedColour { get; set; }
    public Size SelectedSize { get; set; }

    public Order Order { get; set; }
    public Product Product { get; set; }
}

