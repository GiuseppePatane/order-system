using ApiGateway.Core.Product;
using ApiGateway.Core.User;
using ApiGateway.Core.Address;
using ApiGateway.Core.Order;
using ApiGateway.infrastructure.GrcpClient.Product;
using ApiGateway.infrastructure.GrcpClient.User;
using ApiGateway.infrastructure.GrcpClient.Address;
using ApiGateway.infrastructure.GrcpClient.Order;
using Microsoft.Extensions.DependencyInjection;
using Products;
using Address.Protos;
using Order.Protos;
using User.Protos;

namespace ApiGateway.infrastructure.GrcpClient.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProductGrpcClient(
        this IServiceCollection services,
        string grpcServiceUrl)
    {

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

        services.AddScoped<IProductServiceClient, ProductServiceGrpcClient>();

        return services;
    }

    public static IServiceCollection AddUserGrpcClient(
        this IServiceCollection services,
        string grpcServiceUrl)
    {

        services.AddGrpcClient<UserService.UserServiceClient>(options =>
            {
                options.Address = new Uri(grpcServiceUrl);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = true
            })
            .AddServiceDiscovery()
            .AddStandardResilienceHandler();

        services.AddScoped<IUserServiceClient, UserServiceGrpcClient>();
        services.AddScoped<IUserOrchestratorService, UserOchestratorService>();
        return services;
    }

    public static IServiceCollection AddAddressGrpcClient(
        this IServiceCollection services,
        string grpcServiceUrl)
    {

        services.AddGrpcClient<AddressService.AddressServiceClient>(options =>
            {
                options.Address = new Uri(grpcServiceUrl);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = true
            })
            .AddServiceDiscovery()
            .AddStandardResilienceHandler();

        services.AddScoped<IAddressServiceClient, AddressServiceGrpcClient>();

        return services;
    }

    public static IServiceCollection AddOrderGrpcClient(
        this IServiceCollection services,
        string grpcServiceUrl)
    {

        services.AddGrpcClient<OrderService.OrderServiceClient>(options =>
            {
                options.Address = new Uri(grpcServiceUrl);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = true
            })
            .AddServiceDiscovery()
            .AddStandardResilienceHandler();

        services.AddScoped<IOrderServiceClient, OrderServiceGrpcClient>();

        services.AddScoped<IOrderOrchestrationService, OrderOrchestrationService>();

        return services;
    }
}