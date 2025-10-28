using Shared.Core.Domain.Errors;

namespace Product.Core.Domain.Errors;



/// <summary>
///  Error when the product is not found
/// </summary>
public record ProductNotFoundError(Guid ProductId)
    : NotFoundError("Product", ProductId.ToString());

/// <summary>
/// Error when there is insufficient stock for a product
/// </summary>
public record InsufficientStockError(string ProductName, int Available, int Requested)
    : DomainError("INSUFFICIENT_STOCK", $"Product '{ProductName}' has only {Available} units available, but {Requested} were requested");

/// <summary>
///  Error when the stock goes negative
/// </summary>
public record NegativeStockError(string ProductName)
    : DomainError("NEGATIVE_STOCK", $"Stock for product '{ProductName}' cannot be negative");

/// <summary>
/// Error when the product is not active
/// </summary>
public record ProductNotActiveError(Guid ProductId)
    : DomainError("PRODUCT_NOT_ACTIVE", $"Product with ID {ProductId} is not active");

/// <summary>
/// Error when the category is not found
/// </summary>
public record CategoryNotFoundError(Guid CategoryId)
    : NotFoundError("Category", CategoryId.ToString());

/// <summary>
/// Error when the price is invalid
/// </summary>
public record InvalidPriceError(decimal Price)
    : DomainError("INVALID_PRICE", $"Price {Price} is invalid. Price must be greater than zero");

/// <summary>
/// Errore quando lo SKU è duplicato
/// </summary>
public record DuplicateSkuError(string Sku)
    : DuplicateError("Product", "SKU", Sku);