using Microsoft.Extensions.DependencyInjection;
using Order.Application.Commands.CreateOrder;
using Order.Application.Commands.UpdateOrderStatus;
using Order.Application.Commands.CancelOrder;

namespace Order.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderApplication(this IServiceCollection services)
    {
        // Register handlers
        services.AddScoped<CreateOrderHandler>();
        services.AddScoped<UpdateOrderStatusHandler>();
        services.AddScoped<CancelOrderHandler>();

        return services;
    }
}
