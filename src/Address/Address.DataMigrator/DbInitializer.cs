using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Address.Infrastructure.EF;


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
    
            logger.LogInformation("Checking for pending migrations...");
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

            var migrations = pendingMigrations.ToList();
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
            
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }
}
