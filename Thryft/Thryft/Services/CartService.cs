using Thryft.Models;
using Microsoft.JSInterop;
using System.Text.Json;
@* cart branch *@

namespace Thryft.Services
{
    public class CartService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly UserService _userService;
        private const string CartStorageKeyPrefix = "thryft_cart_";

        public Cart CurrentCart { get; private set; } = new Cart();
        public event Action? OnCartUpdated;

        public CartService(IJSRuntime jsRuntime, UserService userService)
        {
            _jsRuntime = jsRuntime;
            _userService = userService;

            // Subscribe to user changes
            _userService.OnUserChanged += OnUserChanged;
            LoadCartForCurrentUser();
        }

        private void OnUserChanged()
        {
            // When user changes, load their cart
            LoadCartForCurrentUser();
        }

        private string GetUserCartKey()
        {
            if (_userService.currentUser?.Email != null)
            {
                return $"{CartStorageKeyPrefix}{_userService.currentUser.Email}";
            }
            return $"{CartStorageKeyPrefix}guest";
        }

        private async void LoadCartForCurrentUser()
        {
            await LoadCartFromStorage();
            OnCartUpdated?.Invoke();
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
                var cartKey = GetUserCartKey();
                var cartJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", cartKey);
                if (!string.IsNullOrEmpty(cartJson))
                {
                    CurrentCart = JsonSerializer.Deserialize<Cart>(cartJson) ?? new Cart();
                }
                else
                {
                    CurrentCart = new Cart();
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
                var cartKey = GetUserCartKey();
                var cartJson = JsonSerializer.Serialize(CurrentCart);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", cartKey, cartJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving cart to storage: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _userService.OnUserChanged -= OnUserChanged;
        }
    }
}