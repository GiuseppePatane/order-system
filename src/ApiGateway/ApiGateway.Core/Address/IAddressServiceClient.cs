using ApiGateway.Core.Address.Dto;
using ApiGateway.Core.Common;

namespace ApiGateway.Core.Address;

public interface IAddressServiceClient
{
    Task<ServiceResult<AddressDto>> GetAddressById(string addressId, CancellationToken cancellationToken=default);

    Task<ServiceResult<List<AddressDto>>> GetAddressesByUser(string userId,  CancellationToken cancellationToken=default);
    Task<ServiceResult<AddressDto>> GetDefaultAddress(string userId,  CancellationToken cancellationToken=default);

    Task<ServiceResult<PagedAddressesDto>> GetPagedAddressesByUser(string userId, int pageNumber, int pageSize, CancellationToken cancellationToken=default);

    Task<ServiceResult<AddressDto>> CreateAddress(CreateAddressRequestDto request, CancellationToken cancellationToken=default);

    Task<ServiceResult<AddressDto>> UpdateAddress(string addressId, UpdateAddressRequestDto request, CancellationToken cancellationToken=default);

    Task<ServiceResult<DeleteAddressResultDto>> DeleteAddress(string addressId, CancellationToken cancellationToken=default);

    Task<ServiceResult<AddressDto>> SetDefaultAddress(string addressId, CancellationToken cancellationToken=default);
}
