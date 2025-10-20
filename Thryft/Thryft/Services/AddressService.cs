using Microsoft.EntityFrameworkCore;
using Thryft.Data;
using Thryft.Models;

namespace Thryft.Services;

public class AddressService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public AddressService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task AddAddressAsync(Address address)
    {
        using var context = _contextFactory.CreateDbContext();

        // If this is set as default, unset other defaults for this user
        if (address.IsDefault)
        {
            var userAddresses = await context.Addresses
                .Where(a => a.UserId == address.UserId && a.IsDefault)
                .ToListAsync();

            foreach (var userAddress in userAddresses)
            {
                userAddress.IsDefault = false;
            }
        }

        context.Addresses.Add(address);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAddressAsync(Address address)
    {
        using var context = _contextFactory.CreateDbContext();

        // If this is set as default, unset other defaults for this user
        if (address.IsDefault)
        {
            var userAddresses = await context.Addresses
                .Where(a => a.UserId == address.UserId && a.IsDefault && a.AddressId != address.AddressId)
                .ToListAsync();

            foreach (var userAddress in userAddresses)
            {
                userAddress.IsDefault = false;
            }
        }

        context.Addresses.Update(address);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAddressAsync(int addressId)
    {
        using var context = _contextFactory.CreateDbContext();
        var address = await context.Addresses.FindAsync(addressId);
        if (address != null)
        {
            context.Addresses.Remove(address);
            await context.SaveChangesAsync();
        }
    }

    public async Task<List<Address>> GetUserAddressesAsync(int userId)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Addresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.AddressId)
            .ToListAsync();
    }
}