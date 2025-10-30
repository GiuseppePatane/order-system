namespace ApiGateway.Core.Product.Dto;

public class CategoryDto
{
    public required string CategoryId { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
