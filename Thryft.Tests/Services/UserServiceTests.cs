using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;
using Moq;
using Thryft.Data;
using Thryft.Models;
using Thryft.Services;
using Xunit;

namespace Thryft.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IDbContextFactory<AppDbContext>> _mockContextFactory;
        private readonly Mock<ProtectedLocalStorage> _mockProtectedLocalStorage;
        private readonly Mock<CustomAuthenticationStateProvider> _mockAuthStateProvider;
        private readonly Mock<AppDbContext> _mockContext;
        private readonly List<User> _users;
        private readonly UserService _svc;

        public UserServiceTests()
        {
            _mockContextFactory = new Mock<IDbContextFactory<AppDbContext>>();
            _mockProtectedLocalStorage = new Mock<ProtectedLocalStorage>();
            _mockAuthStateProvider = new Mock<CustomAuthenticationStateProvider>();
            _mockContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());

            _users = new List<User>
            {
                new User { UserId = 1, Email = "test@example.com", Password = BCrypt.Net.BCrypt.HashPassword("password123"), IsActive = true },
                new User { UserId = 2, Email = "inactive@example.com", Password = BCrypt.Net.BCrypt.HashPassword("password123"), IsActive = false }
            };

            var mockUsersDbSet = _users.AsQueryable().BuildMockDbSet();
            _mockContext.Setup(c => c.Users).Returns(mockUsersDbSet.Object);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            _mockContextFactory.Setup(f => f.CreateDbContext()).Returns(_mockContext.Object);

            _svc = new UserService(
                _mockContextFactory.Object,
                _mockProtectedLocalStorage.Object,
                _mockAuthStateProvider.Object);
        }

        [Fact]
        public async Task ValidateUserCredentialsAsync_ValidCredentials_ReturnsUser_AndPersists()
        {
            var result = await _svc.ValidateUserCredentialsAsync("test@example.com", "password123");

            Assert.NotNull(result);
            Assert.Equal("test@example.com", result.Email);
            _mockProtectedLocalStorage.Verify(x => x.SetAsync("currentUser", It.IsAny<User>()), Times.Once);
            _mockAuthStateProvider.Verify(x => x.MarkUserAsAuthenticated(It.IsAny<User>()), Times.Once);
            Assert.Equal(_svc.currentUser?.Email, "test@example.com");
        }

        [Fact]
        public async Task ValidateUserCredentialsAsync_InvalidEmail_ReturnsNull()
        {
            var result = await _svc.ValidateUserCredentialsAsync("noone@example.com", "password123");
            Assert.Null(result);
        }

        [Fact]
        public async Task ValidateUserCredentialsAsync_InvalidPassword_ReturnsNull()
        {
            var result = await _svc.ValidateUserCredentialsAsync("test@example.com", "wrong");
            Assert.Null(result);
        }

        [Fact]
        public async Task Logout_ClearsCurrentUser_DeletesStorage_AndMarksLoggedOut()
        {
            _svc.currentUser = _users[0];

            await _svc.Logout();

            Assert.Null(_svc.currentUser);
            _mockProtectedLocalStorage.Verify(x => x.DeleteAsync("currentUser"), Times.Once);
            _mockAuthStateProvider.Verify(x => x.MarkUserAsLoggedOut(), Times.Once);
        }

        [Fact]
        public async Task GetUsersAsync_ReturnsAllUsers()
        {
            var result = await _svc.GetUsersAsync();
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Theory]
        [InlineData("test@example.com")]
        [InlineData("TEST@EXAMPLE.COM")]
        public async Task GetUserAsync_FindsByEmail_CaseInsensitive(string email)
        {
            var result = await _svc.GetUserAsync(email);
            Assert.NotNull(result);
            Assert.Equal("test@example.com", result.Email);
        }

        [Fact]
        public async Task UpdateUserAsync_UpdatesAndInvokesOnUserChanged_WhenCurrentMatches()
        {
            var user = _users[0];
            _svc.currentUser = new User { UserId = user.UserId, Email = user.Email };

            bool invoked = false;
            _svc.OnUserChanged += () => invoked = true;

            user.Email = "updated@example.com";
            await _svc.UpdateUserAsync(user);

            Assert.Equal("updated@example.com", _svc.currentUser.Email);
            Assert.True(invoked);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AddUserAsync_HashesPassword_AndAddsToContext()
        {
            var newUser = new User { Email = "new@example.com", Password = "plainpw" };

            await _svc.AddUserAsync(newUser);

            _mockContext.Verify(c => c.Users.Add(It.IsAny<User>()), Times.Once);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.StartsWith("$2", newUser.Password);
        }

        [Fact]
        public async Task GetCurrentUserAsync_ReturnsUser_WhenPrincipalAuthenticated_WithEmailClaim()
        {
            var claims = new[] { new Claim(ClaimTypes.Email, "test@example.com") };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));

            var result = await _svc.GetCurrentUserAsync(principal);

            Assert.NotNull(result);
            Assert.Equal("test@example.com", result.Email);
        }

        [Fact]
        public async Task GetCurrentUserAsync_ReturnsNull_WhenNotAuthenticated()
        {
            var principal = new ClaimsPrincipal(new ClaimsIdentity());
            var result = await _svc.GetCurrentUserAsync(principal);
            Assert.Null(result);
        }

        [Fact]
        public void HashPassword_And_VerifyPassword_Work()
        {
            var hash = _svc.HashPassword("s3cret!");
            Assert.NotNull(hash);
            Assert.StartsWith("$2", hash);
            Assert.True(_svc.VerifyPassword("s3cret!", hash));
            Assert.False(_svc.VerifyPassword("wrong", hash));
        }

        [Fact]
        public async Task IsActiveUserAsync_ReturnsCorrectValues()
        {
            Assert.True(await _svc.IsActiveUserAsync("test@example.com"));
            Assert.False(await _svc.IsActiveUserAsync("inactive@example.com"));
            Assert.False(await _svc.IsActiveUserAsync("nouser@example.com"));
        }

        [Fact]
        public async Task InitializeAsync_WithStoredUser_SetsCurrentUser_AndMarksAuthenticated()
        {
            var storedUser = _users[0];
            _mockProtectedLocalStorage
                .Setup(x => x.GetAsync<User>("currentUser"))
                .Returns(ValueTask.FromResult(ProtectedBrowserStorageResult<User>.Success(storedUser)));

            await _svc.InitializeAsync();

            Assert.NotNull(_svc.currentUser);
            Assert.Equal(storedUser.Email, _svc.currentUser.Email);
            _mockAuthStateProvider.Verify(x => x.MarkUserAsAuthenticated(storedUser), Times.Once);
        }

        [Fact]
        public async Task InitializeAsync_CryptographicException_DeletesStoredUser()
        {
            _mockProtectedLocalStorage
                .Setup(x => x.GetAsync<User>("currentUser"))
                .ThrowsAsync(new CryptographicException("bad key"));

            await _svc.InitializeAsync();

            _mockProtectedLocalStorage.Verify(x => x.DeleteAsync("currentUser"), Times.Once);
        }
    }

    // Helpers to mock EF Core DbSet<T> for asynchronous LINQ operations
    internal static class TestExtensions
    {
        public static Mock<DbSet<T>> BuildMockDbSet<T>(this IQueryable<T> data) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            mockSet.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));

            // Basic Add/Update/Remove behavior against the underlying list
            mockSet.Setup(m => m.Add(It.IsAny<T>())).Callback<T>(entity =>
            {
                var list = data.ToList();
                list.Add(entity);
            });

            mockSet.Setup(m => m.Update(It.IsAny<T>())).Callback<T>(entity =>
            {
                // no-op for tests that rely on SaveChanges verification
            });

            mockSet.Setup(m => m.Remove(It.IsAny<T>())).Callback<T>(entity =>
            {
                var list = data.ToList();
                list.Remove(entity);
            });

            return mockSet;
        }
    }

    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;
        public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
        public T Current => _inner.Current;
        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }
        public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(_inner.MoveNext());
    }
}