using Order.Core.Repositories;
using Shared.Core.Domain.Results;
using Shared.Core.Domain.Errors;

namespace Order.Application.Commands.UpdateOrderStatus;

public class UpdateOrderStatusHandler
{
    private readonly IOrderWriteRepository _writeRepository;
    private readonly IOrderReadOnlyRepository _readRepository;

    public UpdateOrderStatusHandler(
        IOrderWriteRepository writeRepository,
        IOrderReadOnlyRepository readRepository)
    {
        _writeRepository = writeRepository;
        _readRepository = readRepository;
    }

    public async Task<Result<UpdateOrderStatusResult>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        // Get the existing order
        var order = await _readRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            return Result<UpdateOrderStatusResult>.Failure(new NotFoundError("Order", request.OrderId.ToString()));
        }

        // Update the status
        var updateResult = order.UpdateStatus(request.Status);
        if (!updateResult.IsSuccess)
        {
            return Result<UpdateOrderStatusResult>.Failure(updateResult.Error!);
        }

        // Save changes
        var updatedOrder = await _writeRepository.UpdateAsync(order, cancellationToken);

        return Result<UpdateOrderStatusResult>.Success(new UpdateOrderStatusResult(updatedOrder.Id));
    }
}
