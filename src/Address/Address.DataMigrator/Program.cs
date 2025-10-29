using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Address.DataMigrator;
using Address.Infrastructure.EF;

var builder = Host.CreateApplicationBuilder(args);

// Add configuration
builder.Configuration
    .AddEnvironmentVariables();

// Add DbContext
builder.Services.AddDbContext<AddressDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("addressdb"),
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorCodesToAdd: null);
        });
});

var app = builder.Build();

// Run migrations and seed data
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var logger = services.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("Starting database migration and seed...");

    var context = services.GetRequiredService<AddressDbContext>();
    await DbInitializer.InitializeAsync(context, logger);

    logger.LogInformation("Database migration and seed completed successfully.");
}
catch (Exception ex)
{
    logger.LogError(ex, "Error during database initialization");
    throw;
}
