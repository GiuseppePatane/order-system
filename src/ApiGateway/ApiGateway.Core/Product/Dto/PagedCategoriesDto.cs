namespace ApiGateway.Core.Product.Dto;

public class PagedCategoriesDto
{
    public List<CategoryDto> Items { get; init; } = new();
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
}
