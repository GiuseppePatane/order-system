using Address.Core.Repositories;
using Shared.Core.Domain.Results;

using Shared.Core.Domain.Errors;
namespace Address.Application.Commands.DeleteAddress;

public class DeleteAddressHandler
{
    private readonly IAddressWriteRepository _writeRepository;
    private readonly IAddressReadOnlyRepository _readRepository;

    public DeleteAddressHandler(
        IAddressWriteRepository writeRepository,
        IAddressReadOnlyRepository readRepository)
    {
        _writeRepository = writeRepository;
        _readRepository = readRepository;
    }

    public async Task<Result<DeleteAddressResult>> Handle(DeleteAddressCommand request, CancellationToken cancellationToken)
    {
        // Check if address exists
        var exists = await _readRepository.ExistsAsync(request.AddressId, cancellationToken);
        if (!exists)
        {
            return Result<DeleteAddressResult>.Failure(new NotFoundError("Address", request.AddressId.ToString()));
        }

        // Delete the address
        var deleted = await _writeRepository.DeleteAsync(request.AddressId, cancellationToken);

        if (!deleted.IsSuccess)
        {
            return Result<DeleteAddressResult>.Failure(deleted.Error!);
        }

        return Result<DeleteAddressResult>.Success(new DeleteAddressResult(true, request.AddressId));
    }
}
