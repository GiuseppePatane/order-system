using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using User.Infrastructure.EF;

namespace User.DataMigrator;

/// <summary>
/// Initializes the database with migrations and seed data
/// </summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(UserDbContext context, ILogger logger)
    {
        try
        {
            // Apply migrations
            logger.LogInformation("Checking for pending migrations...");
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

            var migrations = pendingMigrations as string[] ?? pendingMigrations.ToArray();
            if (migrations.Any())
            {
                logger.LogInformation("Applying {Count} pending migrations...", migrations.Count());
                await context.Database.MigrateAsync();
                logger.LogInformation("Migrations applied successfully.");
            }
            else
            {
                logger.LogInformation("Database is up to date. No pending migrations.");
            }

            // Seed data
            await SeedDataAsync(context, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    private static async Task SeedDataAsync(UserDbContext context, ILogger logger)
    {
        // Check if data already exists
        if (await context.Users.AnyAsync())
        {
            logger.LogInformation("Database already contains data. Skipping seed.");
            return;
        }

        logger.LogInformation("Seeding database with initial data...");

        // Create users
        var users = CreateUsers();
        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

        logger.LogInformation("Created {Count} users", users.Count);
        logger.LogInformation("Database seeding completed successfully.");
    }

    private static List<Core.Domain.UserEntity> CreateUsers()
    {
        var users = new List<Core.Domain.UserEntity>();
        var faker = new Faker();

        // Create some predefined users with known data
        var predefinedUsers = new[]
        {
            ("Mario", "Rossi", "mario.rossi@example.com"),
            ("Giulia", "Bianchi", "giulia.bianchi@example.com"),
            ("Luca", "Verdi", "luca.verdi@example.com"),
            ("Anna", "Romano", "anna.romano@example.com"),
            ("Marco", "Ferrari", "marco.ferrari@example.com"),
            ("Sofia", "Esposito", "sofia.esposito@example.com"),
            ("Alessandro", "Ricci", "alessandro.ricci@example.com"),
            ("Elena", "Moretti", "elena.moretti@example.com"),
            ("Francesco", "Barbieri", "francesco.barbieri@example.com"),
            ("Chiara", "Fontana", "chiara.fontana@example.com")
        };

        foreach (var (firstName, lastName, email) in predefinedUsers)
        {
            var result = Core.Domain.UserEntity.Create(firstName, lastName, email);
            if (result.IsSuccess)
            {
                users.Add(result.Value);
            }
        }

        // Create additional random users
        for (int i = 0; i < 40; i++)
        {
            var firstName = faker.Name.FirstName();
            var lastName = faker.Name.LastName();
            var email = faker.Internet.Email(firstName, lastName);

            var result = Core.Domain.UserEntity.Create(firstName, lastName, email);
            if (result.IsSuccess)
            {
                users.Add(result.Value);
            }
        }

        return users;
    }
}
