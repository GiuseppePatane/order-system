namespace ApiGateway.Core.Product.Dto;

public class PagedProductsDto
{
    public IReadOnlyList<ProductDto> Items { get; init; } = [];
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }

    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    
}
