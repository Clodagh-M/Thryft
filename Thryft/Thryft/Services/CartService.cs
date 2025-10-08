using Thryft.Models;

namespace Thryft.Services
{
    public class CartService
    {
        public Cart CurrentCart { get; private set; } = new Cart();

        public event Action? OnCartUpdated;

        public void AddToCart(Product product, Colour? color, Size? size, int quantity = 1)
        {
            Console.WriteLine($"CartService: Adding {product.ProductName} to cart");

            var cartItem = new CartItem
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Price = product.Price,
                Quantity = quantity,
                SelectedColor = color,
                SelectedSize = size
            };

            CurrentCart.AddItem(cartItem);
            Console.WriteLine($"CartService: Cart now has {CurrentCart.TotalItems} items");

            OnCartUpdated?.Invoke();
            Console.WriteLine($"CartService: OnCartUpdated event fired");
        }

        public void RemoveFromCart(int productId, Colour? color, Size? size)
        {
            CurrentCart.RemoveItem(productId, color, size);
            OnCartUpdated?.Invoke();
        }

        public void UpdateQuantity(int productId, Colour? color, Size? size, int quantity)
        {
            CurrentCart.UpdateQuantity(productId, color, size, quantity);
            OnCartUpdated?.Invoke();
        }

        public void ClearCart()
        {
            CurrentCart.Clear();
            OnCartUpdated?.Invoke();
        }
    }
}