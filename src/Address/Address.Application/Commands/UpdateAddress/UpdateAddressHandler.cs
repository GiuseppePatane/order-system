using Address.Core.Repositories;
using Shared.Core.Domain.Results;
using Shared.Core.Domain.Errors;

namespace Address.Application.Commands.UpdateAddress;

public class UpdateAddressHandler
{
    private readonly IAddressWriteRepository _writeRepository;
    private readonly IAddressReadOnlyRepository _readRepository;

    public UpdateAddressHandler(
        IAddressWriteRepository writeRepository,
        IAddressReadOnlyRepository readRepository)
    {
        _writeRepository = writeRepository;
        _readRepository = readRepository;
    }

    public async Task<Result<UpdateAddressResult>> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
    {
       
        var address = await _readRepository.GetByIdAsync(request.AddressId, cancellationToken);
        if (address == null)
        {
            return Result<UpdateAddressResult>.Failure(new NotFoundError("Address", request.AddressId.ToString()));
        }
        
  
        var updateResult = address.Update(
            request.Street,
            request.Street2,
            request.City,
            request.State,
            request.PostalCode,
            request.Country,
            request.Label
        );

        if (!updateResult.IsSuccess)
        {
            return Result<UpdateAddressResult>.Failure(updateResult.Error!);
        }

      
        var updatedAddressResults = await _writeRepository.UpdateAsync(address, cancellationToken);
        if (!updatedAddressResults.IsSuccess)
        {
            return Result<UpdateAddressResult>.Failure(updatedAddressResults.Error!);
        }

        return Result<UpdateAddressResult>.Success(new UpdateAddressResult(address.Id));
    }
}
