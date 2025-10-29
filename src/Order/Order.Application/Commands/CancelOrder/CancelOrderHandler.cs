using Order.Core.Repositories;
using Shared.Core.Domain.Results;
using Shared.Core.Domain.Errors;

namespace Order.Application.Commands.CancelOrder;

public class CancelOrderHandler
{
    private readonly IOrderWriteRepository _writeRepository;
    private readonly IOrderReadOnlyRepository _readRepository;

    public CancelOrderHandler(
        IOrderWriteRepository writeRepository,
        IOrderReadOnlyRepository readRepository)
    {
        _writeRepository = writeRepository;
        _readRepository = readRepository;
    }

    public async Task<Result<CancelOrderResult>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        // Get the existing order
        var order = await _readRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            return Result<CancelOrderResult>.Failure(new NotFoundError("Order", request.OrderId.ToString()));
        }

        // Cancel the order
        var cancelResult = order.Cancel(request.Reason);
        if (!cancelResult.IsSuccess)
        {
            return Result<CancelOrderResult>.Failure(cancelResult.Error!);
        }

        // Save changes
        var updatedOrder = await _writeRepository.UpdateAsync(order, cancellationToken);

        return Result<CancelOrderResult>.Success(new CancelOrderResult(updatedOrder.Id));
    }
}
