using Microsoft.Extensions.Logging;
using Shared.Contracts;

namespace Shared.GrpcInfrastructure.Base;

/// <summary>
/// Base class for gRPC services that provides common error handling utilities
/// </summary>
/// <typeparam name="T">The service type for logging</typeparam>
public abstract class GrpcServiceBase<T> where T : class
{
    protected readonly ILogger<T> Logger;

    protected GrpcServiceBase(ILogger<T> logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Creates a standard error response
    /// </summary>
    public ErrorResponse CreateError(string code, string message, Dictionary<string, string>? details = null)
    {
        var error = new ErrorResponse
        {
            Code = code,
            Message = message
        };

        if (details != null)
        {
            foreach (var (key, value) in details)
            {
                error.Details.Add(key, value);
            }
        }

        return error;
    }

    /// <summary>
    /// Creates a "not found" error response
    /// </summary>
    public ErrorResponse CreateNotFoundError(string resourceType, string resourceId)
    {
        return CreateError(
            $"{resourceType.ToUpperInvariant()}_NOT_FOUND",
            $"{resourceType} with ID '{resourceId}' was not found"
        );
    }

    /// <summary>
    /// Creates an "invalid argument" error response
    /// </summary>
    public ErrorResponse CreateInvalidArgumentError(string fieldName, string reason)
    {
        return CreateError(
            "INVALID_ARGUMENT",
            $"Invalid value for field '{fieldName}': {reason}"
        );
    }

    /// <summary>
    /// Creates an "internal error" response
    /// </summary>
    public ErrorResponse CreateInternalError(string? message = null)
    {
        return CreateError(
            "INTERNAL_ERROR",
            message ?? "An unexpected error occurred"
        );
    }

    /// <summary>
    /// Validates that a string is not null or whitespace
    /// </summary>
    public bool IsValidString(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }
    
    public bool IsValidGuid(string? value, out Guid guid)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            guid = Guid.Empty;
            return false;
        }
        return Guid.TryParse(value, out guid);
    }
}