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
   
        var addressExists = await _readRepository.GetByIdAsync(request.AddressId, cancellationToken);
        if (addressExists == null)
        {
            return Result<SetDefaultAddressResult>.Failure(new NotFoundError("Address", request.AddressId.ToString()));
        }
        
        await _writeRepository.SetDefaultAddressAsync(request.AddressId, addressExists.UserId, cancellationToken);

        return Result<SetDefaultAddressResult>.Success(new SetDefaultAddressResult(request.AddressId));
    }
}
