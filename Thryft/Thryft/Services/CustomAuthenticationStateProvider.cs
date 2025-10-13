using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;
using System.Security.Cryptography;
using Thryft.Models;

namespace Thryft.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ProtectedLocalStorage _protectedLocalStorage;
        private readonly ILogger<CustomAuthenticationStateProvider> _logger;

        // Cache the authentication state to avoid repeated storage access during prerendering
        private AuthenticationState _cachedState;

        public CustomAuthenticationStateProvider(
            ProtectedLocalStorage protectedLocalStorage,
            ILogger<CustomAuthenticationStateProvider> logger)
        {
            _protectedLocalStorage = protectedLocalStorage;
            _logger = logger;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Return cached state if available (for prerendering scenarios)
            if (_cachedState != null)
            {
                return _cachedState;
            }

            try
            {
                // Try to get user from local storage
                var storedUser = await _protectedLocalStorage.GetAsync<User>("currentUser");

                if (storedUser.Success && storedUser.Value != null)
                {
                    // Create claims based on the stored user
                    var claims = CreateClaims(storedUser.Value);
                    var identity = new ClaimsIdentity(claims, "custom");
                    var principal = new ClaimsPrincipal(identity);

                    _cachedState = new AuthenticationState(principal);
                    return _cachedState;
                }
            }
            catch (InvalidOperationException ex)
            {
                // This occurs during prerendering - return empty state
                _logger.LogDebug("Prerendering detected, returning empty authentication state");
                return CreateEmptyAuthenticationState();
            }
            catch (CryptographicException)
            {
                // Local storage might be encrypted with different key, clear it
                await _protectedLocalStorage.DeleteAsync("currentUser");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting authentication state");
            }

            // Return empty authentication state if no user is found or error occurred
            return CreateEmptyAuthenticationState();
        }

        public async Task MarkUserAsAuthenticated(User user)
        {
            var claims = CreateClaims(user);
            var identity = new ClaimsIdentity(claims, "custom");
            var principal = new ClaimsPrincipal(identity);

            _cachedState = new AuthenticationState(principal);
            NotifyAuthenticationStateChanged(Task.FromResult(_cachedState));
        }

        public async Task MarkUserAsLoggedOut()
        {
            try
            {
                await _protectedLocalStorage.DeleteAsync("currentUser");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user from storage during logout");
            }

            _cachedState = CreateEmptyAuthenticationState();
            NotifyAuthenticationStateChanged(Task.FromResult(_cachedState));
        }

        public void ClearCache()
        {
            _cachedState = null;
        }

        private AuthenticationState CreateEmptyAuthenticationState()
        {
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);
            return new AuthenticationState(principal);
        }

        private List<Claim> CreateClaims(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim("UserId", user.UserId.ToString())
            };

            // Add role claim if user has admin email (you can modify this logic)
            if (user.Email == "thryft-sa-user@gmail.com")
            {
                claims.Add(new Claim(ClaimTypes.Role, "admin"));
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, "user"));
            }

            return claims;
        }
    }
}