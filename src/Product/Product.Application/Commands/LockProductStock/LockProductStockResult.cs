namespace Product.Application.Commands.LockProductStock;

public record LockProductStockResult(
    Guid ProductId,
    int UpdatedStock,
    int LockedQuantity,
    decimal LockedPrice
    );

