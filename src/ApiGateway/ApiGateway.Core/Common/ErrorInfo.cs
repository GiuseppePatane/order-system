namespace ApiGateway.Core.Common;

public class ErrorInfo
{
    public required string Code { get; init; }
    public required string Message { get; init; }
    public Dictionary<string, string>? Details { get; init; }
}