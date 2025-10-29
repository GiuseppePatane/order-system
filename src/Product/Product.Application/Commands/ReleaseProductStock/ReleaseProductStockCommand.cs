namespace Product.Application.Commands.ReleaseProductStock;

public record ReleaseProductStockCommand(
    Guid ProductId,
    int Quantity);

