namespace Product.Application.Commands.LockProductStock;

public record LockProductStockCommand(
    Guid ProductId,
    int Quantity);

