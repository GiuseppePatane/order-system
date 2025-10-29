namespace Product.Application.Commands.ReleaseProductStock;

public record ReleaseProductStockResult(
    Guid ProductId,
    int UpdatedStock,
    int ReleasedQuantity);

