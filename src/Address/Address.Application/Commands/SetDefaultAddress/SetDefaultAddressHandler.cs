using Address.Core.Repositories;
using Shared.Core.Domain.Results;

using Shared.Core.Domain.Errors;
namespace Address.Application.Commands.SetDefaultAddress;

public class SetDefaultAddressHandler
{
    private readonly IAddressWriteRepository _writeRepository;
    private readonly IAddressReadOnlyRepository _readRepository;

    public SetDefaultAddressHandler(
        IAddressWriteRepository writeRepository,
        IAddressReadOnlyRepository readRepository)
    {
        _writeRepository = writeRepository;
        _readRepository = readRepository;
    }

    public async Task<Result<SetDefaultAddressResult>> Handle(SetDefaultAddressCommand request, CancellationToken cancellationToken)
    {
        // Get the address
        var address = await _readRepository.GetByIdAsync(request.AddressId, cancellationToken);
        if (address == null)
        {
            return Result<SetDefaultAddressResult>.Failure(new NotFoundError("Address", request.AddressId.ToString()));
        }

        // Unset all defaults for this user
        await _writeRepository.UnsetAllDefaultsForUserAsync(address.UserId, cancellationToken);

        // Set this address as default
        address.SetAsDefault();

        // Save changes
        await _writeRepository.UpdateAsync(address, cancellationToken);

        return Result<SetDefaultAddressResult>.Success(new SetDefaultAddressResult(address.Id));
    }
}
