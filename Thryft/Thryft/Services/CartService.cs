// Services/CartService.cs
using Thryft.Models;

namespace Thryft.Services;

public class CartService
{
    public List<CartItem> CartItems { get; private set; } = new List<CartItem>();
    public event Action? OnChange;

    public void AddToCart(Product product, Colour? colour, Size? size, int quantity = 1)
    {
        var existingItem = CartItems.FirstOrDefault(item =>
            item.ProductId == product.ProductId.ToString() &&
            item.Colour == colour &&
            item.Size == size);

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            var cartItem = new CartItem
            {
                ProductId = product.ProductId.ToString(),
                ProductName = product.ProductName,
                Price = product.Price,
                Colour = colour,
                Size = size,
                Quantity = quantity
            };
            CartItems.Add(cartItem);
        }

        NotifyStateChanged();
    }

    public void RemoveFromCart(string productId, Colour? colour, Size? size)
    {
        var item = CartItems.FirstOrDefault(item =>
            item.ProductId == productId &&
            item.Colour == colour &&
            item.Size == size);

        if (item != null)
        {
            CartItems.Remove(item);
            NotifyStateChanged();
        }
    }

    public void UpdateQuantity(string productId, Colour? colour, Size? size, int quantity)
    {
        var item = CartItems.FirstOrDefault(item =>
            item.ProductId == productId &&
            item.Colour == colour &&
            item.Size == size);

        if (item != null)
        {
            if (quantity <= 0)
            {
                CartItems.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
            }
            NotifyStateChanged();
        }
    }

    public void ClearCart()
    {
        CartItems.Clear();
        NotifyStateChanged();
    }

    public int GetTotalItems()
    {
        return CartItems.Sum(item => item.Quantity);
    }

    public decimal GetTotalPrice()
    {
        return CartItems.Sum(item => item.Price * item.Quantity);
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}