using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace Shared.GrpcInfrastructure.Interceptors;

/// <summary>
/// Interceptor that handles exceptions in gRPC calls and converts them to proper RpcException responses.
/// This provides centralized error handling for all gRPC services.
/// </summary>
public class ErrorHandlingInterceptor : Interceptor
{
    private readonly ILogger<ErrorHandlingInterceptor> _logger;

    public ErrorHandlingInterceptor(ILogger<ErrorHandlingInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (RpcException)
        {
            
            throw;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument in gRPC call: {Method}", context.Method);
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation in gRPC call: {Method}", context.Method);
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found in gRPC call: {Method}", context.Method);
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access in gRPC call: {Method}", context.Method);
            throw new RpcException(new Status(StatusCode.PermissionDenied, ex.Message));
        }
        catch (TimeoutException ex)
        {
            _logger.LogWarning(ex, "Timeout in gRPC call: {Method}", context.Method);
            throw new RpcException(new Status(StatusCode.DeadlineExceeded, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in gRPC call: {Method}", context.Method);

            //  no exception to the client about internal errors
            var message = "An internal error occurred. Please contact support.";
            throw new RpcException(new Status(StatusCode.Internal, message));
        }
    }

    public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        ServerCallContext context,
        ClientStreamingServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(requestStream, context);
        }
        catch (Exception ex)
        {
            return await HandleStreamingException<TResponse>(ex, context);
        }
    }

    public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
        TRequest request,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        ServerStreamingServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            await continuation(request, responseStream, context);
        }
        catch (Exception ex)
        {
            await HandleStreamingException<object>(ex, context);
        }
    }

    public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        DuplexStreamingServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            await continuation(requestStream, responseStream, context);
        }
        catch (Exception ex)
        {
            await HandleStreamingException<object>(ex, context);
        }
    }

    private Task<T> HandleStreamingException<T>(Exception ex, ServerCallContext context)
    {
        if (ex is RpcException)
        {
            throw ex;
        }

        _logger.LogError(ex, "Unhandled exception in streaming gRPC call: {Method}", context.Method);
        throw new RpcException(new Status(StatusCode.Internal, "An internal error occurred"));
    }
}