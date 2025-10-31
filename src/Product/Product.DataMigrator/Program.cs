using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Product.DataMigrator;
using Product.Infrastructure.EF;

var builder = Host.CreateApplicationBuilder(args);


builder.Configuration
    .AddEnvironmentVariables();


builder.Services.AddDbContext<ProductDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("productdb"),
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorCodesToAdd: null);
        });
});

var app = builder.Build();


using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var logger = services.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("Starting database migration and seed...");

    var context = services.GetRequiredService<ProductDbContext>();
    await DbInitializer.InitializeAsync(context, logger);

    logger.LogInformation("Database migration and seed completed successfully.");
}
catch (Exception ex)
{
    logger.LogError(ex, "Error during database initialization");
    throw;
}