using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Thryft.Services;
using Thryft.Models;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Thryft.Data;

namespace Thryft.Tests.Services
{
    [TestClass]
    public class CartServiceTests
    {
        private Mock<IJSRuntime> _mockJsRuntime;
        private Mock<UserService> _mockUserService;
        private CartService _cartService;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockJsRuntime = new Mock<IJSRuntime>();

            // Create proper mocks for UserService dependencies
            var mockContextFactory = new Mock<IDbContextFactory<AppDbContext>>();
            var mockStorage = new Mock<ProtectedLocalStorage>();
            var mockAuthProvider = new Mock<CustomAuthenticationStateProvider>();

            _mockUserService = new Mock<UserService>(
                mockContextFactory.Object,
                mockStorage.Object,
                mockAuthProvider.Object);

            _mockUserService.Setup(x => x.currentUser).Returns((User)null);

            _cartService = new CartService(_mockJsRuntime.Object, _mockUserService.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _cartService?.Dispose();
        }

        [TestMethod]
        public void Constructor_ShouldInitializeWithEmptyCart()
        {
            // Assert
            Assert.IsNotNull(_cartService.CurrentCart);
            Assert.AreEqual(0, _cartService.CurrentCart.Items.Count);
        }

        [TestMethod]
        public void AddToCart_ShouldAddItemAndTriggerEvent()
        {
            // Arrange
            var product = new Product { ProductId = 1, ProductName = "Test Product", Price = 29.99m };
            var color = new Colour { ColourId = 1, ColourName = "Red" };
            var size = new Size { SizeId = 1, SizeName = "M" };
            bool eventTriggered = false;
            _cartService.OnCartUpdated += () => eventTriggered = true;

            _mockJsRuntime.Setup(x => x.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
                         .ReturnsAsync((string)null);
            _mockJsRuntime.Setup(x => x.InvokeVoidAsync("localStorage.setItem", It.IsAny<object[]>()))
                         .ReturnsAsync(Mock.Of<IJSVoidResult>());

            // Act
            _cartService.AddToCart(product, color, size, 2);

            // Assert
            Assert.IsTrue(eventTriggered);
            Assert.AreEqual(1, _cartService.CurrentCart.Items.Count);
            var cartItem = _cartService.CurrentCart.Items[0];
            Assert.AreEqual(1, cartItem.ProductId);
            Assert.AreEqual(2, cartItem.Quantity);
        }

        [TestMethod]
        public void RemoveFromCart_ShouldRemoveItemAndTriggerEvent()
        {
            // Arrange
            var product = new Product { ProductId = 1, ProductName = "Test Product", Price = 29.99m };
            var color = new Colour { ColourId = 1, ColourName = "Red" };
            var size = new Size { SizeId = 1, SizeName = "M" };

            _mockJsRuntime.Setup(x => x.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
                         .ReturnsAsync((string)null);
            _mockJsRuntime.Setup(x => x.InvokeVoidAsync("localStorage.setItem", It.IsAny<object[]>()))
                         .ReturnsAsync(Mock.Of<IJSVoidResult>());

            _cartService.AddToCart(product, color, size, 2);
            bool eventTriggered = false;
            _cartService.OnCartUpdated += () => eventTriggered = true;

            // Act
            _cartService.RemoveFromCart(1, color, size);

            // Assert
            Assert.IsTrue(eventTriggered);
            Assert.AreEqual(0, _cartService.CurrentCart.Items.Count);
        }

        [TestMethod]
        public void ClearCart_ShouldRemoveAllItems()
        {
            // Arrange
            var product1 = new Product { ProductId = 1, ProductName = "Product 1", Price = 10.0m };
            var product2 = new Product { ProductId = 2, ProductName = "Product 2", Price = 20.0m };

            _mockJsRuntime.Setup(x => x.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
                         .ReturnsAsync((string)null);
            _mockJsRuntime.Setup(x => x.InvokeVoidAsync("localStorage.setItem", It.IsAny<object[]>()))
                         .ReturnsAsync(Mock.Of<IJSVoidResult>());

            _cartService.AddToCart(product1, null, null, 1);
            _cartService.AddToCart(product2, null, null, 1);

            // Act
            _cartService.ClearCart();

            // Assert
            Assert.AreEqual(0, _cartService.CurrentCart.Items.Count);
        }
    }
}