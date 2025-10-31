namespace ApiGateway.Core.User.Dto;

public record GetUsersRequestDto
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    
}
