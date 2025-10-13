using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Interfaces;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Thryft.Data;
using Thryft.Models;


namespace Thryft.Services;

public class UserService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ProtectedLocalStorage _protectedLocalStorage;

    public User currentUser;

    public event Action OnUserChanged;

    public UserService(IDbContextFactory<AppDbContext> contextFactory, ProtectedLocalStorage protectedLocalStorage)
    {
        _contextFactory = contextFactory;
        _protectedLocalStorage = protectedLocalStorage;
        _ = InitializeUserAsync(); // Initialize on service creation
    }

    private async Task InitializeUserAsync()
    {
        try
        {
            // Try to get user from local storage on service initialization
            var storedUser = await _protectedLocalStorage.GetAsync<User>("currentUser");
            if (storedUser.Success && storedUser.Value != null)
            {
                currentUser = storedUser.Value;
                OnUserChanged?.Invoke();
            }
        }
        catch (CryptographicException)
        {
            // Local storage might be encrypted with different key, ignore
            await _protectedLocalStorage.DeleteAsync("currentUser");
        }
    }

    public async Task<User> ValidateUserCredentialsAsync(string email, string password)
    {
        using var context = _contextFactory.CreateDbContext();
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
            return null;

        if (user.Password == password)
        {
            currentUser = user;
            await _protectedLocalStorage.SetAsync("currentUser", user);
            OnUserChanged?.Invoke();
            return user;
        }

        return null;
    }

    public void Logout()
    {
        currentUser = null;
        _protectedLocalStorage.DeleteAsync("currentUser");
        OnUserChanged?.Invoke();
        
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
    }

    // to get the current user logged in
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