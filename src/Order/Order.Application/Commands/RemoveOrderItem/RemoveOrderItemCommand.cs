namespace Order.Application.Commands.RemoveOrderItem;

public record RemoveOrderItemCommand(
    Guid OrderId,
    Guid ItemId
);
