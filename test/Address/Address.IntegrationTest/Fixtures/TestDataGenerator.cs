using Bogus;
using Address.Core.Domain;
using Address.Infrastructure.EF;

namespace Address.IntegrationTest.Fixtures;

/// <summary>
/// Generates realistic test data using Bogus
/// </summary>
public static class TestDataGenerator
{
    private static readonly Faker _faker = new();

    /// <summary>
    /// Creates test addresses for a specific user
    /// </summary>
    public static List<AddressEntity> CreateAddresses(Guid userId, int count = 5)
    {
        var addresses = new List<AddressEntity>();
        var labels = new[] { "Home", "Work", "Office", "Vacation Home", "Warehouse" };

        for (int i = 0; i < count; i++)
        {
            var result = AddressEntity.Create(
                userId,
                _faker.Address.StreetAddress(),
                _faker.Address.City(),
                _faker.Address.State(),
                _faker.Address.ZipCode(),
                _faker.Address.Country(),
                i % 3 == 0 ? _faker.Address.SecondaryAddress() : null,
                i < labels.Length ? labels[i] : null,
                i == 0 // First address is default
            );

            if (result.IsSuccess)
            {
                addresses.Add(result.Value);
            }
        }

        return addresses;
    }

    /// <summary>
    /// Creates a single address with specific properties
    /// </summary>
    public static AddressEntity CreateAddress(
        Guid userId,
        string? street = null,
        string? city = null,
        string? state = null,
        string? postalCode = null,
        string? country = null,
        string? street2 = null,
        string? label = null,
        bool isDefault = false)
    {
        var result = AddressEntity.Create(
            userId,
            street ?? _faker.Address.StreetAddress(),
            city ?? _faker.Address.City(),
            state ?? _faker.Address.State(),
            postalCode ?? _faker.Address.ZipCode(),
            country ?? _faker.Address.Country(),
            street2,
            label,
            isDefault
        );

        return result.Value;
    }

    /// <summary>
    /// Seeds database with test data for multiple users
    /// </summary>
    public static async Task SeedDatabase(AddressDbContext context, List<Guid> userIds, int addressesPerUser = 3)
    {
        foreach (var userId in userIds)
        {
            var addresses = CreateAddresses(userId, addressesPerUser);
            await context.Addresses.AddRangeAsync(addresses);
        }

        await context.SaveChangesAsync();
    }
}
