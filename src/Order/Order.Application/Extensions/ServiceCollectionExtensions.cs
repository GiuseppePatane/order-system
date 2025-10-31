using Microsoft.Extensions.DependencyInjection;
using Order.Application.Commands.CreateOrder;
using Order.Application.Commands.UpdateOrderStatus;
using Order.Application.Commands.CancelOrder;
using Order.Application.Commands.AddOrderItem;
using Order.Application.Commands.RemoveOrderItem;
using Order.Application.Commands.UpdateOrderItemQuantity;

namespace Order.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderApplication(this IServiceCollection services)
    {
 
        services.AddScoped<CreateOrderHandler>();
        services.AddScoped<UpdateOrderStatusHandler>();
        services.AddScoped<CancelOrderHandler>();
        
        services.AddScoped<AddOrderItemHandler>();
        services.AddScoped<RemoveOrderItemHandler>();
        services.AddScoped<UpdateOrderItemQuantityHandler>();

        return services;
    }
}
