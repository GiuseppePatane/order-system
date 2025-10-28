namespace User.Application.Commands.CreateUser;

public record CreateUserCommand(
    string FirstName,
    string LastName,
    string Email);

