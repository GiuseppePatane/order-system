using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Order.Infrastructure.EF;

namespace Order.DataMigrator;

/// <summary>
/// Initializes the database with migrations and seed data
/// </summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(OrderDbContext context, ILogger logger)
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

            // Seed data (optional - orders are typically created through the application)
            await SeedDataAsync(context, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    private static async Task SeedDataAsync(OrderDbContext context, ILogger logger)
    {
        // Check if data already exists
        if (await context.Orders.AnyAsync())
        {
            logger.LogInformation("Database already contains data. Skipping seed.");
            return;
        }

        logger.LogInformation("Order database is ready. No seed data required.");
    }
}
