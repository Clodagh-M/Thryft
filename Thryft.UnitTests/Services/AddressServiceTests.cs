using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using Moq;
using Thryft.Services;
using Thryft.Models;
using Thryft.Data;

namespace Thryft.Tests.Services
{
    [TestClass]
    public class AddressServiceTests
    {
        private AppDbContext _context;
        private AddressService _addressService;

        [TestInitialize]
        public void TestInitialize()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            var mockContextFactory = new Mock<IDbContextFactory<AppDbContext>>();
            mockContextFactory.Setup(f => f.CreateDbContext())
                            .Returns(_context);

            _addressService = new AddressService(mockContextFactory.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _context?.Dispose();
        }

        [TestMethod]
        public async Task AddAddressAsync_ShouldAddAddress()
        {
            // Arrange
            var address = new Address
            {
                UserId = 1,
                Street = "123 Test St",
                City = "Test City",
                State = "TS",
                ZipCode = "12345",
                IsDefault = true
            };

            // Act
            await _addressService.AddAddressAsync(address);

            // Assert
            var savedAddress = await _context.Addresses.FirstOrDefaultAsync();
            Assert.IsNotNull(savedAddress);
            Assert.AreEqual("123 Test St", savedAddress.Street);
            Assert.IsTrue(savedAddress.IsDefault);
        }

        [TestMethod]
        public async Task AddAddressAsync_WhenSettingDefault_ShouldUnsetOtherDefaults()
        {
            // Arrange
            var existingAddress = new Address
            {
                UserId = 1,
                Street = "Existing Address",
                IsDefault = true
            };
            _context.Addresses.Add(existingAddress);
            await _context.SaveChangesAsync();

            var newAddress = new Address
            {
                UserId = 1,
                Street = "New Address",
                IsDefault = true
            };

            // Act
            await _addressService.AddAddressAsync(newAddress);

            // Assert
            var addresses = await _context.Addresses.Where(a => a.UserId == 1).ToListAsync();
            var defaultAddresses = addresses.Where(a => a.IsDefault).ToList();
            Assert.AreEqual(1, defaultAddresses.Count);
            Assert.AreEqual("New Address", defaultAddresses[0].Street);
        }

        [TestMethod]
        public async Task GetUserAddressesAsync_ShouldReturnUserAddressesOrderedByDefault()
        {
            // Arrange
            var userId = 1;
            var addresses = new List<Address>
            {
                new Address { UserId = userId, Street = "Address 1", IsDefault = false },
                new Address { UserId = userId, Street = "Address 2", IsDefault = true },
                new Address { UserId = userId, Street = "Address 3", IsDefault = false }
            };
            _context.Addresses.AddRange(addresses);
            await _context.SaveChangesAsync();

            // Act
            var result = await _addressService.GetUserAddressesAsync(userId);

            // Assert
            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result[0].IsDefault); // Default should be first
            Assert.AreEqual("Address 2", result[0].Street);
        }

        [TestMethod]
        public async Task DeleteAddressAsync_ShouldRemoveAddress()
        {
            // Arrange
            var address = new Address { AddressId = 1, UserId = 1, Street = "Test St" };
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            // Act
            await _addressService.DeleteAddressAsync(1);

            // Assert
            var deletedAddress = await _context.Addresses.FindAsync(1);
            Assert.IsNull(deletedAddress);
        }
    }
}