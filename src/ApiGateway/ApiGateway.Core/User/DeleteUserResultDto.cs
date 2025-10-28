namespace ApiGateway.Core.User;

public class DeleteUserResultDto
{
    public bool Success { get; init; }
    public string? UserId { get; init; }
}
