using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Thryft.Data;
using Thryft.Models;
using BCrypt.Net;

//@* login branch *@


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
    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            Console.WriteLine("UserService already initialized");
            return;
        }

        Console.WriteLine("Starting UserService initialization...");

        try
        {
            // Try to get user from local storage
            var storedUser = await _protectedLocalStorage.GetAsync<User>("currentUser");
            Console.WriteLine($"Stored user retrieval successful: {storedUser.Success}");

            if (storedUser.Success && storedUser.Value != null)
            {
                Console.WriteLine($"Found user in local storage: {storedUser.Value.Email}");
                currentUser = storedUser.Value;
                await _authStateProvider.MarkUserAsAuthenticated(currentUser);
                OnUserChanged?.Invoke();
                Console.WriteLine("User marked as authenticated");
            }
            else
            {
                Console.WriteLine("No user found in local storage");
            }
        }
        catch (InvalidOperationException ex)
        {
            // Ignore during prerendering
            Console.WriteLine($"InvalidOperationException during initialization: {ex.Message}");
        }
        catch (CryptographicException ex)
        {
            // Local storage might be encrypted with different key, ignore
            Console.WriteLine($"CryptographicException during initialization: {ex.Message}");
            await _protectedLocalStorage.DeleteAsync("currentUser");
        }
        catch (Exception ex)
        {
            // Log other exceptions but don't crash
            Console.WriteLine($"Error initializing user service: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        finally
        {
            _isInitialized = true;
            Console.WriteLine($"UserService initialization complete. _isInitialized = {_isInitialized}");
        }
    }

    public async Task<User> ValidateUserCredentialsAsync(string email, string password)
    {
        using var context = _contextFactory.CreateDbContext();
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
            return null;

        if (VerifyPassword(password, user.Password))
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
        email = email.ToLower();
       var currentUser = await context.Users
            .Include(u => u.Addresses) // Make sure to include addresses
            .FirstOrDefaultAsync(u => u.Email == email );
        return currentUser;
    }

    public async Task UpdateUserAsync(User user)
    {
        using var context = _contextFactory.CreateDbContext();
        context.Users.Update(user);
        await context.SaveChangesAsync();

        // Update current user if it's the same user
        if (currentUser?.UserId == user.UserId)
        {
            currentUser = user;
            OnUserChanged?.Invoke();
        }
    }

    //private string HashPassword(string password)
    //{
    //    using var context = _contextFactory.CreateDbContext();
    //    email = email.ToLower();
    //    currentUser = await context.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
    //    return true;

    //}

    public async Task AddUserAsync(User user)
    {
        using var context = _contextFactory.CreateDbContext();
        user.Password = HashPassword(user.Password);

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

        using var context = _contextFactory.CreateDbContext();
        return await context.Users
            .Include(u => u.Addresses) // Include addresses when getting current user
            .FirstOrDefaultAsync(u => u.Email == email);
    }
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }

    // Run this once to migrate existing users
    public async Task MigrateExistingPasswords()
    {
        using var context = _contextFactory.CreateDbContext();
        var users = await context.Users.ToListAsync();

        foreach (var user in users)
        {
            // If password doesn't look like a BCrypt hash, hash it
            if (!user.Password.StartsWith("$2a$") && !user.Password.StartsWith("$2b$"))
            {
                user.Password = HashPassword(user.Password);
                _contextFactory.CreateDbContext().Users.Update(user);
            }
        }

        await context.SaveChangesAsync();
    }

    public async Task<bool> IsActiveUserAsync(string email)
    {
        using var context = _contextFactory.CreateDbContext();
        email = email.ToLower();
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        return user?.IsActive ?? false;
    }

    public async Task activateAccount(User user)
    {
        user.IsActive = true;
        using var context = _contextFactory.CreateDbContext();
        context.Users.Update(user);
        await context.SaveChangesAsync();
    }

    public async Task deactivateAccount(User user)
    {
        user.IsActive = false;
        using var context = _contextFactory.CreateDbContext();
        context.Users.Update(user);
        await context.SaveChangesAsync();
    }
}