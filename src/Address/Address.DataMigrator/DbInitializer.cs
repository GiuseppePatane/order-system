using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Address.Core.Domain;
using Address.Infrastructure.EF;
using Shared.DataMigrator;

namespace Address.DataMigrator;

/// <summary>
/// Initializes the database with migrations and seed data
/// </summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(AddressDbContext context, ILogger logger)
    {
        try
        {
            // Apply migrations
            logger.LogInformation("Checking for pending migrations...");
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

            if (pendingMigrations.Any())
            {
                logger.LogInformation("Applying {Count} pending migrations...", pendingMigrations.Count());
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

    private static async Task SeedDataAsync(AddressDbContext context, ILogger logger)
    {
        // Check if data already exists
        if (await context.Addresses.AnyAsync())
        {
            logger.LogInformation("Database already contains data. Skipping seed.");
            return;
        }

        logger.LogInformation("Seeding database with initial data...");

        
        var addresses = CreateSampleAddresses();
        await context.Addresses.AddRangeAsync(addresses);
        await context.SaveChangesAsync();

        logger.LogInformation("Created {Count} addresses", addresses.Count);
        logger.LogInformation("Database seeding completed successfully.");
    }

    private static List<AddressEntity> CreateSampleAddresses()
    {
        var addresses = new List<AddressEntity>();
        var faker = new Faker("it");

        // Create some predefined Italian addresses using shared user IDs
        var italianAddresses = new[]
        {
            (SharedUserIds.MarioRossi, "Via Roma 123", "Roma", "RM", "00100", "Italia", "Casa"),
            (SharedUserIds.GiuliaBianchi, "Corso Vittorio Emanuele 45", "Milano", "MI", "20100", "Italia", "Ufficio"),
            (SharedUserIds.LucaVerdi, "Via Garibaldi 67", "Napoli", "NA", "80100", "Italia", "Casa"),
            (SharedUserIds.AnnaRomano, "Piazza San Marco 1", "Venezia", "VE", "30100", "Italia", "Negozio"),
            (SharedUserIds.MarcoFerrari, "Via Dante 89", "Firenze", "FI", "50100", "Italia", "Casa"),
            (SharedUserIds.SofiaEsposito, "Corso Francia 234", "Torino", "TO", "10100", "Italia", "Ufficio"),
            (SharedUserIds.AlessandroRicci, "Via Mazzini 12", "Bologna", "BO", "40100", "Italia", "Casa"),
            (SharedUserIds.ElenaMoretti, "Lungomare Caracciolo 56", "Napoli", "NA", "80122", "Italia", "Vacanze"),
            (SharedUserIds.FrancescoBarbieri, "Via Toledo 78", "Napoli", "NA", "80134", "Italia", "Casa"),
            (SharedUserIds.ChiaraFontana, "Piazza del Duomo 5", "Milano", "MI", "20121", "Italia", "Ufficio")
        };


        // Create addresses for predefined users
        for (int i = 0; i < 10; i++)
        {
            var (userId, street, city, state, postalCode, country, label) = italianAddresses[i];

            // Create primary address
            var primaryResult = AddressEntity.Create(
                userId,
                street,
                city,
                state,
                postalCode,
                country,
                null,
                label,
                i == 0 // First address is default
            );

            if (primaryResult.IsSuccess)
            {
                addresses.Add(primaryResult.Value);
            }

         
        }
        
        return addresses;
    }
}
