namespace ApiGateway.Core.Product.Dto;

public record ProductMutationResultDto(string ProductId);

public class ProductDto
{
    public required string ProductId { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public double Price { get; init; }
    public int Stock { get; set; }
    public string Sku { get; set; }
    public string CategoryId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}