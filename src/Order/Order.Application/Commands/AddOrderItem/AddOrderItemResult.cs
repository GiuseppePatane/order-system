namespace Order.Application.Commands.AddOrderItem;

public record AddOrderItemResult(Guid OrderId, Guid ItemId);
