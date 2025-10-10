using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Thryft.Models;

public class Product
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int Stock {  get; set; }

    [Column(TypeName = "decimal(18,2)")]
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

//INSERT into Products(ProductName, Stock, Price, Category, Colours, Sizes)
//VALUES('Floral Dress', 20, 24.99, 'Dress', 'Red, Blue, Pink, Purple, Black, White', 'XS, S, M, L, XL, XXL');

//    INSERT into Products(ProductName, Stock, Price, Category, Colours, Sizes)
//VALUES('Striped Socks', 55, 9.99, 'Socks', 'White, Black, Brown, Navy', 'S, M, L');

//    INSERT into Products(ProductName, Stock, Price, Category, Colours, Sizes)
//VALUES('Baseball Cap', 20, 19.99, 'Hat', 'Black, Green, Yellow, Blue, White', 'OneSize');

