namespace ApiGateway.Core.Product;

public class ProductDto
{
    public required string ProductId { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public double Price { get; init; }
}