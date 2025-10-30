using Microsoft.Extensions.DependencyInjection;
using Address.Application.Commands.CreateAddress;
using Address.Application.Commands.UpdateAddress;
using Address.Application.Commands.DeleteAddress;
using Address.Application.Commands.SetDefaultAddress;

namespace Address.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAddressApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateAddressHandler>();
        services.AddScoped<UpdateAddressHandler>();
        services.AddScoped<DeleteAddressHandler>();
        services.AddScoped<SetDefaultAddressHandler>();

        return services;
    }
}
