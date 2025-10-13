using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;
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
    private readonly CustomAuthenticationStateProvider _authStateProvider;
    private bool _isInitialized = false;

    public User currentUser;

    public event Action OnUserChanged;

    public UserService(
        IDbContextFactory<AppDbContext> contextFactory,
        ProtectedLocalStorage protectedLocalStorage,
        CustomAuthenticationStateProvider authStateProvider)
    {
        _contextFactory = contextFactory;
        _protectedLocalStorage = protectedLocalStorage;
        _authStateProvider = authStateProvider;
    }

    // Call this method when you actually need to initialize (e.g., after render)
    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        try
        {
            // Try to get user from local storage
            var storedUser = await _protectedLocalStorage.GetAsync<User>("currentUser");
            if (storedUser.Success && storedUser.Value != null)
            {
                currentUser = storedUser.Value;
                await _authStateProvider.MarkUserAsAuthenticated(currentUser);
                OnUserChanged?.Invoke();
            }
        }
        catch (InvalidOperationException)
        {
            // Ignore during prerendering
        }
        catch (CryptographicException)
        {
            // Local storage might be encrypted with different key, ignore
            await _protectedLocalStorage.DeleteAsync("currentUser");
        }
        finally
        {
            _isInitialized = true;
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
            try
            {
                await _protectedLocalStorage.SetAsync("currentUser", user);
                await _authStateProvider.MarkUserAsAuthenticated(user);
                OnUserChanged?.Invoke();
                return user;
            }
            catch (InvalidOperationException)
            {
                // Ignore during prerendering, but still set the user
                currentUser = user;
                await _authStateProvider.MarkUserAsAuthenticated(user);
                OnUserChanged?.Invoke();
                return user;
            }
        }

        return null;
    }

    public async Task Logout()
    {
        currentUser = null;
        try
        {
            await _protectedLocalStorage.DeleteAsync("currentUser");
        }
        catch (InvalidOperationException)
        {
            // Ignore during prerendering
        }
        await _authStateProvider.MarkUserAsLoggedOut();
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