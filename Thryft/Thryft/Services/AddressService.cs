using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Thryft.Data;
using Thryft.Models;

namespace Thryft.Services;

public class AddressService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public async Task AddAddressAsync(Address address)
    {
        using var context = _contextFactory.CreateDbContext();
        context.Addresses.Add(address);
        await context.SaveChangesAsync();
    }

}
