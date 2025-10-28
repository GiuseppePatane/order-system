namespace ApiGateway.Core.Common;

public class ServiceResult<T> where T : class
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public ErrorInfo? Error { get; init; }

    public static ServiceResult<T> Success(T data) => new()
    {
        IsSuccess = true,
        Data = data
    };

    public static ServiceResult<T> Failure(ErrorInfo error) => new()
    {
        IsSuccess = false,
        Error = error
    };
}