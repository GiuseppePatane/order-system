namespace ApiGateway.Core.Address.Dto;

public record UpdateAddressRequestDto
{
    public string? Street { get; init; }
    public string? Street2 { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
    public string? Label { get; init; }
}
