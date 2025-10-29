using Shared.Core.Domain.Results;

namespace Address.Application.Commands.SetDefaultAddress;

public record SetDefaultAddressCommand(Guid AddressId);
