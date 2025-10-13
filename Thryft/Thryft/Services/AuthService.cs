using Microsoft.AspNetCore.Components.Authorization;
using Thryft.Models;

namespace Thryft.Services
{
    public class AuthService
    {
        private readonly UserService _userService;
        private readonly CustomAuthenticationStateProvider _authenticationStateProvider;

        public AuthService(UserService userService, CustomAuthenticationStateProvider authenticationStateProvider)
        {
            _userService = userService;
            _authenticationStateProvider = authenticationStateProvider;
        }

        public async Task<User> LoginAsync(string email, string password)
        {
            var user = await _userService.ValidateUserCredentialsAsync(email, password);
            return user;
        }

        public async Task LogoutAsync()
        {
            await _userService.Logout();
        }

        public async Task InitializeAsync()
        {
            await _userService.InitializeAsync();
        }
    }
}