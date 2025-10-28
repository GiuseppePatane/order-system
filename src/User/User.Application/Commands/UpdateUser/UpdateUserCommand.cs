namespace User.Application.Commands.UpdateUser;

public record UpdateUserCommand(
    Guid UserId,
    string? FirstName,
    string? LastName,
    string? Email);

