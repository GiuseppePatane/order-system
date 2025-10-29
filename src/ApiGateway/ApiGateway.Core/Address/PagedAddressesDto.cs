namespace ApiGateway.Core.Address;

public class PagedAddressesDto
{
    public required List<AddressDto> Items { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
}
