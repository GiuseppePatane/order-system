using Order.Core.Domain;
using Order.Core.Repositories;
using Shared.Core.Domain.Errors;
using Shared.Core.Domain.Results;

namespace Order.Application.Commands.AddOrderItem;

public class AddOrderItemHandler
{
    private readonly IOrderWriteRepository _writeRepository;
    private readonly IOrderReadOnlyRepository _readRepository;

    public AddOrderItemHandler(
        IOrderWriteRepository writeRepository,
        IOrderReadOnlyRepository readRepository)
    {
        _writeRepository = writeRepository;
        _readRepository = readRepository;
    }

    public async Task<Result<AddOrderItemResult>> Handle(AddOrderItemCommand request, CancellationToken cancellationToken)
    {
       
        var order = await _readRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            return Result<AddOrderItemResult>.Failure(new NotFoundError("Order", request.OrderId.ToString()));
        }
        
        if (order.Status != OrderStatus.Pending)
        {
            return Result<AddOrderItemResult>.Failure(new ValidationError(
                nameof(order.Status),
                "Can only add items to pending orders"));
        }
        
        var itemResult = OrderItem.Create(request.ProductId, request.Quantity, request.UnitPrice);
        if (!itemResult.IsSuccess)
        {
            return Result<AddOrderItemResult>.Failure(itemResult.Error);
        }


        var newItem = itemResult.Value;
        
        var addResult = order.AddItem(newItem);
        if (!addResult.IsSuccess)
        {
            return Result<AddOrderItemResult>.Failure(addResult.Error);
        }

      
        await _writeRepository.UpdateAsync(order, cancellationToken);

        
        return Result<AddOrderItemResult>.Success(new AddOrderItemResult(order.Id, newItem.Id));
    }
}
