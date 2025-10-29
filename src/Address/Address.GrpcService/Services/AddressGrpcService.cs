using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using Address.Protos;
using Address.Core.Repositories;
using Address.Application.Commands.CreateAddress;
using Address.Application.Commands.UpdateAddress;
using Address.Application.Commands.DeleteAddress;
using Address.Application.Commands.SetDefaultAddress;
using Shared.GrpcInfrastructure.Base;
using Shared.Core.Domain.Errors;

namespace Address.GrpcService.Services;

public class AddressGrpcService : AddressService.AddressServiceBase
{
    private readonly ILogger<AddressGrpcService> _logger;
    private readonly IAddressReadOnlyRepository _addressRepository;
    private readonly CreateAddressHandler _createAddressHandler;
    private readonly UpdateAddressHandler _updateAddressHandler;
    private readonly DeleteAddressHandler _deleteAddressHandler;
    private readonly SetDefaultAddressHandler _setDefaultAddressHandler;
    private readonly GrpcServiceBase<AddressGrpcService> _baseService;

    public AddressGrpcService(
        ILogger<AddressGrpcService> logger,
        IAddressReadOnlyRepository addressRepository,
        CreateAddressHandler createAddressHandler,
        UpdateAddressHandler updateAddressHandler,
        DeleteAddressHandler deleteAddressHandler,
        SetDefaultAddressHandler setDefaultAddressHandler)
    {
        _logger = logger;
        _addressRepository = addressRepository;
        _createAddressHandler = createAddressHandler;
        _updateAddressHandler = updateAddressHandler;
        _deleteAddressHandler = deleteAddressHandler;
        _setDefaultAddressHandler = setDefaultAddressHandler;
        _baseService = new InternalGrpcServiceBase(logger);
    }

    public override async Task<AddressResponse> GetAddress(GetAddressRequest request, ServerCallContext context)
    {
        try
        {
            if (!_baseService.IsValidGuid(request.AddressId, out var addressId))
            {
                return new AddressResponse
                {
                    Error = _baseService.CreateInvalidArgumentError("AddressId", "invalid or missing GUID")
                };
            }

            var address = await _addressRepository.GetByIdAsync(addressId, context.CancellationToken);

            if (address == null)
            {
                return new AddressResponse
                {
                    Error = _baseService.CreateNotFoundError("Address", request.AddressId)
                };
            }

            return new AddressResponse
            {
                Data = MapToAddressData(address)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetAddress for AddressId: {AddressId}", request.AddressId);
            return new AddressResponse { Error = _baseService.CreateInternalError() };
        }
    }

    public override async Task<GetAddressesResponse> GetAddressesByUser(GetAddressesByUserRequest request, ServerCallContext context)
    {
        try
        {
            if (!_baseService.IsValidGuid(request.UserId, out var userId))
            {
                return new GetAddressesResponse
                {
                    Error = _baseService.CreateInvalidArgumentError("UserId", "invalid or missing GUID")
                };
            }

            var addresses = await _addressRepository.GetByUserIdAsync(userId, context.CancellationToken);

            var response = new AddressListData();
            response.Items.AddRange(addresses.Select(MapToAddressData));

            return new GetAddressesResponse
            {
                Data = response
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetAddressesByUser for UserId: {UserId}", request.UserId);
            return new GetAddressesResponse { Error = _baseService.CreateInternalError() };
        }
    }

    public override async Task<AddressResponse> GetDefaultAddress(GetDefaultAddressRequest request, ServerCallContext context)
    {
        try
        {
            if (!_baseService.IsValidGuid(request.UserId, out var userId))
            {
                return new AddressResponse
                {
                    Error = _baseService.CreateInvalidArgumentError("UserId", "invalid or missing GUID")
                };
            }

            var address = await _addressRepository.GetDefaultByUserIdAsync(userId, context.CancellationToken);

            if (address == null)
            {
                return new AddressResponse
                {
                    Error = _baseService.CreateNotFoundError("Default Address", $"for user {request.UserId}")
                };
            }

            return new AddressResponse
            {
                Data = MapToAddressData(address)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetDefaultAddress for UserId: {UserId}", request.UserId);
            return new AddressResponse { Error = _baseService.CreateInternalError() };
        }
    }

    public override async Task<GetPagedAddressesResponse> GetPagedAddressesByUser(GetPagedAddressesRequest request, ServerCallContext context)
    {
        try
        {
            if (!_baseService.IsValidGuid(request.UserId, out var userId))
            {
                return new GetPagedAddressesResponse
                {
                    Error = _baseService.CreateInvalidArgumentError("UserId", "invalid or missing GUID")
                };
            }

            var (items, totalCount) = await _addressRepository.GetPagedByUserIdAsync(
                userId,
                request.PageNumber,
                request.PageSize,
                context.CancellationToken);

            var response = new PagedAddressListData
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
            response.Items.AddRange(items.Select(MapToAddressData));

            return new GetPagedAddressesResponse
            {
                Data = response
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetPagedAddressesByUser for UserId: {UserId}", request.UserId);
            return new GetPagedAddressesResponse { Error = _baseService.CreateInternalError() };
        }
    }

    public override async Task<AddressResponse> CreateAddress(CreateAddressRequest request, ServerCallContext context)
    {
        try
        {
            if (!_baseService.IsValidGuid(request.UserId, out var userId))
            {
                return new AddressResponse
                {
                    Error = _baseService.CreateInvalidArgumentError("UserId", "invalid or missing GUID")
                };
            }

            var command = new CreateAddressCommand(
                userId,
                request.Street,
                request.City,
                request.State,
                request.PostalCode,
                request.Country,
                request.Street2,
                request.Label,
                request.IsDefault);

            var result = await _createAddressHandler.Handle(command, context.CancellationToken);

            if (result.IsFailure)
            {
                var error = result.Error;
                if (error is ValidationError ve)
                {
                    return new AddressResponse
                    {
                        Error = _baseService.CreateInvalidArgumentError(ve.FieldName, ve.Reason)
                    };
                }

                return new AddressResponse { Error = _baseService.CreateInternalError(error.Message) };
            }

            var createdAddress = await _addressRepository.GetByIdAsync(result.Value.AddressId, context.CancellationToken);

            return new AddressResponse
            {
                Data = MapToAddressData(createdAddress!)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in CreateAddress");
            return new AddressResponse { Error = _baseService.CreateInternalError() };
        }
    }

    public override async Task<AddressResponse> UpdateAddress(UpdateAddressRequest request, ServerCallContext context)
    {
        try
        {
            if (!_baseService.IsValidGuid(request.AddressId, out var addressId))
            {
                return new AddressResponse
                {
                    Error = _baseService.CreateInvalidArgumentError("AddressId", "invalid or missing GUID")
                };
            }

            var command = new UpdateAddressCommand(
                addressId,
                request.Street,
                request.Street2,
                request.City,
                request.State,
                request.PostalCode,
                request.Country,
                request.Label);

            var result = await _updateAddressHandler.Handle(command, context.CancellationToken);

            if (result.IsFailure)
            {
                var error = result.Error;
                if (error is ValidationError ve)
                {
                    return new AddressResponse
                    {
                        Error = _baseService.CreateInvalidArgumentError(ve.FieldName, ve.Reason)
                    };
                }
                if (error is NotFoundError)
                {
                    return new AddressResponse
                    {
                        Error = _baseService.CreateNotFoundError("Address", request.AddressId)
                    };
                }

                return new AddressResponse { Error = _baseService.CreateInternalError(error.Message) };
            }

            var updatedAddress = await _addressRepository.GetByIdAsync(result.Value.AddressId, context.CancellationToken);

            return new AddressResponse
            {
                Data = MapToAddressData(updatedAddress!)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in UpdateAddress for AddressId: {AddressId}", request.AddressId);
            return new AddressResponse { Error = _baseService.CreateInternalError() };
        }
    }

    public override async Task<DeleteAddressResponse> DeleteAddress(DeleteAddressRequest request, ServerCallContext context)
    {
        try
        {
            if (!_baseService.IsValidGuid(request.AddressId, out var addressId))
            {
                return new DeleteAddressResponse
                {
                    Error = _baseService.CreateInvalidArgumentError("AddressId", "invalid or missing GUID")
                };
            }

            var command = new DeleteAddressCommand(addressId);
            var result = await _deleteAddressHandler.Handle(command, context.CancellationToken);

            if (result.IsFailure)
            {
                var error = result.Error;
                if (error is NotFoundError)
                {
                    return new DeleteAddressResponse
                    {
                        Error = _baseService.CreateNotFoundError("Address", request.AddressId)
                    };
                }

                return new DeleteAddressResponse { Error = _baseService.CreateInternalError(error.Message) };
            }

            return new DeleteAddressResponse
            {
                Data = new DeleteAddressData
                {
                    Success = result.Value.Success,
                    AddressId = result.Value.AddressId.ToString()
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in DeleteAddress for AddressId: {AddressId}", request.AddressId);
            return new DeleteAddressResponse { Error = _baseService.CreateInternalError() };
        }
    }

    public override async Task<AddressResponse> SetDefaultAddress(SetDefaultAddressRequest request, ServerCallContext context)
    {
        try
        {
            if (!_baseService.IsValidGuid(request.AddressId, out var addressId))
            {
                return new AddressResponse
                {
                    Error = _baseService.CreateInvalidArgumentError("AddressId", "invalid or missing GUID")
                };
            }

            var command = new SetDefaultAddressCommand(addressId);
            var result = await _setDefaultAddressHandler.Handle(command, context.CancellationToken);

            if (result.IsFailure)
            {
                var error = result.Error;
                if (error is NotFoundError)
                {
                    return new AddressResponse
                    {
                        Error = _baseService.CreateNotFoundError("Address", request.AddressId)
                    };
                }

                return new AddressResponse { Error = _baseService.CreateInternalError(error.Message) };
            }

            var updatedAddress = await _addressRepository.GetByIdAsync(result.Value.AddressId, context.CancellationToken);

            return new AddressResponse
            {
                Data = MapToAddressData(updatedAddress!)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in SetDefaultAddress for AddressId: {AddressId}", request.AddressId);
            return new AddressResponse { Error = _baseService.CreateInternalError() };
        }
    }

    private static AddressData MapToAddressData(Core.Domain.AddressEntity address)
    {
        return new AddressData
        {
            AddressId = address.Id.ToString(),
            UserId = address.UserId.ToString(),
            Street = address.Street,
            Street2 = address.Street2 ?? "",
            City = address.City,
            State = address.State,
            PostalCode = address.PostalCode,
            Country = address.Country,
            Label = address.Label ?? "",
            IsDefault = address.IsDefault,
            CreatedAt = Timestamp.FromDateTime(DateTime.SpecifyKind(address.CreatedAt, DateTimeKind.Utc)),
            UpdatedAt = address.UpdatedAt.HasValue
                ? Timestamp.FromDateTime(DateTime.SpecifyKind(address.UpdatedAt.Value, DateTimeKind.Utc))
                : null
        };
    }

    private class InternalGrpcServiceBase : GrpcServiceBase<AddressGrpcService>
    {
        public InternalGrpcServiceBase(ILogger<AddressGrpcService> logger) : base(logger)
        {
        }
    }
}
