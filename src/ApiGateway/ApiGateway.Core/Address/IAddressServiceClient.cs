using ApiGateway.Core.Address.Dto;
using ApiGateway.Core.Common;

namespace ApiGateway.Core.Address;

public interface IAddressServiceClient
{
    Task<ServiceResult<AddressDto>> GetAddressById(string addressId);

    Task<ServiceResult<List<AddressDto>>> GetAddressesByUser(string userId);

    Task<ServiceResult<AddressDto>> GetDefaultAddress(string userId);

    Task<ServiceResult<PagedAddressesDto>> GetPagedAddressesByUser(string userId, int pageNumber, int pageSize);

    Task<ServiceResult<AddressDto>> CreateAddress(CreateAddressRequestDto request);

    Task<ServiceResult<AddressDto>> UpdateAddress(UpdateAddressRequestDto request);

    Task<ServiceResult<DeleteAddressResultDto>> DeleteAddress(string addressId);

    Task<ServiceResult<AddressDto>> SetDefaultAddress(string addressId);
}
