using Microsoft.Extensions.DependencyInjection;
using Product.Application.Commands.CreateProduct;
using Product.Application.Commands.DeleteProduct;
using Product.Application.Commands.UpdateProduct;
using Product.Application.Commands.LockProductStock;
using Product.Application.Commands.ReleaseProductStock;

namespace Product.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProductApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateProductHandler>();
        services.AddScoped<UpdateProductHandler>();
        services.AddScoped<DeleteProductHandler>();
        services.AddScoped<LockProductStockHandler>();
        services.AddScoped<ReleaseProductStockHandler>();
 
        return services;
    }
}

