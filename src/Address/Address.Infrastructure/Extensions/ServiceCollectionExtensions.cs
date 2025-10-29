using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Address.Core.Repositories;
using Address.Infrastructure.EF;
using Address.Infrastructure.Repositories;

namespace Address.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAddressInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AddressDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("addressdb"));
        });

        services.AddAddressRepositories();

        return services;
    }

    public static IServiceCollection AddAddressRepositories(this IServiceCollection services)
    {
        services.AddScoped<IAddressReadOnlyRepository, AddressReadOnlyRepository>();
        services.AddScoped<IAddressWriteRepository, AddressWriteRepository>();

        return services;
    }
}
