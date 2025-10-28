namespace Product.Application.Commands.UpdateProduct;

public record UpdateProductCommand(
    Guid ProductId,
    string? Name,
    string? Description,
    decimal? Price,
    int? Stock,
    string? Sku,
    Guid? CategoryId);

