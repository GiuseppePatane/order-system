using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Order.Core.Repositories;
using Order.Infrastructure.EF;
using Order.Infrastructure.Repositories;

namespace Order.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // Register DbContext
        var connectionString = configuration.GetConnectionString("orderdb");

        services.AddDbContext<OrderDbContext>(options =>
        {
            options.UseNpgsql(connectionString);

            if (environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // Register repositories
        services.AddScoped<IOrderReadOnlyRepository, OrderReadOnlyRepository>();
        services.AddScoped<IOrderWriteRepository, OrderWriteRepository>();

        return services;
    }
}
