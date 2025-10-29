using Shared.Core.Domain.Results;

namespace Address.Application.Commands.DeleteAddress;

public record DeleteAddressCommand(Guid AddressId);
