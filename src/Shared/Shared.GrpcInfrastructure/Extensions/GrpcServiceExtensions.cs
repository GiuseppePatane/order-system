using Microsoft.Extensions.DependencyInjection;
using Shared.GrpcInfrastructure.Interceptors;

namespace Shared.GrpcInfrastructure.Extensions;

/// <summary>
/// Extension methods for configuring gRPC services with common infrastructure
/// </summary>
public static class GrpcServiceExtensions
{
    /// <summary>
    /// Adds gRPC services with error handling interceptor
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddGrpcWithErrorHandling(this IServiceCollection services)
    {
        services.AddGrpc(options =>
        {
            options.Interceptors.Add<ErrorHandlingInterceptor>();
        });

        return services;
    }

    /// <summary>
    /// Adds error handling interceptor to existing gRPC configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddGrpcErrorHandling(this IServiceCollection services)
    {
        services.AddSingleton<ErrorHandlingInterceptor>();
        return services;
    }
}