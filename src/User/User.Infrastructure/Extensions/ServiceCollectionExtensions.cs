using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using User.Core.Repositories;
using User.Infrastructure.EF;
using User.Infrastructure.Repositories;

namespace User.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUserInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<UserDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("userdb"));
        });

        services.AddUserRepositories();

        return services;
    }

    public static IServiceCollection AddUserRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserReadOnlyRepository, UserReadOnlyRepository>();
        services.AddScoped<IUserWriteRepository, UserWriteRepository>();

        
        return services;
    }
}
