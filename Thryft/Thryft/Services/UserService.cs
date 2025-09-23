using Microsoft.EntityFrameworkCore;
using Thryft.Data;
using Thryft.Models;

namespace Thryft.Services;

public class UserService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public UserService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<User>> GetUsersAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Users.ToListAsync();
    }

    public async Task AddUserAsync(User user)
    {
        using var context = _contextFactory.CreateDbContext();
        context.Users.Add(user);
        await context.SaveChangesAsync();
    }
}
