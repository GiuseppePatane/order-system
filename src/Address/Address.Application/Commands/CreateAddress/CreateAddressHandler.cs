using Address.Core.Domain;
using Address.Core.Repositories;

using Shared.Core.Domain.Results;

namespace Address.Application.Commands.CreateAddress;

public class CreateAddressHandler
{
    private readonly IAddressWriteRepository _writeRepository;
    private readonly IAddressReadOnlyRepository _readRepository;

    public CreateAddressHandler(
        IAddressWriteRepository writeRepository,
        IAddressReadOnlyRepository readRepository)
    {
        _writeRepository = writeRepository;
        _readRepository = readRepository;
    }

    public async Task<Result<CreateAddressResult>> Handle(CreateAddressCommand request, CancellationToken cancellationToken)
    {
        // Create the address entity
        var addressResult = AddressEntity.Create(
            request.UserId,
            request.Street,
            request.City,
            request.State,
            request.PostalCode,
            request.Country,
            request.Street2,
            request.Label,
            request.IsDefault
        );

        if (!addressResult.IsSuccess)
        {
            return Result<CreateAddressResult>.Failure(addressResult.Error!);
        }

        var address = addressResult.Value!;

        // If this is set as default, unset all other defaults for this user
        if (address.IsDefault)
        {
            await _writeRepository.UnsetAllDefaultsForUserAsync(request.UserId, cancellationToken);
        }

        // Save the address
        var savedAddress = await _writeRepository.AddAsync(address, cancellationToken);
        if (!savedAddress.IsSuccess)
        {
            return Result<CreateAddressResult>.Failure(savedAddress.Error!);
        }

        return Result<CreateAddressResult>.Success(new CreateAddressResult(address.Id));
    }
}
