using Order.Core.Domain;
using Order.Core.Repositories;
using Shared.Core.Domain.Results;

namespace Order.Application.Commands.CreateOrder;

public class CreateOrderHandler
{
    private readonly IOrderWriteRepository _writeRepository;

    public CreateOrderHandler(IOrderWriteRepository writeRepository)
    {
        _writeRepository = writeRepository;
    }

    public async Task<Result<CreateOrderResult>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Create order items
        var orderItems = new List<OrderItem>();

        foreach (var itemInput in request.Items)
        {
            var itemResult = OrderItem.Create(itemInput.ProductId, itemInput.Quantity, itemInput.UnitPrice);
            if (!itemResult.IsSuccess)
            {
                return Result<CreateOrderResult>.Failure(itemResult.Error!);
            }

            orderItems.Add(itemResult.Value!);
        }

        // Create the order
        var orderResult = Core.Domain.Order.Create(
            request.UserId,
            request.ShippingAddressId,
            orderItems,
            request.BillingAddressId
        );

        if (!orderResult.IsSuccess)
        {
            return Result<CreateOrderResult>.Failure(orderResult.Error!);
        }

        var order = orderResult.Value!;

        // Save the order
        var savedOrder = await _writeRepository.AddAsync(order, cancellationToken);

        return Result<CreateOrderResult>.Success(new CreateOrderResult(savedOrder.Id));
    }
}
