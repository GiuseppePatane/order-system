namespace ApiGateway.Core.Product.Dto;

public class CreateProductRequestDto
{
    public required string Name { get; init; }
    public required string? Description { get; init; }
    public required double Price { get; init; }
    public required int Stock { get; init; }
    public  required string Sku { get; init; }
    public required string CategoryId { get; init; }
}

