using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using Thryft.Data;
using Thryft.Models;

namespace Thryft.Services;

public class UserService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public User currentUser;

    // Add event to notify when user changes
    public event Action OnUserChanged;

    public UserService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<User>> GetUsersAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Users.ToListAsync();
    }

    public async Task<User> GetUserAsync(string email)
    {
        using var context = _contextFactory.CreateDbContext();
        currentUser = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        return currentUser;
    }

    public async Task<User> ValidateUserCredentialsAsync(string email, string password)
    {
        using var context = _contextFactory.CreateDbContext();
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
            return null;

        // Hash the entered password and compare with stored hash
        //string enteredPasswordHash = HashPassword(password);

        if (user.Password == password)
        {
            currentUser = user;
            OnUserChanged?.Invoke(); // Notify subscribers
            return user;
        }

        return null;
    }

    // Add logout method
    public void Logout()
    {
        currentUser = null;
        OnUserChanged?.Invoke(); // Notify subscribers
    }

    private string HashPassword(string password)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(password);
        byte[] hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public async Task AddUserAsync(User user)
    {
        using var context = _contextFactory.CreateDbContext();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        currentUser = user;
        OnUserChanged?.Invoke(); // Notify subscribers
    }

    public async Task<User?> GetCurrentUserAsync(ClaimsPrincipal principal)
    {
        if (principal?.Identity?.IsAuthenticated != true)
            return null;

        var email = principal.FindFirst(ClaimTypes.Email)?.Value
                    ?? principal.Identity?.Name;

        if (string.IsNullOrWhiteSpace(email))
            return null;

        return await GetUserAsync(email);
    }
}