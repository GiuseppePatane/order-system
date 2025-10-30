namespace ApiGateway.Core.Product.Dto;

public class GetCategoriesRequestDto
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public bool? IsActive { get; init; }
}
