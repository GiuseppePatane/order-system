using Order.Core.Repositories;
using Shared.Core.Domain.Errors;
using Shared.Core.Domain.Results;

namespace Order.Application.Commands.RemoveOrderItem;

public class RemoveOrderItemHandler
{
    private readonly IOrderWriteRepository _writeRepository;
    private readonly IOrderReadOnlyRepository _readRepository;

    public RemoveOrderItemHandler(
        IOrderWriteRepository writeRepository,
        IOrderReadOnlyRepository readRepository)
    {
        _writeRepository = writeRepository;
        _readRepository = readRepository;
    }

    public async Task<Result<RemoveOrderItemResult>> Handle(RemoveOrderItemCommand request, CancellationToken cancellationToken)
    {
        // Get the existing order
        var order = await _readRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            return Result<RemoveOrderItemResult>.Failure(new NotFoundError("Order", request.OrderId.ToString()));
        }

        // Validate order is in pending status (only pending orders can be modified)
        if (order.Status != Core.Domain.OrderStatus.Pending)
        {
            return Result<RemoveOrderItemResult>.Failure(new ValidationError(
                nameof(order.Status),
                "Can only remove items from pending orders"));
        }

        // Remove the item from the order
        var removeResult = order.RemoveItem(request.ItemId);
        if (!removeResult.IsSuccess)
        {
            return Result<RemoveOrderItemResult>.Failure(removeResult.Error);
        }

        // Save changes
        await _writeRepository.UpdateAsync(order, cancellationToken);

        return Result<RemoveOrderItemResult>.Success(new RemoveOrderItemResult(order.Id));
    }
}
