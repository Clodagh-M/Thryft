using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Thryft.Services;
using Thryft.Models;
using Thryft.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Thryft.Services.Tests
{
    [TestClass()]
    public class UserServiceTests
    {
        private Mock<IDbContextFactory<AppDbContext>> _mockContextFactory;
        private Mock<ProtectedLocalStorage> _mockProtectedLocalStorage;
        private Mock<CustomAuthenticationStateProvider> _mockAuthStateProvider;
        private UserService _userService;
        private Mock<AppDbContext> _mockContext;
        private List<User> _mockUsers;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockContextFactory = new Mock<IDbContextFactory<AppDbContext>>();
            _mockProtectedLocalStorage = new Mock<ProtectedLocalStorage>();
            _mockAuthStateProvider = new Mock<CustomAuthenticationStateProvider>();
            _mockContext = new Mock<AppDbContext>();

            // Setup mock users
            _mockUsers = new List<User>
            {
                new User
                {
                    UserId = 1,
                    Email = "test@example.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                    IsActive = true
                },
                new User
                {
                    UserId = 2,
                    Email = "inactive@example.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                    IsActive = false
                }
            };

            // Setup mock DbSet for Users
            var mockUsersDbSet = _mockUsers.AsQueryable().BuildMockDbSet();
            _mockContext.Setup(c => c.Users).Returns(mockUsersDbSet.Object);

            _mockContextFactory.Setup(f => f.CreateDbContext())
                .Returns(_mockContext.Object);

            _userService = new UserService(
                _mockContextFactory.Object,
                _mockProtectedLocalStorage.Object,
                _mockAuthStateProvider.Object);
        }

        [TestMethod()]
        public async Task ValidateUserCredentialsAsyncTest_ValidCredentials_ReturnsUser()
        {
            // Arrange
            var email = "test@example.com";
            var password = "password123";

            // Act
            var result = await _userService.ValidateUserCredentialsAsync(email, password);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(email, result.Email);
            _mockProtectedLocalStorage.Verify(x => x.SetAsync("currentUser", It.IsAny<User>()), Times.Once);
            _mockAuthStateProvider.Verify(x => x.MarkUserAsAuthenticated(It.IsAny<User>()), Times.Once);
        }

        [TestMethod()]
        public async Task ValidateUserCredentialsAsyncTest_InvalidEmail_ReturnsNull()
        {
            // Arrange
            var email = "nonexistent@example.com";
            var password = "password123";

            // Act
            var result = await _userService.ValidateUserCredentialsAsync(email, password);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod()]
        public async Task ValidateUserCredentialsAsyncTest_InvalidPassword_ReturnsNull()
        {
            // Arrange
            var email = "test@example.com";
            var password = "wrongpassword";

            // Act
            var result = await _userService.ValidateUserCredentialsAsync(email, password);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod()]
        public async Task ValidateUserCredentialsAsyncTest_InactiveUser_ReturnsNull()
        {
            // Arrange
            var email = "inactive@example.com";
            var password = "password123";

            // Act
            var result = await _userService.ValidateUserCredentialsAsync(email, password);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod()]
        public async Task LogoutTest_ClearsUserAndStorage()
        {
            // Arrange
            _userService.currentUser = _mockUsers[0];

            // Act
            await _userService.Logout();

            // Assert
            Assert.IsNull(_userService.currentUser);
            _mockProtectedLocalStorage.Verify(x => x.DeleteAsync("currentUser"), Times.Once);
            _mockAuthStateProvider.Verify(x => x.MarkUserAsLoggedOut(), Times.Once);
        }

        [TestMethod()]
        public async Task GetUsersAsyncTest_ReturnsAllUsers()
        {
            // Act
            var result = await _userService.GetUsersAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod()]
        public async Task GetUserAsyncTest_ValidEmail_ReturnsUser()
        {
            // Arrange
            var email = "test@example.com";

            // Act
            var result = await _userService.GetUserAsync(email);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(email, result.Email);
        }

        [TestMethod()]
        public async Task GetUserAsyncTest_InvalidEmail_ReturnsNull()
        {
            // Arrange
            var email = "nonexistent@example.com";

            // Act
            var result = await _userService.GetUserAsync(email);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod()]
        public async Task GetUserAsyncTest_EmailCaseInsensitive_ReturnsUser()
        {
            // Arrange
            var email = "TEST@EXAMPLE.COM";

            // Act
            var result = await _userService.GetUserAsync(email);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("test@example.com", result.Email);
        }

        [TestMethod()]
        public async Task UpdateUserAsyncTest_UpdatesUserInDatabase()
        {
            // Arrange
            var userToUpdate = _mockUsers[0];
            userToUpdate.Email = "updated@example.com";

            // Act
            await _userService.UpdateUserAsync(userToUpdate);

            // Assert
            _mockContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }

        [TestMethod()]
        public async Task UpdateUserAsyncTest_CurrentUserSameId_UpdatesCurrentUser()
        {
            // Arrange
            _userService.currentUser = _mockUsers[0];
            var userToUpdate = _mockUsers[0];
            userToUpdate.Email = "updated@example.com";
            bool userChangedEventFired = false;
            _userService.OnUserChanged += () => userChangedEventFired = true;

            // Act
            await _userService.UpdateUserAsync(userToUpdate);

            // Assert
            Assert.AreEqual(userToUpdate.Email, _userService.currentUser.Email);
            Assert.IsTrue(userChangedEventFired);
        }

        [TestMethod()]
        public async Task AddUserAsyncTest_AddsUserWithHashedPassword()
        {
            // Arrange
            var newUser = new User
            {
                Email = "new@example.com",
                Password = "plainpassword"
            };

            // Act
            await _userService.AddUserAsync(newUser);

            // Assert
            _mockContext.Verify(x => x.Users.Add(It.IsAny<User>()), Times.Once);
            _mockContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
            Assert.IsTrue(newUser.Password.StartsWith("$2"));
        }

        [TestMethod()]
        public async Task GetCurrentUserAsyncTest_AuthenticatedUser_ReturnsUser()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, "test@example.com")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            // Act
            var result = await _userService.GetCurrentUserAsync(principal);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("test@example.com", result.Email);
        }

        [TestMethod()]
        public async Task GetCurrentUserAsyncTest_UnauthenticatedUser_ReturnsNull()
        {
            // Arrange
            var principal = new ClaimsPrincipal();

            // Act
            var result = await _userService.GetCurrentUserAsync(principal);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod()]
        public async Task GetCurrentUserAsyncTest_NoEmailClaim_ReturnsNull()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "testuser")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            // Act
            var result = await _userService.GetCurrentUserAsync(principal);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod()]
        public void HashPasswordTest_ReturnsHashedPassword()
        {
            // Arrange
            var password = "testpassword";

            // Act
            var result = _userService.HashPassword(password);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StartsWith("$2"));
            Assert.AreNotEqual(password, result);
        }

        [TestMethod()]
        public void VerifyPasswordTest_CorrectPassword_ReturnsTrue()
        {
            // Arrange
            var password = "testpassword";
            var hashedPassword = _userService.HashPassword(password);

            // Act
            var result = _userService.VerifyPassword(password, hashedPassword);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void VerifyPasswordTest_IncorrectPassword_ReturnsFalse()
        {
            // Arrange
            var correctPassword = "testpassword";
            var wrongPassword = "wrongpassword";
            var hashedPassword = _userService.HashPassword(correctPassword);

            // Act
            var result = _userService.VerifyPassword(wrongPassword, hashedPassword);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod()]
        public async Task IsActiveUserAsyncTest_ActiveUser_ReturnsTrue()
        {
            // Arrange
            var email = "test@example.com";

            // Act
            var result = await _userService.IsActiveUserAsync(email);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public async Task IsActiveUserAsyncTest_InactiveUser_ReturnsFalse()
        {
            // Arrange
            var email = "inactive@example.com";

            // Act
            var result = await _userService.IsActiveUserAsync(email);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod()]
        public async Task IsActiveUserAsyncTest_NonExistentUser_ReturnsFalse()
        {
            // Arrange
            var email = "nonexistent@example.com";

            // Act
            var result = await _userService.IsActiveUserAsync(email);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod()]
        public async Task ActivateAccountTest_ActivatesUser()
        {
            // Arrange
            var user = _mockUsers[1]; // Inactive user
            Assert.IsFalse(user.IsActive);

            // Act
            await _userService.activateAccount(user);

            // Assert
            Assert.IsTrue(user.IsActive);
            _mockContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }

        [TestMethod()]
        public async Task DeactivateAccountTest_DeactivatesUser()
        {
            // Arrange
            var user = _mockUsers[0]; // Active user
            Assert.IsTrue(user.IsActive);

            // Act
            await _userService.deactivateAccount(user);

            // Assert
            Assert.IsFalse(user.IsActive);
            _mockContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }

        [TestMethod()]
        public async Task InitializeAsyncTest_WithStoredUser_SetsCurrentUser()
        {
            // Arrange
            var storedUser = _mockUsers[0];
            _mockProtectedLocalStorage
                .Setup(x => x.GetAsync<User>("currentUser"))
                .ReturnsAsync(ValueTask.FromResult(ProtectedBrowserStorageResult<User>.Success(storedUser)));

            // Act
            await _userService.InitializeAsync();

            // Assert
            Assert.IsNotNull(_userService.currentUser);
            Assert.AreEqual(storedUser.Email, _userService.currentUser.Email);
            _mockAuthStateProvider.Verify(x => x.MarkUserAsAuthenticated(storedUser), Times.Once);
        }

        [TestMethod()]
        public async Task InitializeAsyncTest_NoStoredUser_CurrentUserRemainsNull()
        {
            // Arrange
            _mockProtectedLocalStorage
                .Setup(x => x.GetAsync<User>("currentUser"))
                .ReturnsAsync(ValueTask.FromResult(ProtectedBrowserStorageResult<User>.Success(default(User))));

            // Act
            await _userService.InitializeAsync();

            // Assert
            Assert.IsNull(_userService.currentUser);
        }
    }

    // Helper extension method to mock DbSet
    public static class TestExtensions
    {
        public static Mock<DbSet<T>> BuildMockDbSet<T>(this IQueryable<T> data) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            mockSet.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(default))
                .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));
            return mockSet;
        }
    }

    // Helper class for async operations
    public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current => _inner.Current;

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}