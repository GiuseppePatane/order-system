namespace ApiGateway.Core.Address;

public class DeleteAddressResultDto
{
    public bool Success { get; init; }
    public required string AddressId { get; init; }
}
