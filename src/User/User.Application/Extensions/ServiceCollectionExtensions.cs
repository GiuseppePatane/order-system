using Microsoft.Extensions.DependencyInjection;
using User.Application.Commands.CreateUser;
using User.Application.Commands.UpdateUser;
using User.Application.Commands.DeleteUser;

namespace User.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUserApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateUserHandler>();
        services.AddScoped<UpdateUserHandler>();
        services.AddScoped<DeleteUserHandler>();

        return services;
    }
}

