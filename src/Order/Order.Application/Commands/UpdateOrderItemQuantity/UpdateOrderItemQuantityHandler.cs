using Order.Core.Repositories;
using Shared.Core.Domain.Errors;
using Shared.Core.Domain.Results;

namespace Order.Application.Commands.UpdateOrderItemQuantity;

public class UpdateOrderItemQuantityHandler
{
    private readonly IOrderWriteRepository _writeRepository;
    private readonly IOrderReadOnlyRepository _readRepository;

    public UpdateOrderItemQuantityHandler(
        IOrderWriteRepository writeRepository,
        IOrderReadOnlyRepository readRepository)
    {
        _writeRepository = writeRepository;
        _readRepository = readRepository;
    }

    public async Task<Result<UpdateOrderItemQuantityResult>> Handle(UpdateOrderItemQuantityCommand request, CancellationToken cancellationToken)
    {
        // Get the existing order
        var order = await _readRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            return Result<UpdateOrderItemQuantityResult>.Failure(new NotFoundError("Order", request.OrderId.ToString()));
        }

        // Validate order is in pending status (only pending orders can be modified)
        if (order.Status != Core.Domain.OrderStatus.Pending)
        {
            return Result<UpdateOrderItemQuantityResult>.Failure(new ValidationError(
                nameof(order.Status),
                "Can only update quantities for pending orders"));
        }

        // Find the item to update
        var item = order.Items.FirstOrDefault(i => i.Id == request.ItemId);
        if (item == null)
        {
            return Result<UpdateOrderItemQuantityResult>.Failure(new NotFoundError("OrderItem", request.ItemId.ToString()));
        }

        // Update the quantity
        var updateResult = item.UpdateQuantity(request.NewQuantity);
        if (!updateResult.IsSuccess)
        {
            return Result<UpdateOrderItemQuantityResult>.Failure(updateResult.Error!);
        }

        // Save changes
        await _writeRepository.UpdateAsync(order, cancellationToken);

        return Result<UpdateOrderItemQuantityResult>.Success(
            new UpdateOrderItemQuantityResult(order.Id, item.Id, request.NewQuantity));
    }
}
