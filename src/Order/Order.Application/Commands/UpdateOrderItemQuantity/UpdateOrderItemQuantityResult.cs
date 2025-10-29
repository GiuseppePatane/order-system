namespace Order.Application.Commands.UpdateOrderItemQuantity;

public record UpdateOrderItemQuantityResult(Guid OrderId, Guid ItemId, int NewQuantity);
