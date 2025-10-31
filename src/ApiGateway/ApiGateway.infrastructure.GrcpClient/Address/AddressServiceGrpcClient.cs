using ApiGateway.Core.Common;
using ApiGateway.Core.Address;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Address.Protos;
using ApiGateway.Core.Address.Dto;

namespace ApiGateway.infrastructure.GrcpClient.Address;

public class AddressServiceGrpcClient : IAddressServiceClient
{
    private readonly AddressService.AddressServiceClient _grpcClient;
    private readonly ILogger<AddressServiceGrpcClient> _logger;

    public AddressServiceGrpcClient(
        AddressService.AddressServiceClient grpcClient,
        ILogger<AddressServiceGrpcClient> logger)
    {
        _grpcClient = grpcClient;
        _logger = logger;
    }

    public async Task<ServiceResult<AddressDto>> GetAddressById(string addressId, CancellationToken cancellationToken=default)
    {
        try
        {
            _logger.LogInformation("Calling gRPC service to get address with ID: {AddressId}", addressId);

            var request = new GetAddressRequest
            {
                AddressId = addressId
            };

            var response = await _grpcClient.GetAddressAsync(request, cancellationToken: cancellationToken);

            return response.ResultCase switch
            {
                AddressResponse.ResultOneofCase.Data => ServiceResult<AddressDto>.Success(MapToDto(response.Data)),
                AddressResponse.ResultOneofCase.Error => ServiceResult<AddressDto>.Failure(MapToErrorInfo(response.Error)),
                _ => ServiceResult<AddressDto>.Failure(new ErrorInfo
                {
                    Code = "EMPTY_RESPONSE",
                    Message = "The gRPC service returned an empty response"
                })
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC call failed for AddressId: {AddressId}", addressId);

            return ServiceResult<AddressDto>.Failure(new ErrorInfo
            {
                Code = "GRPC_ERROR",
                Message = $"Failed to communicate with the address service: {ex.Status.Detail}",
                Details = new Dictionary<string, string>
                {
                    ["StatusCode"] = ex.StatusCode.ToString(),
                    ["Detail"] = ex.Status.Detail ?? string.Empty
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while calling address service for AddressId: {AddressId}", addressId);

            return ServiceResult<AddressDto>.Failure(new ErrorInfo
            {
                Code = "UNEXPECTED_ERROR",
                Message = "An unexpected error occurred while retrieving the address",
                Details = new Dictionary<string, string>
                {
                    ["ExceptionType"] = ex.GetType().Name,
                    ["Message"] = ex.Message
                }
            });
        }
    }

    public async Task<ServiceResult<List<AddressDto>>> GetAddressesByUser(string userId,CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetAddressesByUserRequest { UserId = userId };
            var response = await _grpcClient.GetAddressesByUserAsync(request, cancellationToken: cancellationToken);

            return response.ResultCase switch
            {
                GetAddressesResponse.ResultOneofCase.Data => ServiceResult<List<AddressDto>>.Success(
                    response.Data.Items.Select(MapToDto).ToList()),
                GetAddressesResponse.ResultOneofCase.Error => ServiceResult<List<AddressDto>>.Failure(MapToErrorInfo(response.Error)),
                _ => ServiceResult<List<AddressDto>>.Failure(new ErrorInfo { Code = "EMPTY_RESPONSE", Message = "Empty response from gRPC" })
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC GetAddressesByUser failed");
            return ServiceResult<List<AddressDto>>.Failure(new ErrorInfo { Code = "GRPC_ERROR", Message = ex.Status.Detail });
        }
    }

    public async Task<ServiceResult<AddressDto>> GetDefaultAddress(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetDefaultAddressRequest { UserId = userId };
            var response = await _grpcClient.GetDefaultAddressAsync(request);

            return response.ResultCase switch
            {
                AddressResponse.ResultOneofCase.Data => ServiceResult<AddressDto>.Success(MapToDto(response.Data)),
                AddressResponse.ResultOneofCase.Error => ServiceResult<AddressDto>.Failure(MapToErrorInfo(response.Error)),
                _ => ServiceResult<AddressDto>.Failure(new ErrorInfo { Code = "EMPTY_RESPONSE", Message = "Empty response from gRPC" })
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC GetDefaultAddress failed");
            return ServiceResult<AddressDto>.Failure(new ErrorInfo { Code = "GRPC_ERROR", Message = ex.Status.Detail });
        }
    }

    public async Task<ServiceResult<PagedAddressesDto>> GetPagedAddressesByUser(string userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetPagedAddressesRequest
            {
                UserId = userId,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var response = await _grpcClient.GetPagedAddressesByUserAsync(request,cancellationToken: cancellationToken);

            return response.ResultCase switch
            {
                GetPagedAddressesResponse.ResultOneofCase.Data => ServiceResult<PagedAddressesDto>.Success(MapToPagedDto(response.Data)),
                GetPagedAddressesResponse.ResultOneofCase.Error => ServiceResult<PagedAddressesDto>.Failure(MapToErrorInfo(response.Error)),
                _ => ServiceResult<PagedAddressesDto>.Failure(new ErrorInfo { Code = "EMPTY_RESPONSE", Message = "Empty response from gRPC" })
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC GetPagedAddressesByUser failed");
            return ServiceResult<PagedAddressesDto>.Failure(new ErrorInfo { Code = "GRPC_ERROR", Message = ex.Status.Detail });
        }
    }

    public async Task<ServiceResult<AddressDto>> CreateAddress(CreateAddressRequestDto requestDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new CreateAddressRequest
            {
                UserId = requestDto.UserId,
                Street = requestDto.Street,
                Street2 = requestDto.Street2 ?? string.Empty,
                City = requestDto.City,
                State = requestDto.State,
                PostalCode = requestDto.PostalCode,
                Country = requestDto.Country,
                Label = requestDto.Label ?? string.Empty,
                IsDefault = requestDto.IsDefault
            };

            var response = await _grpcClient.CreateAddressAsync(request, cancellationToken: cancellationToken);

            return response.ResultCase switch
            {
                AddressResponse.ResultOneofCase.Data => ServiceResult<AddressDto>.Success(MapToDto(response.Data)),
                AddressResponse.ResultOneofCase.Error => ServiceResult<AddressDto>.Failure(MapToErrorInfo(response.Error)),
                _ => ServiceResult<AddressDto>.Failure(new ErrorInfo { Code = "EMPTY_RESPONSE", Message = "Empty response from gRPC" })
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC CreateAddress failed");
            return ServiceResult<AddressDto>.Failure(new ErrorInfo { Code = "GRPC_ERROR", Message = ex.Status.Detail });
        }
    }

    public async Task<ServiceResult<AddressDto>> UpdateAddress(string addressId, UpdateAddressRequestDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new UpdateAddressRequest
            {
                AddressId = addressId
            };

            if (!string.IsNullOrWhiteSpace(dto.Street)) request.Street = dto.Street;
            if (dto.Street2 != null) request.Street2 = dto.Street2;
            if (!string.IsNullOrWhiteSpace(dto.City)) request.City = dto.City;
            if (!string.IsNullOrWhiteSpace(dto.State)) request.State = dto.State;
            if (!string.IsNullOrWhiteSpace(dto.PostalCode)) request.PostalCode = dto.PostalCode;
            if (!string.IsNullOrWhiteSpace(dto.Country)) request.Country = dto.Country;
            if (dto.Label != null) request.Label = dto.Label;

            var response = await _grpcClient.UpdateAddressAsync(request, cancellationToken: cancellationToken);

            return response.ResultCase switch
            {
                AddressResponse.ResultOneofCase.Data => ServiceResult<AddressDto>.Success(MapToDto(response.Data)),
                AddressResponse.ResultOneofCase.Error => ServiceResult<AddressDto>.Failure(MapToErrorInfo(response.Error)),
                _ => ServiceResult<AddressDto>.Failure(new ErrorInfo { Code = "EMPTY_RESPONSE", Message = "Empty response from gRPC" })
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC UpdateAddress failed");
            return ServiceResult<AddressDto>.Failure(new ErrorInfo { Code = "GRPC_ERROR", Message = ex.Status.Detail });
        }
    }

    public async Task<ServiceResult<DeleteAddressResultDto>> DeleteAddress(string addressId, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new DeleteAddressRequest { AddressId = addressId };
            var response = await _grpcClient.DeleteAddressAsync(request, cancellationToken: cancellationToken);

            return response.ResultCase switch
            {
                DeleteAddressResponse.ResultOneofCase.Data => ServiceResult<DeleteAddressResultDto>.Success(
                    new DeleteAddressResultDto { Success = response.Data.Success, AddressId = response.Data.AddressId }),
                DeleteAddressResponse.ResultOneofCase.Error => ServiceResult<DeleteAddressResultDto>.Failure(MapToErrorInfo(response.Error)),
                _ => ServiceResult<DeleteAddressResultDto>.Failure(new ErrorInfo { Code = "EMPTY_RESPONSE", Message = "Empty response from gRPC" })
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC DeleteAddress failed");
            return ServiceResult<DeleteAddressResultDto>.Failure(new ErrorInfo { Code = "GRPC_ERROR", Message = ex.Status.Detail });
        }
    }

    public async Task<ServiceResult<AddressDto>> SetDefaultAddress(string addressId, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new SetDefaultAddressRequest { AddressId = addressId };
            var response = await _grpcClient.SetDefaultAddressAsync(request);

            return response.ResultCase switch
            {
                AddressResponse.ResultOneofCase.Data => ServiceResult<AddressDto>.Success(MapToDto(response.Data)),
                AddressResponse.ResultOneofCase.Error => ServiceResult<AddressDto>.Failure(MapToErrorInfo(response.Error)),
                _ => ServiceResult<AddressDto>.Failure(new ErrorInfo { Code = "EMPTY_RESPONSE", Message = "Empty response from gRPC" })
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC SetDefaultAddress failed");
            return ServiceResult<AddressDto>.Failure(new ErrorInfo { Code = "GRPC_ERROR", Message = ex.Status.Detail });
        }
    }

    private static AddressDto MapToDto(AddressData data)
    {
        return new AddressDto
        {
            AddressId = data.AddressId,
            UserId = data.UserId,
            Street = data.Street,
            Street2 = string.IsNullOrEmpty(data.Street2) ? null : data.Street2,
            City = data.City,
            State = data.State,
            PostalCode = data.PostalCode,
            Country = data.Country,
            Label = string.IsNullOrEmpty(data.Label) ? null : data.Label,
            IsDefault = data.IsDefault,
            CreatedAt = data.CreatedAt.ToDateTime(),
            UpdatedAt = data.UpdatedAt?.ToDateTime()
        };
    }

    private static ErrorInfo MapToErrorInfo(Shared.Contracts.ErrorResponse error)
    {
        return new ErrorInfo
        {
            Code = error.Code,
            Message = error.Message,
            Details = error.Details?.Count > 0 ? new Dictionary<string, string>(error.Details) : null
        };
    }

    private static PagedAddressesDto MapToPagedDto(PagedAddressListData data)
    {
        return new PagedAddressesDto
        {
            Items = data.Items.Select(MapToDto).ToList(),
            PageNumber = data.PageNumber,
            PageSize = data.PageSize,
            TotalCount = data.TotalCount
        };
    }
}
