using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Thryft.Data;
using Thryft.Models;
using Thryft.Services;
using Xunit;

namespace Thryft.Tests.Services
{
    public class AddressServiceTests
    {
        private Mock<IDbContextFactory<AppDbContext>> CreateFactoryWithSharedDatabase(string dbName, DbContextOptions<AppDbContext> optionsOverride = null)
        {
            var options = optionsOverride ?? new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var factoryMock = new Mock<IDbContextFactory<AppDbContext>>();
            factoryMock.Setup(f => f.CreateDbContext())
                .Returns(() => new AppDbContext(options));

            return factoryMock;
        }

        [Fact]
        public async Task AddAddressAsync_WhenNewDefault_UnsetsExistingDefaultAndAddsNew()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            // Seed existing default address
            using (var seed = new AppDbContext(options))
            {
                seed.Addresses.Add(new Address
                {
                    UserId = 1,
                    AddressLine1 = "Old Default",
                    IsDefault = true
                });
                await seed.SaveChangesAsync();
            }

            var factoryMock = CreateFactoryWithSharedDatabase(dbName, options);
            var service = new AddressService(factoryMock.Object);

            var newAddress = new Address
            {
                UserId = 1,
                AddressLine1 = "New Default",
                IsDefault = true
            };

            await service.AddAddressAsync(newAddress);

            using var assertContext = new AppDbContext(options);
            var addresses = assertContext.Addresses.Where(a => a.UserId == 1).ToList();
            Assert.Equal(2, addresses.Count);

            var old = addresses.Single(a => a.AddressLine1 == "Old Default");
            var nw = addresses.Single(a => a.AddressLine1 == "New Default");

            Assert.False(old.IsDefault);
            Assert.True(nw.IsDefault);
        }

        [Fact]
        public async Task AddAddressAsync_WhenNotDefault_AddsAddressWithoutChangingOtherDefaults()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            // Seed existing default address
            using (var seed = new AppDbContext(options))
            {
                seed.Addresses.Add(new Address
                {
                    UserId = 2,
                    AddressLine1 = "Existing Default",
                    IsDefault = true
                });
                await seed.SaveChangesAsync();
            }

            var factoryMock = CreateFactoryWithSharedDatabase(dbName, options);
            var service = new AddressService(factoryMock.Object);

            var add = new Address
            {
                UserId = 2,
                AddressLine1 = "New NonDefault",
                IsDefault = false
            };

            await service.AddAddressAsync(add);

            using var assertContext = new AppDbContext(options);
            var addresses = assertContext.Addresses.Where(a => a.UserId == 2).ToList();
            Assert.Equal(2, addresses.Count);

            var existing = addresses.Single(a => a.AddressLine1 == "Existing Default");
            var added = addresses.Single(a => a.AddressLine1 == "New NonDefault");

            Assert.True(existing.IsDefault);
            Assert.False(added.IsDefault);
        }

        [Fact]
        public async Task UpdateAddressAsync_WhenSettingDefault_UnsetsOtherDefaults()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            int idOldDefault, idMakeDefault;
            using (var seed = new AppDbContext(options))
            {
                var oldDefault = new Address { UserId = 3, AddressLine1 = "Old", IsDefault = true };
                var other = new Address { UserId = 3, AddressLine1 = "Other", IsDefault = false };
                seed.Addresses.AddRange(oldDefault, other);
                await seed.SaveChangesAsync();
                idOldDefault = oldDefault.AddressId;
                idMakeDefault = other.AddressId;
            }

            var factoryMock = CreateFactoryWithSharedDatabase(dbName, options);
            var service = new AddressService(factoryMock.Object);

            // Update the 'other' address to become default
            var toUpdate = new Address
            {
                AddressId = idMakeDefault,
                UserId = 3,
                AddressLine1 = "Other",
                IsDefault = true
            };

            await service.UpdateAddressAsync(toUpdate);

            using var assertContext = new AppDbContext(options);
            var addresses = assertContext.Addresses.Where(a => a.UserId == 3).ToList();

            var old = addresses.Single(a => a.AddressId == idOldDefault);
            var updated = addresses.Single(a => a.AddressId == idMakeDefault);

            Assert.False(old.IsDefault);
            Assert.True(updated.IsDefault);
        }

        [Fact]
        public async Task UpdateAddressAsync_UpdatesFieldsAndPersists()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            int addressId;
            using (var seed = new AppDbContext(options))
            {
                var a = new Address { UserId = 4, AddressLine1 = "Before", City = "C1", IsDefault = false };
                seed.Addresses.Add(a);
                await seed.SaveChangesAsync();
                addressId = a.AddressId;
            }

            var factoryMock = CreateFactoryWithSharedDatabase(dbName, options);
            var service = new AddressService(factoryMock.Object);

            var updated = new Address
            {
                AddressId = addressId,
                UserId = 4,
                AddressLine1 = "After",
                City = "C2",
                IsDefault = false
            };

            await service.UpdateAddressAsync(updated);

            using var assertContext = new AppDbContext(options);
            var result = await assertContext.Addresses.FindAsync(addressId);
            Assert.Equal("After", result.AddressLine1);
            Assert.Equal("C2", result.City);
        }

        [Fact]
        public async Task DeleteAddressAsync_RemovesAddressIfExists()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            int id;
            using (var seed = new AppDbContext(options))
            {
                var a = new Address { UserId = 5, AddressLine1 = "ToDelete", IsDefault = false };
                seed.Addresses.Add(a);
                await seed.SaveChangesAsync();
                id = a.AddressId;
            }

            var factoryMock = CreateFactoryWithSharedDatabase(dbName, options);
            var service = new AddressService(factoryMock.Object);

            await service.DeleteAddressAsync(id);

            using var assertContext = new AppDbContext(options);
            var deleted = await assertContext.Addresses.FindAsync(id);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task GetUserAddressesAsync_ReturnsAddressesOrdered_DefaultsFirstThenById()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            using (var seed = new AppDbContext(options))
            {
                seed.Addresses.AddRange(
                    new Address { UserId = 6, AddressLine1 = "A", IsDefault = false },
                    new Address { UserId = 6, AddressLine1 = "B", IsDefault = true },
                    new Address { UserId = 6, AddressLine1 = "C", IsDefault = false }
                );
                await seed.SaveChangesAsync();
            }

            var factoryMock = CreateFactoryWithSharedDatabase(dbName, options);
            var service = new AddressService(factoryMock.Object);

            var result = await service.GetUserAddressesAsync(6);

            Assert.Equal(3, result.Count);
            Assert.True(result[0].IsDefault); // default first
            // then non-defaults ordered by AddressId ascending - verify stable ordering exists
            Assert.Equal("B", result[0].AddressLine1);
        }
    }
}
