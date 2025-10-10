using Thryft.Models;
using Microsoft.JSInterop;
using System.Text.Json;

namespace Thryft.Services
{
    public class CartService
    {
        private readonly IJSRuntime _jsRuntime;
        private const string CartStorageKey = "thryft_cart";

        public Cart CurrentCart { get; private set; } = new Cart();
        public event Action? OnCartUpdated;

        public CartService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            LoadCartFromStorage();
        }

        public async void AddToCart(Product product, Colour? color, Size? size, int quantity = 1)
        {
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
            await SaveCartToStorage();
            OnCartUpdated?.Invoke();
        }

        public async void RemoveFromCart(int productId, Colour? color, Size? size)
        {
            CurrentCart.RemoveItem(productId, color, size);
            await SaveCartToStorage();
            OnCartUpdated?.Invoke();
        }

        public async void UpdateQuantity(int productId, Colour? color, Size? size, int quantity)
        {
            CurrentCart.UpdateQuantity(productId, color, size, quantity);
            await SaveCartToStorage();
            OnCartUpdated?.Invoke();
        }

        public async void ClearCart()
        {
            CurrentCart.Clear();
            await SaveCartToStorage();
            OnCartUpdated?.Invoke();
        }

        private async Task LoadCartFromStorage()
        {
            try
            {
                var cartJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", CartStorageKey);
                if (!string.IsNullOrEmpty(cartJson))
                {
                    CurrentCart = JsonSerializer.Deserialize<Cart>(cartJson) ?? new Cart();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading cart from storage: {ex.Message}");
                CurrentCart = new Cart();
            }
        }

        private async Task SaveCartToStorage()
        {
            try
            {
                var cartJson = JsonSerializer.Serialize(CurrentCart);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", CartStorageKey, cartJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving cart to storage: {ex.Message}");
            }
        }
    }
}