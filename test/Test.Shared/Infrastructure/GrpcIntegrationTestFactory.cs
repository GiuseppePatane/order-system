using Grpc.Net.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using Xunit;
using Xunit.Abstractions;

namespace Test.Shared.Infrastructure;

/// <summary>
/// Base class for integration tests using WebApplicationFactory and Testcontainers.
/// Provides PostgreSQL container, Respawner for cleanup, and gRPC channel setup.
/// </summary>
/// <typeparam name="TProgram">The Program class of the application being tested</typeparam>
/// <typeparam name="TDbContext">The DbContext type for database operations</typeparam>
public abstract class GrpcIntegrationTestFactory<TProgram, TDbContext> : WebApplicationFactory<TProgram>, IAsyncLifetime
    where TProgram : class
    where TDbContext : DbContext
{
    private readonly ITestOutputHelper _testOutputHelper;
    public readonly string Environment;
    public GrpcChannel Channel = null!;
    private Respawner _respawner = null!;

    private readonly PostgreSqlContainer _dbContainer;
    private NpgsqlConnection _dbConnection = null!;

    public string DbConnectionString { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the connection string configuration key name.
    /// Override to use a different key name.
    /// </summary>
    protected virtual string ConnectionStringKey => "ProductDatabase";

    /// <summary>
    /// Gets the database name for the test container.
    /// Override to use a different database name.
    /// </summary>
    protected virtual string DatabaseName => "IntegrationTest";

    protected GrpcIntegrationTestFactory(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        Environment = "IntegrationTest";

        _dbContainer = new PostgreSqlBuilder()
            .WithDatabase(DatabaseName)
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithCommand("-c", "log_statement=all")
            .WithEnvironment("POSTGRES_LOG_MIN_MESSAGES", "notice")
            .Build();
    }

    /// <summary>
    /// Creates a gRPC channel for testing
    /// </summary>
    public GrpcChannel CreateGrpcChannel()
    {
        return CreateClient().CreateChannel();
    }

    /// <summary>
    /// Gets the service provider for dependency injection
    /// </summary>
    public IServiceProvider GetServiceProvider()
    {
        return Services;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environment);

        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            // Override configuration with test settings
            var testSettings = new Dictionary<string, string?>
            {
                {$"ConnectionStrings:{ConnectionStringKey}", DbConnectionString}
            };

            configBuilder.AddInMemoryCollection(testSettings);

            // Allow derived classes to add additional configuration
            ConfigureTestConfiguration(configBuilder, testSettings);
        });

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            RemoveDbContext(services);

            // Register DbContext with test database
            services.AddDbContext<TDbContext>(options =>
            {
                options.UseNpgsql(DbConnectionString);
                ConfigureDbContextOptions(options);
            });

            // Allow derived classes to configure additional services
            ConfigureTestServices(services);
        });

        builder.ConfigureLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.Services.AddSingleton<ILoggerProvider>(_ =>
                new TestOutputHelperLoggerProvider(_testOutputHelper));
            loggingBuilder.AddConsole();
        });
    }

    /// <summary>
    /// Override to configure additional test configuration
    /// </summary>
    protected virtual void ConfigureTestConfiguration(IConfigurationBuilder configBuilder, Dictionary<string, string?> testSettings)
    {
        // Override in derived classes
    }

    /// <summary>
    /// Override to configure DbContext options
    /// </summary>
    protected virtual void ConfigureDbContextOptions(DbContextOptionsBuilder options)
    {
        // Override in derived classes
    }

    /// <summary>
    /// Override to configure additional test services
    /// </summary>
    protected virtual void ConfigureTestServices(IServiceCollection services)
    {
        // Override in derived classes
    }

    /// <summary>
    /// Removes existing DbContext registration from services
    /// </summary>
    private void RemoveDbContext(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d =>
            d.ServiceType == typeof(DbContextOptions<TDbContext>));

        if (descriptor != null)
        {
            services.Remove(descriptor);
        }
    }

    /// <summary>
    /// Initializes Respawner for database cleanup
    /// </summary>
    private async Task InitializeRespawner()
    {
        _dbConnection = new NpgsqlConnection(DbConnectionString);
        await _dbConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "public" },
            TablesToIgnore = new []
            {
                new Respawn.Graph.Table("__EFMigrationsHistory")
            }
        });
    }

    /// <summary>
    /// Resets the database to a clean state
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        if (_respawner == null)
        {
            await InitializeRespawner();
        }
        
        await _respawner.ResetAsync(_dbConnection);
    }

    /// <summary>
    /// Seeds the database with test data
    /// </summary>
    public async Task SeedDatabaseAsync(Action<TDbContext> seedAction)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();

        seedAction(context);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Applies migrations to the test database
    /// </summary>
    private async Task ApplyMigrationsAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        await context.Database.MigrateAsync();
    }

    /// <summary>
    /// Initializes containers and database before tests
    /// </summary>
    public virtual async Task InitializeAsync()
    {
        // Start PostgreSQL container
        await _dbContainer.StartAsync();
        DbConnectionString = _dbContainer.GetConnectionString() + ";Include Error Detail=true";

        // Apply migrations
        await ApplyMigrationsAsync();

        // Create gRPC channel
        Channel = CreateGrpcChannel();

        // Initialize Respawner
        await InitializeRespawner();
    }

    /// <summary>
    /// Cleans up containers after tests
    /// </summary>
    public new async Task DisposeAsync()
    {
        if (_dbConnection != null)
        {
            await _dbConnection.CloseAsync();
            await _dbConnection.DisposeAsync();
        }

        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();

        await base.DisposeAsync();
    }
}
