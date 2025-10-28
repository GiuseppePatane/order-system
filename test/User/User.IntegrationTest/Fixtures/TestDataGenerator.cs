using Bogus;
using User.Infrastructure.EF;

namespace User.IntegrationTest.Fixtures;

/// <summary>
/// Generates realistic test data using Bogus
/// </summary>
public static class TestDataGenerator
{
    private static readonly Faker _faker = new();

    /// <summary>
    /// Creates test users
    /// </summary>
    public static List<Core.Domain.UserEntity> CreateUsers(int count = 10)
    {
        var users = new List<Core.Domain.UserEntity>();

        for (int i = 0; i < count; i++)
        {
            var firstName = _faker.Name.FirstName();
            var lastName = _faker.Name.LastName();
            var email = _faker.Internet.Email(firstName, lastName);

            var result = Core.Domain.UserEntity.Create(
                firstName,
                lastName,
                email
            );

            if (result.IsSuccess)
            {
                users.Add(result.Value);
            }
        }

        return users;
    }

    /// <summary>
    /// Seeds database with test data
    /// </summary>
    public static async Task SeedDatabase(UserDbContext context, int userCount = 10)
    {
        var users = CreateUsers(userCount);
        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
    }
}

