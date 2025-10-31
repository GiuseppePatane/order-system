namespace Address.Application.Commands.UpdateAddress;

public record UpdateAddressCommand(
    Guid AddressId,
    string? Street = null,
    string? Street2 = null,
    string? City = null,
    string? State = null,
    string? PostalCode = null,
    string? Country = null,
    string? Label = null
);
