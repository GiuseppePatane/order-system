using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Product.Core.Repositories;
using Product.Infrastructure.EF;
using Product.Infrastructure.Repositories;

namespace Product.Infrastructure.Extensions;

/// <summary>
/// Extension methods for registering infrastructure services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Product infrastructure services including DbContext and repositories
    /// </summary>
    public static IServiceCollection AddProductInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<ProductDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("productdb"),
                npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);
                });
        });


        services.AddProductRepositories();

        return services;
    }

    /// <summary>
    /// Adds only repositories (use when DbContext is already registered)
    /// </summary>
    public static IServiceCollection AddProductRepositories(this IServiceCollection services)
    {
        // Prefer the split implementations
        services.AddScoped<IProductReadOnlyRepository, ProductReadOnlyRepository>();
        services.AddScoped<IProductWriteRepository, ProductWriteRepository>();

        services.AddScoped<ICategoryReadOnlyRepository, CategoryReadOnlyRepository>();
        
         return services;
     }
 }
