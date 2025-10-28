using System.Net.Sockets;
using ApiGateway.Core.Product;
using ApiGateway.infrastructure.GrcpClient.Product;
using Microsoft.Extensions.DependencyInjection;
using Products;

namespace ApiGateway.infrastructure.GrcpClient.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProductGrpcClient(
        this IServiceCollection services,
        string grpcServiceUrl)
    {
        // Registra il client gRPC generato con service discovery e resilience
        services.AddGrpcClient<ProductService.ProductServiceClient>(options =>
            {
                options.Address = new Uri(grpcServiceUrl);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = true
            })
            .AddServiceDiscovery()
            .AddStandardResilienceHandler();

        // Registra l'implementazione dell'interfaccia
        services.AddScoped<IProductServiceClient, ProductServiceGrpcClient>();

        return services;
    }
}