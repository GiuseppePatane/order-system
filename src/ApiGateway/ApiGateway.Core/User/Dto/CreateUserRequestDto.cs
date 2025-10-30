namespace ApiGateway.Core.User.Dto;

public class CreateUserRequestDto
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
}
