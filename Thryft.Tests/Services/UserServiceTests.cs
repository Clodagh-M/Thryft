//using Microsoft.EntityFrameworkCore;
//using Thryft.Data;
//using Thryft.Models;
//using Thryft.Services;
//using Thryft.Tests.TestHelpers;
//using Xunit;

//namespace Thryft.Tests.Services;

//public class UserServiceTests : IDisposable
//{
//    private AppDbContext _context;
//    private UserService _userService;

//    public UserServiceTests()
//    {
//        _context = TestContextFactory.CreateContext();
//        TestContextFactory.SeedTestData(_context);
//        _userService = new UserService(_context);
//    }

//    [Fact]
//    public async Task GetUserByEmail_ExistingEmail_ReturnsUser()
//    {
//        // Act
//        var result = await _userService.GetUserAsync("test@example.com");

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal("testuser", result.Name);
//        Assert.Equal("test@example.com", result.Email);
//    }

//    [Fact]
//    public async Task CreateUser_ValidUser_CreatesSuccessfully()
//    {
//        // Arrange
//        var newUser = new User
//        {
//            Name = "newuser",
//            Email = "new@example.com",
//            Password = "newUser123",
//            CreatedAt = DateTime.UtcNow
//        };

//        // Act
//        var result = await _userService.AddUserAsync(newUser);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal("newuser", result.Username);
//        Assert.True(result.UserId > 0);
//    }

//    public void Dispose()
//    {
//        _context?.Dispose();
//    }
//}