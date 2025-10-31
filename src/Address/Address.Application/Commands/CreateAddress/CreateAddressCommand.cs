namespace Address.Application.Commands.CreateAddress;

public record CreateAddressCommand(
    Guid UserId,
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country,
    string? Street2 = null,
    string? Label = null,
    bool IsDefault = false
);
