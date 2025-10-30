namespace ApiGateway.Core.Product.Dto;

public record UpdateProductRequestDto
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public double? Price { get; init; }
    public string? Sku { get; init; }
    public string? CategoryId { get; init; }
    
}
