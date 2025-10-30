namespace ApiGateway.Core.Address.Dto;

public class CreateAddressRequestDto
{
    public required string UserId { get; init; }
    public required string Street { get; init; }
    public string? Street2 { get; init; }
    public required string City { get; init; }
    public required string State { get; init; }
    public required string PostalCode { get; init; }
    public required string Country { get; init; }
    public string? Label { get; init; }
    public bool IsDefault { get; init; }
}
