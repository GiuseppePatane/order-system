using ApiGateway.Core.Common;
using ApiGateway.Core.User;
using ApiGateway.Core.User.Dto;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using User.Protos;

namespace ApiGateway.infrastructure.GrcpClient.User;

public class UserServiceGrpcClient : IUserServiceClient
{
    private readonly UserService.UserServiceClient _grpcClient;
    private readonly ILogger<UserServiceGrpcClient> _logger;

    public UserServiceGrpcClient(
        UserService.UserServiceClient grpcClient,
        ILogger<UserServiceGrpcClient> logger
    )
    {
        _grpcClient = grpcClient;
        _logger = logger;
    }

    public async Task<ServiceResult<UserDto>> GetUserById(string userId, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Calling gRPC service to get user with ID: {UserId}", userId);

            var request = new GetUserRequest { UserId = userId };

            var response = await _grpcClient.GetUserAsync(request, cancellationToken: cancellationToken);

            return response.ResultCase switch
            {
                UserResponse.ResultOneofCase.Data => ServiceResult<UserDto>.Success(
                    MapToDto(response.Data)
                ),
                UserResponse.ResultOneofCase.Error => ServiceResult<UserDto>.Failure(
                    MapToErrorInfo(response.Error)
                ),
                _ => ServiceResult<UserDto>.Failure(
                    new ErrorInfo
                    {
                        Code = "EMPTY_RESPONSE",
                        Message = "The gRPC service returned an empty response",
                    }
                ),
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC call failed for UserId: {UserId}", userId);

            return ServiceResult<UserDto>.Failure(
                new ErrorInfo
                {
                    Code = "GRPC_ERROR",
                    Message = $"Failed to communicate with the user service: {ex.Status.Detail}",
                    Details = new Dictionary<string, string>
                    {
                        ["StatusCode"] = ex.StatusCode.ToString(),
                        ["Detail"] = ex.Status.Detail ?? string.Empty,
                    },
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error occurred while calling user service for UserId: {UserId}",
                userId
            );

            return ServiceResult<UserDto>.Failure(
                new ErrorInfo
                {
                    Code = "UNEXPECTED_ERROR",
                    Message = "An unexpected error occurred while retrieving the user",
                    Details = new Dictionary<string, string>
                    {
                        ["ExceptionType"] = ex.GetType().Name,
                        ["Message"] = ex.Message,
                    },
                }
            );
        }
    }

    public async Task<ServiceResult<UserMutationResultDto>> CreateUser(CreateUserRequestDto requestDto)
    {
        try
        {
            var request = new CreateUserRequest
            {
                FirstName = requestDto.FirstName,
                LastName = requestDto.LastName,
                Email = requestDto.Email,
            };

            var response = await _grpcClient.CreateUserAsync(request);

            return response.ResultCase switch
            {
                UserResponse.ResultOneofCase.Data => ServiceResult<UserMutationResultDto>.Success(
                    new UserMutationResultDto(response.Data.UserId)
                ),
                UserResponse.ResultOneofCase.Error => ServiceResult<UserMutationResultDto>.Failure(
                    MapToErrorInfo(response.Error)
                ),
                _ => ServiceResult<UserMutationResultDto>.Failure(
                    new ErrorInfo { Code = "EMPTY_RESPONSE", Message = "Empty response from gRPC" }
                ),
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC CreateUser failed");
            return ServiceResult<UserMutationResultDto>.Failure(
                new ErrorInfo { Code = "GRPC_ERROR", Message = ex.Status.Detail }
            );
        }
    }

    public async Task<ServiceResult<PagedUsersDto>> GetUsers(GetUsersRequestDto requestDto)
    {
        try
        {
            var request = new GetUsersRequest
            {
                PageNumber = requestDto.PageNumber,
                PageSize = requestDto.PageSize,
            };

            if (!string.IsNullOrWhiteSpace(requestDto.SearchTerm))
                request.SearchTerm = requestDto.SearchTerm;

            var response = await _grpcClient.GetUsersAsync(request);

            return response.ResultCase switch
            {
                GetUsersResponse.ResultOneofCase.Data => ServiceResult<PagedUsersDto>.Success(
                    MapToPagedDto(response.Data)
                ),
                GetUsersResponse.ResultOneofCase.Error => ServiceResult<PagedUsersDto>.Failure(
                    MapToErrorInfo(response.Error)
                ),
                _ => ServiceResult<PagedUsersDto>.Failure(
                    new ErrorInfo { Code = "EMPTY_RESPONSE", Message = "Empty response from gRPC" }
                ),
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC GetUsers failed");
            return ServiceResult<PagedUsersDto>.Failure(
                new ErrorInfo { Code = "GRPC_ERROR", Message = ex.Status.Detail }
            );
        }
    }

    public async Task<ServiceResult<UserMutationResultDto>> UpdateUser(string id, UpdateUserRequestDto dto)
    {
        try
        {
            var request = new UpdateUserRequest { UserId = id };

            if (!string.IsNullOrWhiteSpace(dto.FirstName))
                request.FirstName = dto.FirstName;
            if (!string.IsNullOrWhiteSpace(dto.LastName))
                request.LastName = dto.LastName;
            if (!string.IsNullOrWhiteSpace(dto.Email))
                request.Email = dto.Email;

            var response = await _grpcClient.UpdateUserAsync(request);

            return response.ResultCase switch
            {
                UserResponse.ResultOneofCase.Data => ServiceResult<UserMutationResultDto>.Success(
                    new UserMutationResultDto(response.Data.UserId)
                ),
                UserResponse.ResultOneofCase.Error => ServiceResult<UserMutationResultDto>.Failure(
                    MapToErrorInfo(response.Error)
                ),
                _ => ServiceResult<UserMutationResultDto>.Failure(
                    new ErrorInfo { Code = "EMPTY_RESPONSE", Message = "Empty response from gRPC" }
                ),
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC UpdateUser failed");
            return ServiceResult<UserMutationResultDto>.Failure(
                new ErrorInfo { Code = "GRPC_ERROR", Message = ex.Status.Detail }
            );
        }
    }

    public async Task<ServiceResult<UserMutationResultDto>> DeleteUser(string userId)
    {
        try
        {
            var request = new DeleteUserRequest { UserId = userId };
            var response = await _grpcClient.DeleteUserAsync(request);

            return response.ResultCase switch
            {
                DeleteUserResponse.ResultOneofCase.Data =>
                    ServiceResult<UserMutationResultDto>.Success(
                        new UserMutationResultDto(response.Data.UserId)
                    ),
                DeleteUserResponse.ResultOneofCase.Error =>
                    ServiceResult<UserMutationResultDto>.Failure(MapToErrorInfo(response.Error)),
                _ => ServiceResult<UserMutationResultDto>.Failure(
                    new ErrorInfo { Code = "EMPTY_RESPONSE", Message = "Empty response from gRPC" }
                ),
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC DeleteUser failed");
            return ServiceResult<UserMutationResultDto>.Failure(
                new ErrorInfo { Code = "GRPC_ERROR", Message = ex.Status.Detail }
            );
        }
    }

    private static UserDto MapToDto(UserData data)
    {
        return new UserDto
        {
            UserId = data.UserId,
            FirstName = data.FirstName,
            LastName = data.LastName,
            Email = data.Email,
        };
    }

    private static ErrorInfo MapToErrorInfo(Shared.Contracts.ErrorResponse error)
    {
        return new ErrorInfo
        {
            Code = error.Code,
            Message = error.Message,
            Details =
                error.Details?.Count > 0 ? new Dictionary<string, string>(error.Details) : null,
        };
    }

    private static PagedUsersDto MapToPagedDto(UserListData data)
    {
        return new PagedUsersDto
        {
            Items = data.Items.Select(MapToDto).ToList(),
            PageNumber = data.PageNumber,
            PageSize = data.PageSize,
            TotalCount = data.TotalCount,
        };
    }
}
