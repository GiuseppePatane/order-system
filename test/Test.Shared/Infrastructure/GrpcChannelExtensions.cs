using Grpc.Net.Client;

namespace Test.Shared.Infrastructure;

/// <summary>
/// Extension methods for creating gRPC channels from HttpClient
/// </summary>
public static class GrpcChannelExtensions
{
    /// <summary>
    /// Creates a gRPC channel from an HttpClient
    /// </summary>
    public static GrpcChannel CreateChannel(this HttpClient httpClient)
    {
        return GrpcChannel.ForAddress(httpClient.BaseAddress!, new GrpcChannelOptions
        {
            HttpClient = httpClient
        });
    }
}
