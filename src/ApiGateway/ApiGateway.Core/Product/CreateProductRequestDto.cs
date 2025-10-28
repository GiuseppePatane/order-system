namespace ApiGateway.Core.Product;

public class CreateProductRequestDto
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public double Price { get; init; }
    public int Stock { get; init; }
    public string Sku { get; init; } = string.Empty;
    public string CategoryId { get; init; } = string.Empty;
}

