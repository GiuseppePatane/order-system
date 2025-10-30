using Address.Infrastructure.EF;
using Address.IntegrationTest.Fixtures;
using Address.IntegrationTest.Infrastructure;
using Address.Protos;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit.Abstractions;

namespace Address.IntegrationTest;

/// <summary>
/// Integration tests for AddressGrpcService
/// </summary>
[Collection("AddressGrpc")]
public class AddressGrpcServiceTests : IAsyncLifetime
{
    private readonly AddressIntegrationTestFactory _factory;
    private AddressService.AddressServiceClient _grpcClient = null!;
    private readonly Guid _testUserId = Guid.NewGuid();

    public AddressGrpcServiceTests(ITestOutputHelper testOutputHelper)
    {
        _factory = new AddressIntegrationTestFactory(testOutputHelper);
    }

    public async Task InitializeAsync()
    {
        await _factory.InitializeAsync();
        _grpcClient = new AddressService.AddressServiceClient(_factory.Channel);
    }

    public async Task DisposeAsync()
    {
        await _factory.ResetDatabaseAsync();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task GetAddress_WithValidId_ShouldReturnAddress()
    {
        // Arrange
        Guid addressId = Guid.Empty;
        await _factory.SeedDatabaseAsync(context =>
        {
            var addresses = TestDataGenerator.CreateAddresses(_testUserId, 1);
            context.Addresses.AddRange(addresses);
            context.SaveChanges();
            addressId = addresses[0].Id;
        });

        // Act
        var request = new GetAddressRequest { AddressId = addressId.ToString() };
        var response = await _grpcClient.GetAddressAsync(request);

        // Assert
        response.ResultCase.ShouldBe(AddressResponse.ResultOneofCase.Data);
        response.Data.ShouldNotBeNull();
        response.Data.AddressId.ShouldBe(addressId.ToString());
        response.Data.UserId.ShouldBe(_testUserId.ToString());
        response.Data.Street.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task GetAddress_WithInvalidId_ShouldReturnError()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var request = new GetAddressRequest { AddressId = nonExistentId.ToString() };
        var response = await _grpcClient.GetAddressAsync(request);

        // Assert
        response.ResultCase.ShouldBe(AddressResponse.ResultOneofCase.Error);
        response.Error.ShouldNotBeNull();
        response.Error.Code.ShouldContain("NOT_FOUND");
    }

    [Fact]
    public async Task GetAddressesByUser_ShouldReturnAllUserAddresses()
    {
        // Arrange
        await _factory.SeedDatabaseAsync(context =>
        {
            var addresses = TestDataGenerator.CreateAddresses(_testUserId, 5);
            context.Addresses.AddRange(addresses);

            // Add addresses for another user to verify filtering
            var otherUserAddresses = TestDataGenerator.CreateAddresses(Guid.NewGuid(), 3);
            context.Addresses.AddRange(otherUserAddresses);

            context.SaveChanges();
        });

        // Act
        var request = new GetAddressesByUserRequest { UserId = _testUserId.ToString() };
        var response = await _grpcClient.GetAddressesByUserAsync(request);

        // Assert
        response.ResultCase.ShouldBe(GetAddressesResponse.ResultOneofCase.Data);
        response.Data.ShouldNotBeNull();
        response.Data.Items.Count.ShouldBe(5);
        response.Data.Items.All(a => a.UserId == _testUserId.ToString()).ShouldBeTrue();
    }

    [Fact]
    public async Task GetDefaultAddress_ShouldReturnDefaultAddress()
    {
        // Arrange
        Guid defaultAddressId = Guid.Empty;
        await _factory.SeedDatabaseAsync(context =>
        {
            var addresses = TestDataGenerator.CreateAddresses(_testUserId, 3);
            // First address is set as default by TestDataGenerator
            defaultAddressId = addresses[0].Id;
            context.Addresses.AddRange(addresses);
            context.SaveChanges();
        });

        // Act
        var request = new GetDefaultAddressRequest { UserId = _testUserId.ToString() };
        var response = await _grpcClient.GetDefaultAddressAsync(request);

        // Assert
        response.ResultCase.ShouldBe(AddressResponse.ResultOneofCase.Data);
        response.Data.ShouldNotBeNull();
        response.Data.AddressId.ShouldBe(defaultAddressId.ToString());
        response.Data.IsDefault.ShouldBeTrue();
    }

    [Fact]
    public async Task GetPagedAddressesByUser_ShouldReturnPaginatedList()
    {
        // Arrange
        await _factory.SeedDatabaseAsync(context =>
        {
            var addresses = TestDataGenerator.CreateAddresses(_testUserId, 10);
            context.Addresses.AddRange(addresses);
            context.SaveChanges();
        });

        // Act
        var request = new GetPagedAddressesRequest
        {
            UserId = _testUserId.ToString(),
            PageNumber = 1,
            PageSize = 5,
        };
        var response = await _grpcClient.GetPagedAddressesByUserAsync(request);

        // Assert
        response.ResultCase.ShouldBe(GetPagedAddressesResponse.ResultOneofCase.Data);
        response.Data.ShouldNotBeNull();
        response.Data.Items.Count.ShouldBe(5);
        response.Data.PageNumber.ShouldBe(1);
        response.Data.PageSize.ShouldBe(5);
        response.Data.TotalCount.ShouldBe(10);
    }

    [Fact]
    public async Task CreateAddress_WithValidData_ShouldCreateAddress()
    {
        // Act
        var request = new CreateAddressRequest
        {
            UserId = _testUserId.ToString(),
            Street = "123 Main St",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Country = "USA",
            Street2 = "Apt 4B",
            Label = "Home",
            IsDefault = true,
        };
        var response = await _grpcClient.CreateAddressAsync(request);

        // Assert
        response.ResultCase.ShouldBe(AddressResponse.ResultOneofCase.Data);
        response.Data.ShouldNotBeNull();
        response.Data.AddressId.ShouldNotBeNullOrEmpty();

        // Verify in database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AddressDbContext>();
        var savedAddress = await context.Addresses.FirstOrDefaultAsync(a =>
            a.UserId == _testUserId
        );

        savedAddress.ShouldNotBeNull();
        savedAddress.Street.ShouldBe("123 Main St");
        savedAddress.City.ShouldBe("New York");
        savedAddress.IsDefault.ShouldBeTrue();
    }

    [Fact]
    public async Task CreateAddress_AsDefault_ShouldUnsetOtherDefaults()
    {
        // Arrange
        await _factory.SeedDatabaseAsync(context =>
        {
            var addresses = TestDataGenerator.CreateAddresses(_testUserId, 2);
            context.Addresses.AddRange(addresses);
            context.SaveChanges();
        });

        // Act - Create a new default address
        var request = new CreateAddressRequest
        {
            UserId = _testUserId.ToString(),
            Street = "456 Oak Ave",
            City = "Boston",
            State = "MA",
            PostalCode = "02101",
            Country = "USA",
            IsDefault = true,
        };
        var response = await _grpcClient.CreateAddressAsync(request);

        // Assert
        response.ResultCase.ShouldBe(AddressResponse.ResultOneofCase.Data);

        // Verify only one default exists
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AddressDbContext>();
        var defaultAddresses = await context
            .Addresses.Where(a => a.UserId == _testUserId && a.IsDefault)
            .ToListAsync();

        defaultAddresses.Count.ShouldBe(1);
        defaultAddresses[0].Street.ShouldBe("456 Oak Ave");
    }

    [Fact]
    public async Task UpdateAddress_WithValidData_ShouldUpdateAddress()
    {
        // Arrange
        Guid addressId = Guid.Empty;
        await _factory.SeedDatabaseAsync(context =>
        {
            var addresses = TestDataGenerator.CreateAddresses(_testUserId, 1);
            context.Addresses.AddRange(addresses);
            context.SaveChanges();
            addressId = addresses[0].Id;
        });

        // Act
        var request = new UpdateAddressRequest
        {
            AddressId = addressId.ToString(),
            Street = "789 Pine St",
            City = "Los Angeles",
            State = "CA",
            PostalCode = "90001",
            Country = "USA",
            Label = "Office",
        };
        var response = await _grpcClient.UpdateAddressAsync(request);

        // Assert
        response.ResultCase.ShouldBe(AddressResponse.ResultOneofCase.Data);
        response.Data.ShouldNotBeNull();

        // Verify in database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AddressDbContext>();
        var updatedAddress = await context.Addresses.FindAsync(addressId);

        updatedAddress.ShouldNotBeNull();
        updatedAddress.Street.ShouldBe("789 Pine St");
        updatedAddress.City.ShouldBe("Los Angeles");
        updatedAddress.State.ShouldBe("CA");
    }

    [Fact]
    public async Task DeleteAddress_WithValidId_ShouldDeleteAddress()
    {
        // Arrange
        Guid addressId = Guid.Empty;
        await _factory.SeedDatabaseAsync(context =>
        {
            var addresses = TestDataGenerator.CreateAddresses(_testUserId, 1);
            context.Addresses.AddRange(addresses);
            context.SaveChanges();
            addressId = addresses[0].Id;
        });

        // Act
        var request = new DeleteAddressRequest { AddressId = addressId.ToString() };
        var response = await _grpcClient.DeleteAddressAsync(request);

        // Assert
        response.ResultCase.ShouldBe(DeleteAddressResponse.ResultOneofCase.Data);
        response.Data.Success.ShouldBeTrue();

        // Verify in database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AddressDbContext>();
        var deletedAddress = await context.Addresses.FindAsync(addressId);

        deletedAddress.ShouldBeNull();
    }

    [Fact]
    public async Task SetDefaultAddress_ShouldSetAddressAsDefault()
    {
        // Arrange
        Guid actualDefaultAddressId = Guid.Empty;
        Guid newDefaultAddressId = Guid.Empty;
        await _factory.SeedDatabaseAsync(context =>
        {
            var addresses = TestDataGenerator.CreateAddresses(_testUserId, 3);
            // Second address will be set as default
            actualDefaultAddressId = addresses[0].Id; // Initially default
            newDefaultAddressId = addresses[1].Id;
            context.Addresses.AddRange(addresses);
            context.SaveChanges();
        });

        // Act
        var request = new SetDefaultAddressRequest { AddressId = newDefaultAddressId.ToString() };
        var response = await _grpcClient.SetDefaultAddressAsync(request);

        // Assert
        response.ResultCase.ShouldBe(AddressResponse.ResultOneofCase.Data);
        response.Data.ShouldNotBeNull();
        response.Data.IsDefault.ShouldBeTrue();

        // Verify only one default exists
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AddressDbContext>();
        var defaultAddresses = await context.Addresses.FirstOrDefaultAsync(a =>
            a.UserId == _testUserId && a.IsDefault
        );

        defaultAddresses.ShouldNotBeNull();
        defaultAddresses.Id.ShouldBe(newDefaultAddressId);
        defaultAddresses.Id.ShouldNotBe(actualDefaultAddressId);
    }
}
