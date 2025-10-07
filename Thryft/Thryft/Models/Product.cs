namespace Thryft.Models;

public class Product
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int Stock {  get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public Colour[] Colours { get; set; } = Array.Empty<Colour>();
    public Size[] Sizes { get; set; } = Array.Empty<Size>();

    //in terminal (right click Thryft solution (second from top))
    //dotnet ef migrations add [message]
    //if error - 'dotnet tool install --global dotnet-ef'
    //dotnet ef database update

    public ICollection<OrderItem> OrderItems { get; set; }
}
