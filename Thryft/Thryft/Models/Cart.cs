namespace Thryft.Models;

public class Cart
{
    public List<CartItem> Items { get; set; } = new List<CartItem>();
    public int TotalItems => Items.Sum(item => item.Quantity);
    public decimal TotalPrice => Items.Sum(item => item.TotalPrice);

    public void AddItem(CartItem newItem)
    {
        var existingItem = Items.FirstOrDefault(item =>
            item.ProductId == newItem.ProductId &&
            item.SelectedColor == newItem.SelectedColor &&
            item.SelectedSize == newItem.SelectedSize);

        if (existingItem != null)
        {
            existingItem.Quantity += newItem.Quantity;
        }
        else
        {
            Items.Add(newItem);
        }
    }

    public void RemoveItem(int productId, Colour? color, Size? size)
    {
        var item = Items.FirstOrDefault(item =>
            item.ProductId == productId &&
            item.SelectedColor == color &&
            item.SelectedSize == size);

        if (item != null)
        {
            Items.Remove(item);
        }
    }

    public void UpdateQuantity(int productId, Colour? color, Size? size, int quantity)
    {
        var item = Items.FirstOrDefault(item =>
            item.ProductId == productId &&
            item.SelectedColor == color &&
            item.SelectedSize == size);

        if (item != null)
        {
            if (quantity <= 0)
            {
                Items.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
            }
        }
    }

    public void Clear()
    {
        Items.Clear();
    }
}