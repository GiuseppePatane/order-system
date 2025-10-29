namespace Order.Application.Commands.UpdateOrderItemQuantity;

public record UpdateOrderItemQuantityCommand(
    Guid OrderId,
    Guid ItemId,
    int NewQuantity
);
