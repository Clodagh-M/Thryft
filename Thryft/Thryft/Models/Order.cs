namespace Thryft.Models;


public class Order
{
    public int OrderId { get; set; }
    public int UserID { get; set; }
    public Decimal Total { get; set; }
    public DateTime Created { get; set; }
    public string Status { get; set; }
}
